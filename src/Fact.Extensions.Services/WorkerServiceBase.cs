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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">FIX: Not active yet</param>
        protected WorkerServiceBase(string name, CancellationToken ct, bool oneShot = false)
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
        protected WorkerServiceBase(string name, bool oneShot = false)
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

        protected async Task RunWorker()
        {
            do
            {
                worker = Worker(localCts.Token);
                await worker;
            }
            while (!oneShot && !localCts.IsCancellationRequested);
        }

        public bool IsWorkerRunning => worker != null;

        public async Task Shutdown()
        {
            localCts.Cancel();
            await worker;
        }

        // FIX: would use "completedTask" but it doesn't seem to be available for netstandard1.1?
        public virtual async Task Startup(IServiceProvider serviceProvider)
        {
            // we specifically *do not* await here, we are starting up a worker thread
            RunWorker();
        }
    }
}
