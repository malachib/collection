using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Fact.Extensions.Collection.DistributedCache
{
    public class DistributedCacheBag : IBag, IBagAsync
    {
        readonly IDistributedCache cache;
        readonly ISerializationManager serializationManager;

        public object Get(string key, Type type)
        {
            var value = cache.Get(key);
            return serializationManager.Deserialize(value, type);
        }

        public async Task<object> GetAsync(string key, Type type)
        {
            var value = await cache.GetAsync(key);
            if (serializationManager is ISerializationManagerAsync)
                return await ((ISerializationManagerAsync)serializationManager).DeserializeAsync(value, type);
            else
                return serializationManager.Deserialize(value, type);
        }

        public void Set(string key, object value, Type type = null)
        {
            cache.Set(key, serializationManager.Serialize(value, type));
        }

        public async Task SetAsync(string key, object value, Type type = null)
        {
            byte[] serializedValue;

            if (serializationManager is ISerializationManagerAsync)
                serializedValue = await ((ISerializationManagerAsync)serializationManager).SerializeAsync(value, type);
            else
                serializedValue = serializationManager.Serialize(value, type);

            await cache.SetAsync(key, serializedValue);
        }
    }
}
