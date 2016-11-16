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
        public interface IService : ICacheableService
        {
            [OperationCache]
            int ReturnValue();

            [OperationCache(Notify = true, Cache = false)]
            void ClearToZero();
        }

        public class Service : IService
        {
            int value = 7;

            public int ReturnValue() { return value; }

            public void ClearToZero() { value = 0; }
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
            //var service = CacheInterceptor.Intercept<IService>(new Service(), cache);
            // TODO: Make a fluent interface for this, ala:
            /* *
             * new Service().AsCached<IService>(cache).
             *      OnMethodCall(nameof(IService.ClearToZero), 
             *          (interceptor, invoker) => interceptor.RemoveCachedMethod(nameof(IService.ReturnValue))).
             *          Build();
             * 
             */
            CacheInterceptor interceptor;
            var service = new Service().AsCached<IService>(cache, out interceptor);
            interceptor.MethodCalling += Interceptor_MethodCalling;

            var value = service.ReturnValue();
            var value1 = service.ReturnValue();

            Assert.AreEqual(value1, value);

            service.ClearToZero();
            value = service.ReturnValue();

            Assert.AreEqual(0, value);
        }

        private void Interceptor_MethodCalling(CacheInterceptor interceptor, Castle.DynamicProxy.IInvocation invocation)
        {
            if (invocation.Method.Name == nameof(IService.ClearToZero))
            {
                interceptor.RemoveCachedMethod(nameof(IService.ReturnValue));
            }
        }
    }
}
