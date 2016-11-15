using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Fact.Extensions.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace Fact.Extensions.Collection.Tests
{
    [TestClass]
    public class MemoryCacheTest
    {
        [TestMethod]
        public void BasicTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMemoryCache();
            var provider = serviceCollection.BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            var memoryCacheIndexer = new MemoryCacheIndexer(null, memoryCache);
            var memoryCacheBag = memoryCacheIndexer.ToBag();
            var key = "test";
            var value = "test value";

            memoryCacheBag.Set(key, value);
            var result = memoryCacheBag.Get<string>(key);
            Assert.AreEqual(value, result);
        }
    }
}
