using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Fact.Extensions.Caching;
using Microsoft.Extensions.DependencyInjection;

using Fact.Extensions.Collection;

namespace Fact.Extensions.Collection.Tests
{
    [TestClass]
    public class GetterTest
    {
        [TestMethod]
        public void BasicTest()
        {
            var dict = new Dictionary<string, object>();
            var bag = dict.ToBag();
        }
    }
}
