using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Fact.Extensions.Collection;
using Fact.Extensions.Serialization;
using Microsoft.Extensions.Caching.Memory;
using System.IO;

namespace Fact.Extensions.Caching
{
    public class MemoryCacheBag : ICache
    {
        // TODO: serializationManager is 100% dormant here, but at some point we may want to fill it out
        readonly ISerializationManager<Stream> serializationManager;
        readonly IMemoryCache cache;

        public event Action<ICacheEntry> CreatingEntry;

        /// <summary>
        /// EXPERIMENTAL
        /// For this entire bag behavior, allow options and possibly cached-object value itself
        /// to be modified before placement in the cache
        /// </summary>
        public event Action<string, object, List<ICacheItemOption>> Modify;

        public MemoryCacheBag(IMemoryCache cache, ISerializationManager<Stream> serializationManager = null)
        {
            this.serializationManager = serializationManager;
            this.cache = cache;

        }

        public object this[string key, Type type]
        {
            set
            {
                // FIX: clunky code
                this.Set(key, value, type, new ICacheItemOption[0]);
            }
        }

        public IEnumerable<Type> SupportedOptions
        {
            get
            {
                yield return typeof(AbsoluteTimeExpiration);
                yield return typeof(SlidingTimeExpiration);
                yield return typeof(CacheItemOption);
            }
        }

        public object Get(string key, Type type)
        {
            object value;
            if (cache.TryGetValue(key, out value))
                return value;
            throw new KeyNotFoundException();
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }

        public void Set(string key, object value, Type type, params ICacheItemOption[] _options)
        {
            IEnumerable<ICacheItemOption> options;

            if (Modify != null)
            {
                var modifiableOptions = new List<ICacheItemOption>(_options);
                Modify(key, value, modifiableOptions);
                options = modifiableOptions;
            }
            else
                options = _options;

            using (var cacheEntry = cache.CreateEntry(key))
            {
                foreach(var option in options)
                {
                    if(option is SlidingTimeExpiration)
                    {
                        cacheEntry.SetSlidingExpiration(((SlidingTimeExpiration)option).Duration);
                    }
                    else if(option is AbsoluteTimeExpiration)
                    {
                        cacheEntry.SetAbsoluteExpiration(((AbsoluteTimeExpiration)option).Expiry);
                    }
                    else if(option is CacheItemOption)
                    {
                        var o = (CacheItemOption)option;

                        // TODO: Instead of 100% rolling my own, reuse their CacheItem priority
                        switch(o.Priority)
                        {
                            case CacheItemPriority.High:
                                cacheEntry.Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.High;
                                break;
                            case CacheItemPriority.Normal:
                                cacheEntry.Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal;
                                break;
                            case CacheItemPriority.Low:
                                cacheEntry.Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.Low;
                                break;
                            case CacheItemPriority.NotRemovable:
                                cacheEntry.Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.NeverRemove;
                                break;
                        }
                    }
                }

                cacheEntry.SetValue(value);
                CreatingEntry?.Invoke(cacheEntry);
            }
        }

        public bool TryGet(string key, Type type, out object value)
        {
            return cache.TryGetValue(key, out value);
        }
    }
}
