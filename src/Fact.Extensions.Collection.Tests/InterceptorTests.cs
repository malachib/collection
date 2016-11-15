using Fact.Extensions.Caching;
using Fact.Extensions.Collection.Interceptor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection.Tests
{
    [TestClass]
    public class InterceptorTests
    {
        interface IService
        {
            int ReturnSeven();
        }

        public class Service : IService
        {
            [OperationCache]
            public int ReturnSeven() { return 7; }
        }

        [TestMethod]
        public void CacheInterceptorTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMemoryCache();
            var provider = serviceCollection.BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();

            var cacheIndexer = new MemoryCacheIndexer(null, memoryCache);
            var cache = cacheIndexer.ToNamedBag();
            var service = CacheInterceptor.Intercept<IService>(new Service(), cache);

            var value = service.ReturnSeven();
        }
    }
}
