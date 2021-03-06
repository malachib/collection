﻿using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Fact.Extensions.Collection;
using Fact.Extensions.Serialization;

namespace Fact.Extensions.Caching
{
    /// <summary>
    /// Wraps up an IMemoryCache with an IIndexer
    /// </summary>
    public class MemoryCacheIndexer : 
        ITryGetter<object>, 
        IIndexer<object, object>,
        IRemover<object>
    {
        readonly IMemoryCache cache;

        public event Action<ICacheEntry> CreatingEntry;

        public MemoryCacheIndexer(IMemoryCache cache)
        {
            this.cache = cache;
        }


        public object this[object key]
        {
            get { return cache.Get(key); }
            set
            {
                using (var cacheEntry = cache.CreateEntry(key))
                {
                    CreatingEntry?.Invoke(cacheEntry);
                    cacheEntry.SetSlidingExpiration(TimeSpan.FromSeconds(120));
                    cacheEntry.SetValue(value);
                }
            }
        }


        public void Remove(object key) { cache.Remove(key); }

        public bool TryGet(object key, Type type, out object value)
        {
            return cache.TryGetValue(key, out value);
        }
    }
}
