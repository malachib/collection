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
        public static void Set(this ICache cache, string key, object value, Type type, DateTime absoluteTimeExpiration)
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


        public static void Set<TValue>(this ICache cache, string key, TValue value, DateTime absoluteTimeExpiration)
        {
            cache.Set(key, value, typeof(TValue), absoluteTimeExpiration);
        }


        public static void Set<TValue>(this ICache cache, string key, TValue value, TimeSpan slidingTimeExpiration)
        {
            cache.Set(key, value, typeof(TValue), slidingTimeExpiration);
        }
    }
}
