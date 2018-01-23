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
    public abstract class WorkerServiceBase : IService
    {
        readonly ILogger logger;
        string name;

        public string Name => name;
        readonly bool oneShot;

        // this is for asynchronous pre-startup initialization, manually assigned from
        // a constructor
        protected Task configure;


        WorkerServiceBase(IServiceProvider sp, string name)
        {
            logger = sp.GetService<ILogger<WorkerServiceBase>>();
            this.name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">FIX: Not active yet</param>
        protected WorkerServiceBase(IServiceProvider sp, string name, CancellationToken ct, bool oneShot = false)
            : this(sp, name)
        {
            this.ct = ct;
            this.oneShot = oneShot;
            localCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">FIX: Not active yet</param>
        protected WorkerServiceBase(IServiceProvider sp, string name, bool oneShot = false) : this(sp, name)
        {
            this.oneShot = oneShot;
            localCts = new CancellationTokenSource();
        }

        // FIX: making protected to handle IOnlineEvents class services, but
        // I think we can do better
        protected Task worker;
        protected readonly CancellationTokenSource localCts;
        readonly CancellationToken ct;

        // TODO: Decide if we want to keep passing IServiceProvider in, thinking probably
        // yes but let's see how it goes
        protected abstract Task Worker(CancellationToken cts);


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

        protected async Task RunWorker()
        {
            logger.LogTrace($"Worker starting ({Name})");

            do
            {
                worker = Worker(localCts.Token);
                await worker;
            }
            while (!oneShot && !localCts.IsCancellationRequested);

            logger.LogTrace($"Worker has finished: ({Name}) - {worker.Status}");
        }

        /// <summary>
        /// Ascertain whether a worker has even been created
        /// </summary>
        public bool IsWorkerCreated => worker != null;

        public async Task Shutdown()
        {
            // TODO: Do threadsafe stuff
            if (IsWorkerCreated)
            {
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
        public virtual async Task Startup(IServiceProvider serviceProvider)
        {
            if(configure != null)
            {
                await configure;
            }

            // we specifically *do not* await here, we are starting up a worker thread
            RunWorker().ContinueWithErrorLogger(serviceProvider, Name);
        }
    }
}
