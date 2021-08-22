using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Services.Synthetic
{
    /// <summary>
    /// Dummy service which goes offline then back online
    /// </summary>
    public class DummyService : WorkerServiceBase, IOnlineEvents
    {
        public event Action Offline;
        public event Action Online;
        public event Action Generic;

        public override string Name => "Dummy service";

        public DummyService(IServiceProvider sp) : base(sp, true) { }

        protected override async Task Worker(ServiceContext context)
        {
            var ct = context.CancellationToken;
            await Task.Delay(500, ct);
            context.Progress?.Report(25);
            Offline();
            await Task.Delay(500, ct);
            context.Progress?.Report(50);
            Online();
#if NETSTANDARD2_0_OR_GREATER || NET46_OR_GREATER
            Console.WriteLine("Got here");
#endif
            // Give parent time to leave degraded state
            await Task.Delay(500, ct);
            context.Progress?.Report(75);
            // Generic signals test to shut down
            Generic?.Invoke();
            // wait a little longer to see rest of events fire
            await Task.Delay(500, ct);
        }

        public override async Task Startup(ServiceContext context)
        {
            // because we have online-able, expect to get startup called again
            // but don't reinitialize worker
            // DEBT: Might want to move this into WorkerServiceBase
            if (context.Descriptor.LifecycleStatus != LifecycleEnum.Online)
                await base.Startup(context);
        }
    }

}
