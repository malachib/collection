#define STRICT
//#define CONTRACTS_ENABLED


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Caching
{
    public static class ICache_Extensions
    {
        public static void Set(this ICache cache, string key, object value, Type type, DateTimeOffset absoluteTimeExpiration)
        {
#if STRICT
            var validOperation = cache.SupportedOptions.Contains(typeof(AbsoluteTimeExpiration));
#if CONTRACTS_ENABLED
            //Contract.
#else
            if (!validOperation) throw new InvalidOperationException();
#endif
#endif
            cache.Set(key, value, type, new AbsoluteTimeExpiration(absoluteTimeExpiration));
        }


        public static void Set(this ICache cache, string key, object value, Type type, TimeSpan duration)
        {
#if STRICT
            bool validOperation = cache.SupportedOptions.Contains(typeof(SlidingTimeExpiration));
#if CONTRACTS_ENABLED
            //Contract.
#else
            if (!validOperation) throw new InvalidOperationException();
#endif
#endif
            cache.Set(key, value, type, new SlidingTimeExpiration(duration));
        }


        public static void Set<TValue>(this ICache cache, string key, TValue value, DateTimeOffset absoluteTimeExpiration)
        {
            cache.Set(key, value, typeof(TValue), absoluteTimeExpiration);
        }


        public static void Set<TValue>(this ICache cache, string key, TValue value, TimeSpan slidingTimeExpiration)
        {
            cache.Set(key, value, typeof(TValue), slidingTimeExpiration);
        }


        public static async Task SetAsync<TValue>(this ICacheAsync cache, string key, TValue value, DateTimeOffset absoluteTimeExpiration)
        {
            await cache.SetAsync(key, value, typeof(TValue), new AbsoluteTimeExpiration(absoluteTimeExpiration));
        }


        public static async Task SetAsync<TValue>(this ICacheAsync cache, string key, TValue value, TimeSpan slidingTimeExpiration)
        {
            await cache.SetAsync(key, value, typeof(TValue), new SlidingTimeExpiration(slidingTimeExpiration));
        }

        /// <summary>
        /// Acquire a CachedReference class using the specified key
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="cache"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        public static CachedReference<TValue> Reference<TValue>(this ICache cache, string key, Func<TValue> factory, params ICacheItemOption[] options)
        {
            return new CachedReference<TValue>(cache, key, factory, options);
        }


        /// <summary>
        /// Acquire a CachedReference class using an auto-generated unique key
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="cache"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static CachedReference<TValue> Reference<TValue>(this ICache cache, Func<TValue> factory, params ICacheItemOption[] options)
        {
            var guid = Guid.NewGuid();
            var key = guid.ToString();
            return new CachedReference<TValue>(cache, "CachedReference:" + key, factory, options);
        }


        /// <summary>
        /// Acquire a CachedReference class using the specified key
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="cache"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        public static CachedReferenceAsync<TValue> ReferenceAsync<TValue>(this ICacheAsync cache, string key, Func<Task<TValue>> factory, params ICacheItemOption[] options)
        {
            return new CachedReferenceAsync<TValue>(cache, key, factory, options);
        }

        /// <summary>
        /// Acquire a CachedReference class using the specified key
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="cache"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        public static CachedReferenceAsync<TValue> ReferenceAsync<TValue>(this ICacheAsync cache, Func<Task<TValue>> factory, params ICacheItemOption[] options)
        {
            var guid = Guid.NewGuid();
            var key = guid.ToString();
            return new CachedReferenceAsync<TValue>(cache, key, factory, options);
        }
    }
}
