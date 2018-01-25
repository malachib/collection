using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Fact.Extensions.Experimental;

namespace Fact.Extensions.Services
{
    public abstract class WorkerServiceBase : 
        IService,
        IExceptionEventProvider
    {
        readonly ILogger logger;

        public abstract string Name { get; }
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


        WorkerServiceBase(IServiceProvider sp)
        {
            logger = sp.GetService<ILogger<WorkerServiceBase>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">FIX: Not active yet</param>
        protected WorkerServiceBase(ServiceContext context, bool oneShot = false)
            : this(context.ServiceProvider)
        {
            this.ct = context.CancellationToken;
            this.oneShot = oneShot;
            localCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            // TODO: Wrap up configure with proper error event firing mechanism instead
            // (using IExceptionProvider)
            configure = Configure(context).ContinueWithErrorLogger(logger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">FIX: Not active yet</param>
        protected WorkerServiceBase(IServiceProvider sp, bool oneShot = false) : this(sp)
        {
            this.oneShot = oneShot;
            localCts = new CancellationTokenSource();
        }

        Task worker;

        protected readonly CancellationTokenSource localCts;
        readonly CancellationToken ct;

        public event Action<Exception> ExceptionOccurred;

        // TODO: Decide if we want to keep passing IServiceProvider in, thinking probably
        // yes but let's see how it goes
        protected abstract Task Worker(ServiceContext context);


        /// <summary>
        /// Combine incoming cancellation token with our local shutdown-oriented one
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        /*
        protected CancellationToken Combine(CancellationToken ct)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(localCts.Token, ct).Token;
        } */


        /// <summary>
        /// Utilizing our own local cancellation token, initiate a Task cancel operation
        /// </summary>
        protected void Cancel()
        {
            localCts.Cancel();
        }

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

            context = new ServiceContext(context, context.Descriptor);
            context.CancellationToken = localCts.Token;

            do
            {
                // TODO: Work out how progress.report is gonna work with repeating/non one
                // shot services, if any
                worker = Worker(context);
                await worker;
            }
            while (!oneShot && !localCts.IsCancellationRequested);

            // If not oneshot, then it was a cancellation request
            // NOTE: Untested, might just blast right by it with an exception
            if(!oneShot)
            {
                // if our outsider cancellation token was NOT the instigator,
                // then we expect localCts itself generated it
                if(!ct.IsCancellationRequested)
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


        void RunWorkerHelper(ServiceContext context)
        {
            // we specifically *do not* await here, we are starting up a worker thread
            Task.Run(async () =>
            {
                try
                {
                    await RunWorker(context);
                }
                catch (TaskCanceledException)
                {
                    logger.LogDebug($"Worker: ({Name}) cancelled normally");
                }
                catch (OperationCanceledException)
                {
                    logger.LogDebug($"Worker: ({Name}) cancelled forcefully");
                }
                catch (Exception ex)
                {
                    ExceptionOccurred?.Invoke(ex);
                    logger.LogWarning(0, ex, $"Worker: ({Name}) has error");
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

                localCts.Cancel();
                await worker;
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

            RunWorkerHelper(context);

            // TODO: have mini-awaiter which only waits for runworker to start
            context.Progress?.Report(100);
        }
    }
}
