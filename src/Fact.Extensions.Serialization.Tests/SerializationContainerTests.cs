using Microsoft.Extensions.DependencyInjection;
using Fact.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using Fact.Extensions.Serialization.Newtonsoft;
using Fact.Extensions.Collection;

namespace Fact.Extensions.Serialization.Tests
{
    [TestClass]
    public class SerializationContainerTests
    {
        public class TestRecord1 : 
            ISerializable<IPropertySerializer>, 
            IDeserializable<IPropertyDeserializer>
        {
            [Persist]
            internal int field1;
            [Persist]
            internal string field2;


            public TestRecord1() { }

            public TestRecord1(IPropertyDeserializer deserializer)
            {
                field1 = deserializer.Get<int>("field1-named");
                field2 = deserializer.Get<string>("field2-named");
            }

            public void Serialize(IPropertySerializer serializer)
            {
                serializer.Set("field1-named", field1);
                serializer.Set("field2-named", field2);
            }
        }

        [TestMethod]
        public void Test1()
        {
            var _sc = new ServiceCollection();
            //new FieldReflectionSerializer();
            // VS bugged out, can't find it
            var fieldReflectionSerializer = new FieldReflectionSerializer();
            _sc.AddSingleton<ISerializer<IPropertySerializer>>(fieldReflectionSerializer);
            _sc.AddSingleton<IDeserializer<IPropertyDeserializer>>(fieldReflectionSerializer);
            var sp = _sc.BuildServiceProvider();
            var sc = new SerializationContainer(sp);
        }


        [TestMethod]
        public void Test2()
        {
            var sc = new ExperimentalSerializationContainer(null);

            sc.Register(new FieldReflectionSerializer(), typeof(TestRecord1));
            var s = sc.GetSerializer(typeof(TestRecord1));
        }


        [TestMethod]
        public void Test3()
        {
            var sc = new SerializationContainer2();
            var record = new TestRecord1();
            var s = new FieldReflectionSerializer(instance => "testInstance");
            var key = typeof(TestRecord1).Name;
            var fileName = "temp/serializationContainer2.json";
            var newValue = 77;

            record.field1 = newValue;
            //sc.Register(s, typeof(TestRecord1));
            sc.container.Register<ISerializer<IPropertySerializer>>(s, key);
            sc.container.Register<IDeserializer<IPropertyDeserializer>>(s, key);

            sc.SerializeToJsonFile(fileName, record);
            var record2 = sc.DeserializeFromJsonFile<TestRecord1>(fileName);

            Assert.AreEqual(newValue, record2.field1);
        }


        [TestMethod]
        public void JsonSerializableTest()
        {
            var sc = new SerializationContainer2();
            var record = new TestRecord1();
            var s = new SerializableSerializer<
                IPropertySerializer,
                IPropertyDeserializer>();
            var key = typeof(TestRecord1).Name;
            var fileName = "temp/jsonSerializableTest.json";
            var newValue = 77;

            record.field1 = newValue;
            //sc.Register(s, typeof(TestRecord1));
            sc.container.Register<ISerializer<IPropertySerializer>>(s, key);
            sc.container.Register<IDeserializer<IPropertyDeserializer>>(s, key);

            sc.SerializeToJsonFile(fileName, record);
            var record2 = sc.DeserializeFromJsonFile<TestRecord1>(fileName);

            Assert.AreEqual(newValue, record2.field1);
        }
    }

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
        public static void SerializeToJsonFile<T>(this ISerializationContainer sc, string fileName, T instance, bool autoWrap = true)
        {
            using (var file = File.CreateText(fileName))
            using (var writer = new JsonTextWriter(file))
            {
                if(autoWrap) writer.WriteStartObject();
                sc.SerializeToJsonWriter(writer, instance);
                if(autoWrap) writer.WriteEndObject();
            }
        }

        public static void SerializeToJsonWriter<T>(this ISerializationContainer sc, JsonWriter writer, T instance)
        {
            IPropertySerializer jps = new JsonPropertySerializer(writer);

            sc.Serialize(jps, instance);
        }


        public static T DeserializeFromJsonFile<T>(this ISerializationContainer sc, string fileName, bool autoUnwrap = true)
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


        public static T DeserializeFromJsonReader<T>(this ISerializationContainer sc, JsonReader reader)
        {
            var jpds = new JsonPropertyDeserializer(reader);

            return sc.Deserialize<T, IPropertyDeserializer>(jpds);
        }
    }
}
