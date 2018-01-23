using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
    public abstract class WorkerServiceBase : IService
    {
        string name;

        public string Name => name;
        readonly bool oneShot;

        // this is for asynchronous pre-startup initialization, manually assigned from
        // a constructor
        protected Task configure;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">FIX: Not active yet</param>
        protected WorkerServiceBase(ILogger logger, string name, CancellationToken ct, bool oneShot = false)
        {
            this.ct = ct;
            this.name = name;
            this.oneShot = oneShot;
            localCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">FIX: Not active yet</param>
        protected WorkerServiceBase(ILogger logger, string name, bool oneShot = false)
        {
            this.name = name;
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
            do
            {
                worker = Worker(localCts.Token);
                await worker;
            }
            while (!oneShot && !localCts.IsCancellationRequested);
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
                if (worker.Status == TaskStatus.Created || worker.Status == TaskStatus.Running)
                {
                    localCts.Cancel();
                    await worker;
                }
                // TODO: log why we couldn't do a regular shutdown
            }
        }

        // FIX: would use "completedTask" but it doesn't seem to be available for netstandard1.1?
        public virtual async Task Startup(IServiceProvider serviceProvider)
        {
            if(configure != null)
            {
                await configure;
            }

            // we specifically *do not* await here, we are starting up a worker thread
            RunWorker();
        }
    }
}
