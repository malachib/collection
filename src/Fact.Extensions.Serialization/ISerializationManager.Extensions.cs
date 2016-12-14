#if NETSTANDARD1_6
#define FEATURE_ENABLE_PIPELINES
#endif

using System;
using System.Collections.Generic;
using System.IO;
#if FEATURE_ENABLE_PIPELINES
using System.Buffers;
using System.IO.Pipelines;
#endif
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public static class ISerializationManager_Extensions
    {
        public static byte[] SerializeToByteArray(this ISerializationManager serializationManager, object input, Type type = null)
        {
            using (var ms = new MemoryStream())
            {
                serializationManager.Serialize(ms, input, type);
                ms.Flush();
                return ms.ToArray();
            }
        }


        public static string SerializeToString(this ISerializationManager serializationManager, object input, Type type = null, Encoding encoding = null)
        {
            if (encoding == null)
            {
                if (serializationManager is ISerializationManager_TextEncoding)
                    encoding = ((ISerializationManager_TextEncoding)serializationManager).Encoding;
                else
                {
                    // TODO: Do a policy thing here, throw exception or a default
                    encoding = Encoding.UTF8;
                }
            }
            var byteArray = serializationManager.SerializeToByteArray(input, type);
            return encoding.GetString(byteArray, 0, byteArray.Length);
        }

        /*
        public static string SerializeToString(this ISerializationManager serializationManager, object input)
        {
            var byteArray = serializationManager.SerializeToByteArray(input, type);
        }*/

        public static async Task<byte[]> SerializeToByteArrayAsync(this ISerializerAsync<Stream> serializationManager, object input, Type type = null)
        {
            using (var ms = new MemoryStream())
            {
                await serializationManager.SerializeAsync(ms, input);
                ms.Flush();
                return ms.ToArray();
            }
        }



        public static object Deserialize(this ISerializationManager serializationManager, byte[] inputValue, Type type)
        {
            using (var ms = new System.IO.MemoryStream(inputValue))
            {
                return serializationManager.Deserialize(ms, type);
            }
        }


        public static object Deserialize(this ISerializationManager serializationManager, string input, Type type, Encoding encoding = null)
        {
            if (encoding == null)
            {
                if (serializationManager is ISerializationManager_TextEncoding)
                    encoding = ((ISerializationManager_TextEncoding)serializationManager).Encoding;
                else
                    // TODO: Do a policy thing here, throw exception or a default
                    encoding = Encoding.UTF8;
            }
            
            var stream = new ReadonlyStringStream(input, encoding);
            return serializationManager.Deserialize(stream, type);
        }


        public static T Deserialize<T>(this ISerializationManager serializationManager, byte[] input)
        {
            return (T)serializationManager.Deserialize(input, typeof(T));
        }

        public static T Deserialize<T>(this ISerializationManager serializationManager, string input, Encoding encoding = null)
        {
            return (T) serializationManager.Deserialize(input, typeof(T), encoding);
        }

        public static async Task<object> DeserializeAsync(this IDeserializerAsync<Stream> serializationManager, byte[] inputValue, Type type)
        {
            using (var ms = new System.IO.MemoryStream(inputValue))
            {
                return await serializationManager.DeserializeAsync(ms, type);
            }
        }


        public static T Deserialize<T>(this ISerializationManager serializationManager, Stream input)
        {
            return (T) serializationManager.Deserialize(input, typeof(T));
        }
    }
}
