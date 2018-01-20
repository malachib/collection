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
    }
}
