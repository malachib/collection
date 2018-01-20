using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Experimental.Tests
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


        class DummyService : IService, IOnlineEvents
        {
            public IService Service => this;

            public string Name => "dummy service";

            public event Action Offline;
            public event Action Online;
            public event Action Generic;

            Task worker;

            public async Task Shutdown()
            {
                await worker;
            }

            async Task _worker()
            {
                await Task.Delay(500);
                Offline();
                await Task.Delay(500);
                Online();
                Console.WriteLine("Got here");
                Generic?.Invoke();
                await Task.Delay(500);
            }

            public async Task Startup(IServiceProvider serviceProvider)
            {
                // because we have online-able, expect to get startup called again
                // but don't reinitialize worker
                if(worker == null)
                    worker = _worker();
            }
        }

        [TestMethod]
        public void BasicServiceManagerTest()
        {
            var sc = new ServiceCollection();
            var sp = sc.BuildServiceProvider();
            var sm = new ServiceManager("parent");
            var childSm = new ServiceManager("child");
            var child2Sm = new DummyService();
            var sem = new SemaphoreSlim(0, 1);

            sm.AddService(childSm);
            var dummyServiceDescriptor = sm.AddService2(child2Sm);

            dummyServiceDescriptor.LifecycleStatusUpdated += o =>
            {
                Console.WriteLine($"dummy = {dummyServiceDescriptor.LifecycleStatus}");
            };

            sm.LifecycleStatusUpdated += o =>
            {
                Console.WriteLine($"parent: {sm.LifecycleStatus}");
            };

            child2Sm.Generic += () => sem.Release();

            Assert.AreEqual(LifecycleEnum.Unstarted, sm.LifecycleStatus);

            Task.Run(async () =>
            {
                await sm.Startup(sp);

                Assert.AreEqual(LifecycleEnum.Running, sm.LifecycleStatus);

                childSm.LifecycleStatus = LifecycleEnum.Error;

                Assert.AreEqual(LifecycleEnum.Degraded, sm.LifecycleStatus);

                await sem.WaitAsync(10000);

                await sm.Shutdown();

            }).Wait();

        }
    }
}
