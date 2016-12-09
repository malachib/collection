using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Fact.Extensions.Caching;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

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


        [TestMethod]
        public void SlidingExpiryTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMemoryCache();
            var provider = serviceCollection.BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            var cache = new MemoryCacheBag(memoryCache);
            cache.Set("test", "test1", TimeSpan.FromSeconds(0.5));
            var result = cache.Get<string>("test");
            Assert.AreEqual("test1", result);
            Assert.IsTrue(cache.TryGet("test", out result));
            Assert.AreEqual("test1", result);
            Thread.Sleep(500);
            Assert.IsFalse(cache.TryGet("test", out result));
            Assert.AreEqual(null, result);
        }
    }
}
