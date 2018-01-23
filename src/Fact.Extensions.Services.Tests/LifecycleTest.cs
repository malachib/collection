using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services.Tests
{
    [TestClass]
    public class LifecycleTest
    {
        [TestMethod]
        public void BasicLifecycleTest()
        {
            var sc = new ServiceCollection();
            var sp = sc.BuildServiceProvider();
            var lm = new LifecycleManager(sp);
        }


        class DummyService : WorkerServiceBase, IOnlineEvents
        {
            public event Action Offline;
            public event Action Online;
            public event Action Generic;

            internal DummyService(IServiceProvider sp) : base(sp, "dummy service", new CancellationTokenSource().Token, true) { }

            protected override async Task Worker(CancellationToken ct)
            {
                await Task.Delay(500);
                Offline();
                await Task.Delay(500);
                Online();
                Console.WriteLine("Got here");
                // Give parent time to leave degraded state
                await Task.Delay(500);
                // Generic signals test to shut down
                Generic?.Invoke();
                // wait a little longer to see rest of events fire
                await Task.Delay(500);
            }

            public override Task Startup(IServiceProvider serviceProvider)
            {
                // because we have online-able, expect to get startup called again
                // but don't reinitialize worker
                if(!IsWorkerCreated)  RunWorker();

                return Task.CompletedTask;
            }
        }

        [TestMethod]
        public void BasicServiceManagerTest()
        {
            var sc = new ServiceCollection();
            sc.AddLogging();
            var sp = sc.BuildServiceProvider();
            var lf = sp.GetService<ILoggerFactory>();
            lf.AddConsole(LogLevel.Trace);
            var sm = new ServiceManager(sp, "parent");
            var childSm = new ServiceManager(sp, "child");
            var child2Sm = new DummyService(sp);
            var sem = new SemaphoreSlim(0, 1);

            sm.AddChild(childSm);
            var dummyServiceDescriptor = sm.AddService(child2Sm, sp);

            dummyServiceDescriptor.LifecycleStatusUpdated += o =>
            {
                Console.WriteLine($"{DateTime.Now.Millisecond}: dummy = {dummyServiceDescriptor.LifecycleStatus}");
            };

            sm.LifecycleStatusUpdated += o =>
            {
                Console.WriteLine($"{DateTime.Now.Millisecond}: parent: {sm.LifecycleStatus}");
            };

            child2Sm.Generic += () => sem.Release();

            Assert.AreEqual(LifecycleEnum.Unstarted, sm.LifecycleStatus);

            Task.Run(async () =>
            {
                await sm.Startup(sp);

                Assert.AreEqual(LifecycleEnum.Running, sm.LifecycleStatus);

                childSm.LifecycleStatus = LifecycleEnum.Error;

                Assert.AreEqual(LifecycleEnum.Degraded, sm.LifecycleStatus);

                childSm.LifecycleStatus = LifecycleEnum.Running;

                await sem.WaitAsync(10000);

                await sm.Shutdown();

            }).Wait();

        }
    }
}
