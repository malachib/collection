using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Fact.Extensions.Serialization
{
    public class JsonPropertySerializer : IPropertySerializer, IDisposable
    {
        readonly JsonWriter writer;

        public JsonPropertySerializer(JsonWriter writer)
        {
            this.writer = writer;
            //writer.WriteStartObject();
        }

        public object this[string key, Type type]
        {
            set
            {
                writer.WritePropertyName(key);
                writer.WriteValue(value);
            }
        }

        public void StartNode(object key, object[] attributes = null)
        {
            writer.WriteStartObject();
        }


        public void EndNode()
        {
            writer.WriteEndObject();
        }

        public void Dispose()
        {
            //writer.WriteEndObject();
        }
    }

    public class JsonPropertyDeserializer : IPropertyDeserializer
    {
        readonly JsonReader reader;

        public JsonPropertyDeserializer(JsonReader reader)
        {
            this.reader = reader;
            //Debug.Assert(reader.TokenType == JsonToken.StartObject);
            //reader.Read();
        }


        public void StartNode(out object key, out object[] attributes)
        {
            Debug.Assert(reader.TokenType == JsonToken.StartObject);
            reader.Read();
            // JSON has no concept of node keys or attributes
            key = null;
            attributes = null;
        }


        public void EndNode()
        {
            Debug.Assert(reader.TokenType == JsonToken.EndObject);
            reader.Read();
        }

        public object Get(string key, Type type)
        {
            Debug.Assert(reader.TokenType == JsonToken.PropertyName);
            var propertyName = (string)reader.Value;
            reader.Read();
#if DEBUG
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    Debug.Assert(type == typeof(string));
                    break;

                case JsonToken.Boolean:
                    Debug.Assert(type == typeof(bool));
                    break;
            }
#endif
            var value = reader.Value;
            reader.Read();
            return value;
        }
    }
}
