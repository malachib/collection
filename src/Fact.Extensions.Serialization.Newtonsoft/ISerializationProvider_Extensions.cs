using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Newtonsoft
{
    public static class ISerializationContainer_Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sc"></param>
        /// <param name="fileName"></param>
        /// <param name="instance"></param>
        /// <param name="autoWrap">
        /// Whether to automatically wrap the entire file operation with an anonymous object,
        /// which is a default of JSON files I've encountered.
        /// </param>
        public static void SerializeToJsonFile<T>(this ISerializerProvider sc, string fileName, T instance, bool autoWrap = true)
        {
            using (var file = File.CreateText(fileName))
            using (var writer = new JsonTextWriter(file))
            {
                if (autoWrap) writer.WriteStartObject();
                sc.SerializeToJsonWriter(writer, instance);
                if (autoWrap) writer.WriteEndObject();
            }
        }

        public static void SerializeToJsonWriter<T>(this ISerializerProvider sc, JsonWriter writer, T instance)
        {
            // NOTE: this code presumes that sc can provide an IPropertySerializer transport
            IPropertySerializer jps = new JsonPropertySerializer(writer);

            sc.Serialize(jps, instance);
        }


        public static T DeserializeFromJsonFile<T>(this IDeserializerProvider sc, string fileName, bool autoUnwrap = true)
        {
            using (var file = File.OpenText(fileName))
            using (var reader = new JsonTextReader(file))
            {
                reader.Read();
                if (autoUnwrap)
                {
                    Debug.Assert(reader.TokenType == JsonToken.StartObject);
                    reader.Read();
                }
                var instance = sc.DeserializeFromJsonReader<T>(reader);
                if (autoUnwrap)
                {
                    Debug.Assert(reader.TokenType == JsonToken.EndObject);
                    reader.Read();
                }
                return instance;
            }
        }


        public static T DeserializeFromJsonReader<T>(this IDeserializerProvider sc, JsonReader reader)
        {
            var jpds = new JsonPropertyDeserializer(reader);

            return sc.Deserialize<T, IPropertyDeserializer>(jpds);
        }
    }
}
