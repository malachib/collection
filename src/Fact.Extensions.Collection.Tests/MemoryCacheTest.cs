using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Fact.Extensions.Collection.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace Fact.Extensions.Collection.Tests
{
    [TestClass]
    public class MemoryCacheTest
    {
        [TestMethod]
        public void BasicTest()
        {
            var memoryCacheOptions = new MemoryCacheOptions();
            var memoryCache = new MemoryCache(memoryCacheOptions);
            var memoryCacheBag = new MemoryCacheBag(null, memoryCache);

            memoryCacheBag.Set("test", "test value", typeof(string));
            var result = memoryCacheBag.Get("test", typeof(string));

        }
    }
}
