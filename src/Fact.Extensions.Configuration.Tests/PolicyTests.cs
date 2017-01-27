using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Configuration.Tests
{
    [TestClass]
    public class PolicyTests
    {
        [Alias("fact.policy.synthetic")]
        public class SyntheticPolicy : IPolicy
        {

        }

        [TestMethod]
        public void Policy1Test()
        {
            var pp = new PolicyProvider();

            var p = pp.GetPolicy<SyntheticPolicy>();
        }
    }
}
