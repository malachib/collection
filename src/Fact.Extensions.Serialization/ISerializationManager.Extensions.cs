﻿using System;
using System.Collections.Generic;
using System.IO;
#if NETSTANDARD1_6_2
using System.IO.Pipelines;
#endif
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

        public static async Task<byte[]> SerializeToByteArrayAsync(this ISerializationManagerAsync serializationManager, object input, Type type = null)
        {
            using (var ms = new MemoryStream())
            {
#if NETSTANDARD1_6_2
                // See below comment in DeserializeAsync regarding kludginess of this
                var writer = ms.AsPipelineWriter();
                await serializationManager.SerializeAsync(writer, input);
                writer.Complete();
#else
                await serializationManager.SerializeAsync(ms, input);
#endif
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

        public static async Task<object> DeserializeAsync(this ISerializationManagerAsync serializationManager, byte[] inputValue, Type type)
        {
            using (var ms = new System.IO.MemoryStream(inputValue))
            {
#if NETSTANDARD1_6_2
                // 100% certain this is not the right way, but PipeLine stuff is pretty new
                // so after 15 minutes of digging and finding no clues, I am rolling with this - for now
                var reader = ms.AsPipelineReader();
                return await serializationManager.DeserializeAsync(reader, type);
#else
                return await serializationManager.DeserializeAsync(ms, type);
#endif
            }
        }


        public static T Deserialize<T>(this ISerializationManager serializationManager, Stream input)
        {
            return (T) serializationManager.Deserialize(input, typeof(T));
        }
    }
}
