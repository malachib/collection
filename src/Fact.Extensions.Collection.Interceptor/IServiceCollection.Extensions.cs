using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Fact.Extensions.Caching;
using Fact.Extensions.Collection.Interceptor;

namespace Fact.Extensions.Collection
{
    public static class IServiceCollection_Extensions
    {
        /// <summary>
        /// Add a cached singleton with the cache provider of your choosing.  It must be convertible
        /// to an IBag via the <paramref name="configureBag"/> delegate
        /// </summary>
        /// <typeparam name="TCacheService"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="configureBag">Create and configure IBag from the provided <typeparamref name="TCacheService"/> </param>
        /// <param name="configureBuilder">Configure CacheInterceptor</param>
        public static void AddCachedSingleton<TCacheService>(this IServiceCollection serviceCollection, 
            Type serviceType, 
            Type implementationType, 
            Func<TCacheService, IBag> configureBag, 
            Action<CacheInterceptor.Builder> configureBuilder = null)
        {
            // Use DI provider to hold on to raw implementation singleton, so that
            // its constructor is called properly
            serviceCollection.AddSingleton(implementationType);

            var sd = new ServiceDescriptor(serviceType, serviceProvider =>
            {
                var serviceInstance = serviceProvider.GetRequiredService(implementationType);
                var cache = serviceProvider.GetRequiredService<TCacheService>();
                // convert native cache & configure it special for this scenario
                var cacheBag = configureBag(cache);
                var cacheInterceptor = new CacheInterceptor(cacheBag, serviceType);
                configureBuilder?.Invoke(cacheInterceptor.GetBuilder());
                var proxy = AssemblyGlobal.Proxy.CreateInterfaceProxyWithTarget(
                    serviceType,
                    serviceInstance,
                    cacheInterceptor);
                return proxy;
            }, ServiceLifetime.Singleton);
            serviceCollection.Add(sd);
        }


        /// <summary>
        /// Caches this singleton with the built-in .NET Core IMemoryCache
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="configureBuilder"></param>
        public static void AddCachedSingleton(this IServiceCollection serviceCollection, Type serviceType, Type implementationType, Action<CacheInterceptor.Builder> configureBuilder = null)
        {
            serviceCollection.AddCachedSingleton<IMemoryCache>(
                serviceType,
                implementationType, 
                memoryCache =>
                {
                    var memoryCacheIndexer = new MemoryCacheIndexer(null, memoryCache);
                    var memoryCacheBag = memoryCacheIndexer.ToNamedBag();
                    return memoryCacheBag;
                },
                configureBuilder);
        }


        /// <summary>
        /// Caches this singleton with the built-in .NET Core IMemoryCache.  
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="configureBuilder"></param>
        /// <remarks>
        /// If interested in using something other than IMemoryCache, then:
        /// <seealso cref="AddCachedSingleton{TCacheService}(IServiceCollection, Type, Type, Func{TCacheService, IBag}, Action{CacheInterceptor})"/>
        /// </remarks>
        public static void AddCachedSingleton<TService, TImplementation>(this IServiceCollection serviceCollection, Action<CacheInterceptor.Builder> configureBuilder = null)
        {
            serviceCollection.AddCachedSingleton(typeof(TService), typeof(TImplementation), configureBuilder);
        }
    }
}
