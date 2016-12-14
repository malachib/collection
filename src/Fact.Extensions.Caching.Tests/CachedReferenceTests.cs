using Fact.Extensions.Serialization.Newtonsoft;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Caching.Tests
{
    [TestClass]
    public class CachedReferenceTests
    {
        public class TestCachedItem
        {
            public DateTime TimeStamp { get; set; } = DateTime.Now;

            public string Desc { get; set; }
        }


        [TestMethod]
        public void CachedReferenceAsyncTest()
        {
            int descConter = 1;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDistributedMemoryCache();
            var provider = serviceCollection.BuildServiceProvider();
            var dcache = provider.GetService<IDistributedCache>();
            var cache = new DistributedCacheBag(dcache, new JsonSerializationManager());
            TestCachedItem option;
            Func<Task<TestCachedItem>> factory = async delegate
            {
                return await Task.FromResult(new TestCachedItem { Desc = "desc " + descConter++ });
            };
            var cr2 = cache.ReferenceAsync(factory);
            option = cr2.GetValue().Result;
            var cr3 = cache.ReferenceAsync(factory,
                new SlidingTimeExpiration(TimeSpan.FromSeconds(0.5)));
            option = cr3.GetValue().Result;
            Assert.AreEqual(option.TimeStamp, cr3.GetValue().Result.TimeStamp);
            Assert.AreEqual(option.Desc, cr3.GetValue().Result.Desc);
            Thread.Sleep(500);
            Assert.AreNotEqual(option.TimeStamp, cr3.GetValue().Result.TimeStamp);
            Assert.AreEqual("desc 3", cr3.GetValue().Result.Desc);
        }


        [TestMethod]
        public void CachedReferenceAsyncAssignerTest()
        {
            int descConter = 1;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDistributedMemoryCache();
            var provider = serviceCollection.BuildServiceProvider();
            var dcache = provider.GetService<IDistributedCache>();
            var cache = new DistributedCacheBag(dcache, new JsonSerializationManager());
            TestCachedItem option;
            Func<Task<TestCachedItem>> factory = async delegate
            {
                return await Task.FromResult(new TestCachedItem { Desc = "desc " + descConter++ });
            };
            var cr = cache.ReferenceAsync(factory);
            option = cr;
            cr.Clear().Wait();
            option = cr;
            cr.Value = option;
        }
    }
}
