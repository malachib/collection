using Fact.Extensions.Experimental;
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

            public override string Name => "Dummy service";

            internal DummyService(IServiceProvider sp) : base(sp, true) { }

            protected override async Task Worker(ServiceContext context)
            {
                await Task.Delay(500);
                context.Progress?.Report(25);
                Offline();
                await Task.Delay(500);
                context.Progress?.Report(50);
                Online();
                Console.WriteLine("Got here");
                // Give parent time to leave degraded state
                await Task.Delay(500);
                context.Progress?.Report(75);
                // Generic signals test to shut down
                Generic?.Invoke();
                // wait a little longer to see rest of events fire
                await Task.Delay(500);
            }

            public override Task Startup(ServiceContext context)
            {
                // because we have online-able, expect to get startup called again
                // but don't reinitialize worker
                if(!IsWorkerCreated)  RunWorker(context);

                return Task.CompletedTask;
            }
        }

        IServiceProvider Setup()
        {
            var sc = new ServiceCollection();
            sc.AddLogging();
            var sp = sc.BuildServiceProvider();
            var lf = sp.GetService<ILoggerFactory>();
            lf.AddConsole(LogLevel.Trace);
            return sp;
        }

        [TestMethod]
        public void BasicServiceManagerTest()
        {
            var sp = Setup();
            var sm = new ServiceManager(sp, "parent");
            var childSm = new ServiceManager(sp, "child");
            var dummyService = new DummyService(sp);
            var dummySem = new SemaphoreSlim(0, 1);

            sm.AddChild(childSm);
            var dummyServiceDescriptor = sm.AddService(dummyService, sp);

            dummyServiceDescriptor.LifecycleStatusUpdated += o =>
            {
                Console.WriteLine($"{DateTime.Now.Millisecond}: dummy = {dummyServiceDescriptor.LifecycleStatus}");
            };

            sm.LifecycleStatusUpdated += o =>
            {
                Console.WriteLine($"{DateTime.Now.Millisecond}: parent: {sm.LifecycleStatus}");
            };

            dummyService.Generic += () => dummySem.Release();

            Assert.AreEqual(LifecycleEnum.Unstarted, sm.LifecycleStatus);

            Task.Run(async () =>
            {
                await sm.Startup(sp);

                Assert.AreEqual(LifecycleEnum.Running, sm.LifecycleStatus);

                childSm.LifecycleStatus = LifecycleEnum.Error;

                Assert.AreEqual(LifecycleEnum.Degraded, sm.LifecycleStatus);

                childSm.LifecycleStatus = LifecycleEnum.Running;

                await dummySem.WaitAsync(10000);

                // FIX: Something is wrong, dummy service is claiming worker
                // is awaiting activation when we reach here
                await sm.Shutdown();

            }).Wait();

        }

        [TestMethod]
        public void AsyncContextTest()
        {
            var progress = new Progress<float>();

            progress.ProgressChanged += (o, value) => Console.WriteLine($"Progress %: {value}");
            ServiceContext context = new ServiceContext(Setup(), progress);

            var sp = context.ServiceProvider;

            var sm = new ServiceManager(sp, "parent");
            var childSm = new ServiceManager(sp, "child");
            var dummyService = new DummyService(sp);
            var dummySem = new SemaphoreSlim(0, 1);

            sm.AddChild(childSm);
            sm.AddService(dummyService, sp);

            dummyService.Generic += () => dummySem.Release();

            Task.Run(async () =>
            {
                await sm.Startup(context);

                await dummySem.WaitAsync();

                await sm.Shutdown(context);
            }).Wait(3000);
        }
    }
}
