using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Fact.Extensions.Collection;

namespace Fact.Extensions.Caching
{
    public interface ICachedReference
    {
        ICacheItemOption[] Options { get; }
    }


    public interface ICachedReference<TValue> : ICachedReference
    {
        string Key { get; }
        TValue Value { get; set; }
    }

    /// <summary>
    /// EXPERIMENTAL
    /// Use this to further attempt to hide the fact that one is caching a value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>Bears a spiritual similarity to State [of T] class</remarks>
    public struct CachedReference<T> : ICachedReference<T>
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

        public string Key => key;
        public void Clear() { cache.Remove(key); }

        public ICacheItemOption[] Options => getOptions().AsArray();

        public T Value
        {
            get
            {
                T cachedValue;

                if (!cache.TryGet<T>(key, out cachedValue))
                {
                    cachedValue = factory();
                    cache.Set(key, cachedValue, typeof(T), Options);
                }
                return cachedValue;
            }
            set
            {
                // One can explicitly set the cache value if one desires, bypassing the factory method.
                // this can be useful for seed values
                cache.Set(key, value, typeof(T), getOptions().AsArray());
            }
        }

        public static implicit operator T(CachedReference<T> cachedReference)
        {
            return cachedReference.Value;
        }
    }

    public struct CachedReferenceAsync<T> : ICachedReference<T>
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


        public string Key => key;
        public async Task Clear() { await cache.RemoveAsync(key); }


        public ICacheItemOption[] Options => options;

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
                await SetValue(value);
                return value;
            }
        }


        public async Task SetValue(T value)
        {
            await cache.SetAsync(key, value, typeof(T), options);
        }


        public T Value
        {
            get { return GetValue().Result; }
            set { SetValue(value).Wait(); }
        }


        public static implicit operator T(CachedReferenceAsync<T> cachedReference)
        {
            return cachedReference.Value;
        }
    }

}
