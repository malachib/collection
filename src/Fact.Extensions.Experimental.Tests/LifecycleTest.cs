using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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


        class DummyService : LifecycleDescriptorBase, IServiceDescriptor2, IService
        {
            public IService Service => this;

            public string Name => "dummy service";

            public Task Shutdown()
            {
                return Task.CompletedTask;
            }

            public async Task Startup(IServiceProvider serviceProvider)
            {
                await Task.Delay(500);
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

            sm.AddService(childSm);
            sm.AddService(child2Sm);

            Assert.AreEqual(LifecycleEnum.Unstarted, sm.LifecycleStatus);

            Task.Run(async () =>
            {
                await sm.Startup(sp);
            }).Wait();

            Assert.AreEqual(LifecycleEnum.Running, sm.LifecycleStatus);

            childSm.LifecycleStatus = LifecycleEnum.Error;

            Assert.AreEqual(LifecycleEnum.Degraded, sm.LifecycleStatus);
        }
    }
}
