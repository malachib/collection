using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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


        public static string SerializeToString(this ISerializationManager serializationManager, object input, Type type, Encoding encoding)
        {
            var byteArray = serializationManager.SerializeToByteArray(input, type);
            return encoding.GetString(byteArray, 0, byteArray.Length);
        }

        /*
        public static string SerializeToString(this ISerializationManager serializationManager, object input)
        {
            var byteArray = serializationManager.SerializeToByteArray(input, type);
        }*/

        public static async Task<byte[]> SerializeToByteArrayAsync(this ISerializationManagerAsync serializationManager, object input, Type type = null)
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

        /*
        public static object Deserialize(this ISerializationManager serializationManager, string input, Type type, Encoder encoder)
        {
            var reader = new StringReader(input);
        }*/

        public static async Task<object> DeserializeAsync(this ISerializationManagerAsync serializationManager, byte[] inputValue, Type type)
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
