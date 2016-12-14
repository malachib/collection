using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Fact.Extensions.Caching;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;
using Fact.Extensions.Serialization;
using System.IO;

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
            var memoryCacheIndexer = new MemoryCacheIndexer(memoryCache);
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
            Assert.IsFalse(cache.TryGet("test", out result), "Key 'test' should have expired by now");
            Assert.AreEqual(null, result);
        }


        public class TestCachedItem
        {
            public readonly DateTime TimeStamp = DateTime.Now;

            public string Desc { get; set; }
        }


        [TestMethod]
        public void CachedReferenceTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMemoryCache();
            var provider = serviceCollection.BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            var cache = new MemoryCacheBag(memoryCache);
            TestCachedItem option;
            var cr2 = cache.Reference(() => new TestCachedItem { Desc = "desc 1" });
            option = cr2;
            var cr3 = cache.Reference(() => new TestCachedItem { Desc = "desc 2" }, 
                new SlidingTimeExpiration(TimeSpan.FromSeconds(0.5)));
            option = cr3;
            Assert.AreEqual(option.TimeStamp, cr3.Value.TimeStamp);
            Assert.AreEqual(option.Desc, cr3.Value.Desc);
            Thread.Sleep(500);
            Assert.AreNotEqual(option.TimeStamp, cr3.Value.TimeStamp);
            Assert.AreEqual(option.Desc, cr3.Value.Desc);
        }
    }
}
