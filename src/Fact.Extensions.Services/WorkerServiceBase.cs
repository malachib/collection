using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Fact.Extensions.Experimental;
using Fact.Extensions.Collection;

namespace Fact.Extensions.Services
{
    public abstract class WorkerServiceBase : 
        Experimental.Cancellable,
        IService,
        IExceptionEventProvider
    {
        readonly ILogger logger;

        /// <summary>
        /// Reflects WorkerService-specific states
        /// </summary>
        public enum StateEnum
        {
            /// <summary>
            /// Beginning one iteration of a worker cycle
            /// </summary>
            Starting,
            /// <summary>
            /// In flight running of the worker cycle
            /// </summary>
            Running,
            /// <summary>
            /// Completed worker cycle, state represents
            /// end of a worker cycle regardless of it
            /// being due to error, one shot, etc.
            /// does NOT represent end of worker daemon
            /// overall
            /// </summary>
            Ended
        }

        /// <summary>
        /// Worker Service specific states, all happen within the context
        /// of "Running" lifecycle
        /// </summary>
        protected State<StateEnum> state = new State<StateEnum>();

        public abstract string Name { get; }

        /// <summary>
        /// Indicates whether we intend to loop this worker
        /// </summary>
        readonly bool oneShot;

        // this is for asynchronous pre-startup initialization, manually assigned from
        // a constructor
        Task configure;

        /// <summary>
        /// Override this if pre-startup/semi-long-running config needs to happen
        /// Will be kicked off by constructor
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual async Task Configure(ServiceContext context) { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">global service context</param>
        /// <param name="oneShot">Do not loop, run worker only once</param>
        protected WorkerServiceBase(ServiceContext context, bool oneShot = false)
            : base(context.CancellationToken)
        {
            logger = context.ServiceProvider.GetRequiredService<ILogger<WorkerServiceBase>>();
            this.oneShot = oneShot;
            // TODO: Wrap up configure with proper error event firing mechanism instead
            // (using IExceptionProvider)
            configure = Configure(context).ContinueWithErrorLogger(logger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">Do not loop, run worker only once</param>
        protected WorkerServiceBase(IServiceProvider sp, bool oneShot = false)
        {
            logger = sp.GetRequiredService<ILogger<WorkerServiceBase>>();
            this.oneShot = oneShot;
        }

        /// <summary>
        /// Refers to to ever-changing worker task handling per-loop
        /// iterative work.  Does not represent unchanging daemon task itself
        /// </summary>
        Task worker;

        /// <summary>
        /// Refers to the non-changing server/daemon task which kicks
        /// off workers one by one
        /// </summary>
        Task daemon;

        /// <summary>
        /// Exception occurred within worker process itself
        /// </summary>
        public event Action<Exception> ExceptionOccurred;

        protected abstract Task Worker(ServiceContext context);


        /// <summary>
        /// Runs the custom worker process
        /// Note that inbound context will have its cancellation token augmented with this
        /// worker service local CTS
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected async Task RunWorker(ServiceContext context)
        {
            logger.LogTrace($"Worker starting ({Name})");

            // replace incoming context (from Startup) with our own
            // local context
            context = new ServiceContext(context);
            // Our local context is changed only in that we have our own cancellation
            // token
            context.CancellationToken = Token;

            do
            {
                state.Value = StateEnum.Starting;

                try
                {
                    // TODO: Work out how progress.report is gonna work with repeating/non one
                    // shot services, if any
                    worker = Worker(context);

                    state.Value = StateEnum.Running;

                    await worker;
                }
                catch (TaskCanceledException)
                {
                    // We fully expect to encounter this exception due to external shutdown
                    // and/or Cancel() calls
                    logger.LogDebug($"Worker: ({Name}) cancelling normally");
                }
                catch (OperationCanceledException)
                {
                    // We fully expect to encounter this exception due to external shutdown
                    // and/or Cancel() calls
                    logger.LogDebug($"Worker: ({Name}) cancelling forcefully");
                }
                catch (AggregateException aex)
                {
                    if (aex.InnerException is OperationCanceledException)
                    {
                        logger.LogDebug($"Worker: ({Name}) cancelling normally (via aggregate exception)");
                    }
                    else
                        throw;
                }
                finally
                {
                    state.Value = StateEnum.Ended;
                }
            }
            while (!oneShot && !Token.IsCancellationRequested);

            // If not oneshot, then it was a cancellation request
            // NOTE: Untested, might just blast right by it with an exception
            if(!oneShot)
            {
                // if our outsider cancellation token was NOT the instigator,
                // then we expect localCts itself generated it
                if(Token.IsCancellationRequested)
                {
                    logger.LogTrace($"Worker has finished: ({Name}) Shutdown normally");
                }
                else
                {
                    logger.LogWarning($"Worker has finished: ({Name}) Shutdown forcefully from system");
                }
            }
            else
                logger.LogTrace($"Worker has finished: ({Name}) - {worker.Status}");
        }


        Task RunWorkerDaemon(ServiceContext context)
        {
            // we specifically *do not* await here, we are starting up a worker thread
            return Task.Run(async () =>
            {
                try
                {
                    await RunWorker(context);
                }
                // Standalone exception handler, primarily for servicing exceptions which occur that:
                // a) aren't cancel-inspired exceptions
                // b) (mostly) occur between (exclusively) the space of a startup and shutdown
                catch (Exception ex)
                {
                    ExceptionOccurred?.Invoke(ex);
                    logger.LogWarning(0, ex, $"Worker: ({Name}) has error and is aborting");
                }
            });
        }

        /// <summary>
        /// Ascertain whether a worker has even been created
        /// </summary>
        public bool IsWorkerCreated => worker != null;

        public async virtual Task Shutdown(ServiceContext context)
        {
            // TODO: Do threadsafe stuff
            if (IsWorkerCreated)
            {
                if (worker.Status == TaskStatus.Faulted)
                {
                    logger.LogWarning($"Shutdown: Worker was faulted.  Error should appear earlier in logs.  Message = {worker.Exception.InnerException.Message}");
                    return;
                }

                // because of https://stackoverflow.com/questions/20830998/async-always-waitingforactivation
                // these async/await tasks don't get normal statuses
                if (worker.Status != TaskStatus.Created && 
                    worker.Status != TaskStatus.Running &&
                    worker.Status != TaskStatus.WaitingForActivation)
                    logger.LogWarning($"Shutdown: No worker was running ({Name}).  Cancel initiated anyway.  Status = {worker.Status}");

                Cancel();
                context.Progress?.Report(50);
                await daemon;
            }
            else
                logger.LogWarning($"Shutdown: No worker was created before shutdown was called ({Name})");
        }

        // FIX: would use "completedTask" but it doesn't seem to be available for netstandard1.1?
        public virtual async Task Startup(ServiceContext context)
        {
            context.Progress?.Report(0);

            if (configure != null)
            {
                await configure;
            }

            context.Progress?.Report(50);

            daemon = RunWorkerDaemon(context);

            // TODO: have mini-awaiter which only waits for runworker to start
            context.Progress?.Report(100);
        }
    }
}
