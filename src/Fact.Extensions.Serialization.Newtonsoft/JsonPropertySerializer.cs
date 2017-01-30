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
            writer.WriteStartObject();
        }

        public object this[string key, Type type]
        {
            set
            {
                writer.WritePropertyName(key);
                writer.WriteValue(value);
            }
        }

        public void Dispose()
        {
            writer.WriteEndObject();
        }
    }

    public class JsonPropertyDeserializer : IPropertyDeserializer
    {
        readonly JsonReader reader;

        public JsonPropertyDeserializer(JsonReader reader)
        {
            this.reader = reader;
            // FIX: debatable whether we want a read HERE.  Probably we want to read *AFTER* the assert
            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.StartObject);
        }

        public object Get(string key, Type type)
        {
            // FIX: again, debatable whether we want to read HERE.  Perhaps we should expect to already
            // expect to be at propertyName, and instead move PAST the last property which is owned by us
            // leaving the next (uninsterested by us) reader position at the ready
            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.PropertyName);
            var propertyName = (string)reader.Value;
            reader.Read();
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    Debug.Assert(type == typeof(string));
                    break;

                case JsonToken.Boolean:
                    Debug.Assert(type == typeof(bool));
                    break;
            }
            return reader.Value;
        }
    }
}
