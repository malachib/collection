using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Fact.Extensions.Serialization;
using System.Linq;
using System.IO;

namespace Fact.Extensions.Caching
{
    public class DistributedCacheBag : ICache, ICacheAsync
    {
        readonly IDistributedCache cache;
        readonly ISerializationManager<Stream> serializationManager;

        public IEnumerable<Type> SupportedOptions
        {
            get
            {
                yield return typeof(AbsoluteTimeExpiration);
                yield return typeof(SlidingTimeExpiration);
            }
        }

        public event Action<string, DistributedCacheEntryOptions> Setting;

        public DistributedCacheBag(IDistributedCache cache, ISerializationManager<Stream> serializationManager)
        {
            this.serializationManager = serializationManager;
            this.cache = cache;
        }

        public object this[string key, Type type]
        {
            set
            {
                var options = new DistributedCacheEntryOptions();
                Setting?.Invoke(key, options);
                cache.Set(key, serializationManager.SerializeToByteArray(value, type), options);
            }
        }

        public object Get(string key, Type type)
        {
            var value = cache.Get(key);
            return serializationManager.Deserialize(value, type);
        }

        public async Task<object> GetAsync(string key, Type type)
        {
            var value = await cache.GetAsync(key);
            return await serializationManager.DeserializeAsyncHelper(value, type);
            /*
            if (serializationManager is ISerializationManagerAsync)
                return await ((ISerializationManagerAsync)serializationManager).DeserializeAsync(value, type);
            else
                return serializationManager.Deserialize(value, type);*/
        }

        public async Task SetAsync(string key, object value, Type type)
        {
            await SetAsync(key, value, type, new ICacheItemOption[0]);
        }

        public void Remove(string key) { cache.Remove(key); }
        public async Task RemoveAsync(string key)
        {
            await cache.RemoveAsync(key);
        }

        public bool TryGet(string key, Type type, out object value)
        {
            var rawBytes = cache.Get(key);
            if (rawBytes != null)
            {
                value = serializationManager.Deserialize(rawBytes, type);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }


        static void OptionConverter(DistributedCacheEntryOptions msOptions, params ICacheItemOption[] options)
        {
            foreach(var option in options)
            {
                if(option is SlidingTimeExpiration)
                {
                    msOptions.SetSlidingExpiration(((SlidingTimeExpiration)option).Duration);
                }
                else if(option is AbsoluteTimeExpiration)
                {
                    msOptions.SetAbsoluteExpiration(((AbsoluteTimeExpiration)option).Expiry);
                }
            }
        }

        public async Task SetAsync(string key, object value, Type type, params ICacheItemOption[] options)
        {
            var _options = new DistributedCacheEntryOptions();

            OptionConverter(_options, options);
            Setting?.Invoke(key, _options);

            byte[] serializedValue;

            if (serializationManager is ISerializerAsync<Stream>)
                serializedValue = await ((ISerializerAsync<Stream>)serializationManager).SerializeToByteArrayAsync(value, type);
            else
                serializedValue = serializationManager.SerializeToByteArray(value, type);

            await cache.SetAsync(key, serializedValue, _options);
        }

        public void Set(string key, object value, Type type, params ICacheItemOption[] options)
        {
            var _options = new DistributedCacheEntryOptions();
            OptionConverter(_options, options);
            Setting?.Invoke(key, _options);
            cache.Set(key, serializationManager.SerializeToByteArray(value, type), _options);
        }

        public async Task<Tuple<bool, object>> TryGetAsync(string key, Type type)
        {
            // according to this https://github.com/aspnet/Caching/blob/dev/samples/RedisCacheSample/Program.cs
            // it's implied that regular getters can merely return null if key not present.  So we'll try that
            var byteArray = await cache.GetAsync(key);
            if (byteArray == null)
                return Tuple.Create(false, (object)null);
            else
            {
                object value = await serializationManager.DeserializeAsyncHelper(byteArray, type);
                return Tuple.Create(true, value);
            }
        }
    }
}
