using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public interface ISerializationManager
    {
        void Serialize(System.IO.Stream output, object inputValue, Type type = null);
        object Deserialize(System.IO.Stream input, Type type);
    }


    public interface ISerializationManagerAsync
    {
        Task SerializeAsync(System.IO.Stream output, object inputValue, Type type = null);
        Task<object> DeserializeAsync(System.IO.Stream input, Type type);
    }


    public static class ISerializationManager_Extensions
    {
        public static byte[] Serialize(this ISerializationManager serializationManager, object input, Type type = null)
        {
            using (var ms = new MemoryStream())
            {
                serializationManager.Serialize(ms, input);
                ms.Flush();
                return ms.ToArray();
            }
        }

        public static async Task<byte[]> SerializeAsync(this ISerializationManagerAsync serializationManager, object input, Type type = null)
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

        public static async Task<object> DeserializeAsync(this ISerializationManagerAsync serializationManager, byte[] inputValue, Type type)
        {
            using (var ms = new System.IO.MemoryStream(inputValue))
            {
                return await serializationManager.DeserializeAsync(ms, type);
            }
        }
    }
}
