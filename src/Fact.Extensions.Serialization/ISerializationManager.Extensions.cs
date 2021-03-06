﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public static class ISerializationManager_Extensions
    {
        public static Encoding GetEncodingOrDefault<T>(this ISerializer<T> serializer)
        {
            Encoding encoding;
            if (serializer is ISerializationManager_TextEncoding)
                return ((ISerializationManager_TextEncoding)serializer).Encoding;
            else
            {
                // TODO: Do a policy thing here, throw exception or a default
                return Encoding.UTF8;
            }
        }

        public static byte[] SerializeToByteArray(this ISerializer<TextWriter> serializer, object input, Type type = null)
        {
            var stringOutput = serializer.SerializeToString(input, type);
            var encoding = serializer.GetEncodingOrDefault();
            return encoding.GetBytes(stringOutput);
        }

        public static byte[] SerializeToByteArray(this ISerializer<Stream> serializationManager, object input, Type type = null)
        {
            using (var ms = new MemoryStream())
            {
                serializationManager.Serialize(ms, input, type);
                ms.Flush();
                return ms.ToArray();
            }
        }


        public static string SerializeToString(this ISerializer<TextWriter> serializer, object input, Type type = null)
        {
            var writer = new StringWriter();
            serializer.Serialize(writer, input, type);
            return writer.ToString();
        }


        public static string SerializeToString(this ISerializer<Stream> serializationManager, object input, Type type = null, Encoding encoding = null)
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
                await serializationManager.SerializeAsync(ms, input, type);
                ms.Flush();
                return ms.ToArray();
            }
        }


        public static object Deserialize(this IDeserializer<TextReader> deserializer, byte[] inputValue, Type type)
        {
            var encoding = ((ISerializer<TextWriter>)deserializer).GetEncodingOrDefault();
            using (var stream = new MemoryStream(inputValue))
            {
                using (var reader = new StreamReader(stream, encoding))
                {
                    return deserializer.Deserialize(reader, type);
                }
            }
        }


        public static object Deserialize(this IDeserializer<TextReader> deserializer, string input, Type type)
        {
            using (var reader = new StringReader(input))
            {
                return deserializer.Deserialize(reader, type);
            }
        }

        public static T Deserialize<T>(this IDeserializer<TextReader> deserializer, string input)
        {
            return (T)deserializer.Deserialize(input, typeof(T));
        }


        public static object Deserialize(this IDeserializer<Stream> serializationManager, byte[] inputValue, Type type)
        {
            using (var ms = new MemoryStream(inputValue))
            {
                return serializationManager.Deserialize(ms, type);
            }
        }


        public static object Deserialize(this IDeserializer<Stream> serializationManager, string input, Type type, Encoding encoding = null)
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


        public static T Deserialize<T>(this IDeserializer<Stream> serializationManager, byte[] input)
        {
            return (T)serializationManager.Deserialize(input, typeof(T));
        }

        public static T Deserialize<T>(this IDeserializer<Stream> serializationManager, string input, Encoding encoding = null)
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


        public static T Deserialize<T>(this IDeserializer<Stream> serializationManager, Stream input)
        {
            return (T) serializationManager.Deserialize(input, typeof(T));
        }


        /// <summary>
        /// Assists in deserializing from a byte array, and automatically forward casts to ISerializationManagerAsync
        /// if available - otherwise uses regular ISerializationManager
        /// </summary>
        /// <param name="serializationManager"></param>
        /// <param name="input"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static async Task<object> DeserializeAsyncHelper(this IDeserializer<Stream> serializationManager, byte[] input, Type type)
        {
            if (serializationManager is IDeserializerAsync<Stream>)
                return await ((IDeserializerAsync<Stream>)serializationManager).DeserializeAsync(input, type);
            else
                return serializationManager.Deserialize(input, type);
        }
    }
}
