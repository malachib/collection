using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Fact.Extensions.Collection;

namespace Fact.Extensions.Caching
{
    /// <summary>
    /// EXPERIMENTAL
    /// Use this to further attempt to hide the fact that one is caching a value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct CachedReference<T>
    {
        readonly Func<T> factory;
        readonly ICache cache;
        readonly string key;
        readonly Func<IEnumerable<ICacheItemOption>> getOptions;

        public CachedReference(ICache cache, string key, Func<T> factory, params ICacheItemOption[] options)
        {
            this.cache = cache;
            this.key = key;
            this.factory = factory;
            this.getOptions = () => options;
        }

        public T Value
        {
            get
            {
                T cachedValue;

                if (!cache.TryGet<T>(key, out cachedValue))
                {
                    cachedValue = factory();
                    // TODO: Make an AsArray and use it
                    cache.Set(key, cachedValue, typeof(T), getOptions().ToArray());
                }
                return cachedValue;
            }
        }

        public static implicit operator T(CachedReference<T> cachedReference)
        {
            return cachedReference.Value;
        }
    }


    public struct CachedReferenceAsync<T>
    {
        readonly ICacheAsync cache;
        readonly Func<Task<T>> factory;
        readonly string key;
        readonly ICacheItemOption[] options;

        public CachedReferenceAsync(ICacheAsync cache, string key, Func<Task<T>> factory, params ICacheItemOption[] options)
        {
            this.cache = cache;
            this.factory = factory;
            this.key = key;
            this.options = options;
        }


        /// <summary>
        /// Retrieve item from the cache, and if it's not present there, allocate + add it
        /// </summary>
        /// <returns></returns>
        public async Task<T> GetValue()
        {
            var response = await cache.TryGetAsync(key, typeof(T));
            if (response.Item1)
                return (T)response.Item2;
            else
            {
                var value = await factory();
                await cache.SetAsync(key, value, typeof(T), options);
                return value;
            }
        }
    }

}
