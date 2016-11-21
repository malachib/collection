using Fact.Extensions.Caching;
using Fact.Extensions.Collection.Interceptor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public interface IService2
        {
            int ReturnValue();

            void ClearToZero();
        }

        public class Service : IService, IService2
        {
            int value = 7;

            public int ReturnValue() { return value; }

            public void ClearToZero() { value = 0; }

            internal int CheatChangeValue { set { this.value = value; } }
        }

        [TestMethod]
        private void Interceptor_MethodCalling(CacheInterceptor interceptor, Castle.DynamicProxy.IInvocation invocation)
        {
            if (invocation.Method.Name == nameof(IService.ClearToZero))
            {
                interceptor.RemoveCachedMethod(nameof(IService.ReturnValue));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///  Old comment:    // TODO: Make a fluent interface for this, ala:
            /* *
             * new Service().AsCached<IService>(cache).
             *      OnMethodCall(nameof(IService.ClearToZero), 
             *          (interceptor, invoker) => interceptor.RemoveCachedMethod(nameof(IService.ReturnValue))).
             *          Build();
             * 
             */

        /// </remarks>
        [TestMethod]
        public void CacheInterceptorDeclarativeTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddCachedSingleton<IService, Service>(ci =>
            {
                ci.MethodCalling += Interceptor_MethodCalling;
            });

            var provider = serviceCollection.BuildServiceProvider();
            var service = provider.GetService<IService>();
            var rawService = provider.GetService<Service>(); // since it's a singleton, we can do this

            var value = service.ReturnValue();
            rawService.CheatChangeValue = 1;
            var value1 = service.ReturnValue();

            Assert.AreEqual(value1, value);

            service.ClearToZero();
            value = service.ReturnValue();

            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void CacheInterceptorImperativeTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddCachedSingleton<IService2, Service>(
                ci =>
                {
                    var builder = ci.GetBuilder();
                    builder.For(nameof(IService.ReturnValue)).
                        Cache().
                        OnCall(nameof(IService.ClearToZero)).
                            Notify(oc =>
                            {
                                oc.ClearKey();
                            });
                });

            var provider = serviceCollection.BuildServiceProvider();
            var service = provider.GetService<IService2>();
            var rawService = provider.GetService<Service>(); // since it's a singleton, we can do this

            var value = service.ReturnValue();
            rawService.CheatChangeValue = 1;
            var value1 = service.ReturnValue();

            Assert.AreEqual(value1, value);

            service.ClearToZero();
            value = service.ReturnValue();

            Assert.AreEqual(0, value);
        }
    }
}
