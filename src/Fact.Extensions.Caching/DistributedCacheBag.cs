using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Fact.Extensions.Serialization;

namespace Fact.Extensions.Collection.Cache
{
    public class DistributedCacheBag : IBag, IBagAsync, IRemover, IRemoverAsync
    {
        readonly IDistributedCache cache;
        readonly ISerializationManager serializationManager;

        public event Action<string, DistributedCacheEntryOptions> Setting;

        public DistributedCacheBag(ISerializationManager serializationManager, IDistributedCache cache)
        {
            this.serializationManager = serializationManager;
            this.cache = cache;
        }

        public object this[string key, Type type]
        {
            get
            {
                var value = cache.Get(key);
                return serializationManager.Deserialize(value, type);
            }
            set
            {
                var options = new DistributedCacheEntryOptions();
                Setting?.Invoke(key, options);
                cache.Set(key, serializationManager.SerializeToByteArray(value, type), options);
            }
        }

        public async Task<object> GetAsync(string key, Type type)
        {
            var value = await cache.GetAsync(key);
            if (serializationManager is ISerializationManagerAsync)
                return await ((ISerializationManagerAsync)serializationManager).DeserializeAsync(value, type);
            else
                return serializationManager.Deserialize(value, type);
        }

        public async Task SetAsync(string key, object value, Type type)
        {
            var options = new DistributedCacheEntryOptions();
            Setting?.Invoke(key, options);

            byte[] serializedValue;

            if (serializationManager is ISerializationManagerAsync)
                serializedValue = await ((ISerializationManagerAsync)serializationManager).SerializeToByteArrayAsync(value, type);
            else
                serializedValue = serializationManager.SerializeToByteArray(value, type);

            await cache.SetAsync(key, serializedValue, options);
        }

        public void Remove(string key) { cache.Remove(key); }
        public async Task RemoveAsync(string key)
        {
            await cache.RemoveAsync(key);
        }
    }
}
