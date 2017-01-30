﻿using System;
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
        readonly bool closeOnDispose;

        public JsonPropertySerializer(JsonWriter writer, bool closeOnDispose = false)
        {
            this.writer = writer;
            this.closeOnDispose = closeOnDispose;
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
            writer.Flush();
            if (closeOnDispose) writer.Close();
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
            var value = reader.Value;
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    Debug.Assert(type == typeof(string));
                    break;

                case JsonToken.Boolean:
                    Debug.Assert(type == typeof(bool));
                    break;

                case JsonToken.Float:
                    Debug.Assert(type == typeof(float) || type == typeof(double));
                    // Because NewtonSoft always serializes as a double
                    value = Convert.ChangeType(value, type);
                    break;

                case JsonToken.Integer:
                    Debug.Assert(type == typeof(short) || type == typeof(int) || type == typeof(long));
                    // Because NewtonSoft always serializes (probably) as a long
                    value = Convert.ChangeType(value, type);
                    break;
            }
            reader.Read();
            return value;
        }
    }
}
