using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var sm = new ServiceManager();
            var childSm = new ServiceManager();

            sm.AddService(childSm);

            //Assert.AreEqual(LifecycleEnum.Running, sm.LifecycleStatus);

            childSm.SetState(LifecycleEnum.Error);

            Assert.AreEqual(LifecycleEnum.Degraded, sm.LifecycleStatus);
        }
    }
}
