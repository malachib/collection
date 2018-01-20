using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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


        [TestMethod]
        public void BasicServiceManagerTest()
        {
            var sc = new ServiceCollection();
            var sp = sc.BuildServiceProvider();
            var sm = new ServiceManager("parent");
            var childSm = new ServiceManager("child");

            sm.AddService(childSm);

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
