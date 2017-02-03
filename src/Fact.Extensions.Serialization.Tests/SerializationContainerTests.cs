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
using Fact.Extensions.Factories;

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
            var s = new FieldReflectionSerializer(instance => "testInstance");
            var key = typeof(TestRecord1).Name;
            var fileName = "temp/serializationContainer2.json";

            sc.container.Register<ISerializer<IPropertySerializer>>(s, key);
            sc.container.Register<IDeserializer<IPropertyDeserializer>>(s, key);

            doTestRecord1Test(sc, fileName);
        }


        [TestMethod]
        public void JsonSerializableTest()
        {
            var sc = new SerializationContainer2();
            /*var s = new SerializableSerializer<
                IPropertySerializer,
                IPropertyDeserializer>(); */
            var key = typeof(TestRecord1).Name;

            sc.container.Register<ISerializer<IPropertySerializer>>(
                new SerializableSerializer<IPropertySerializer>(), key);
            sc.container.Register<IDeserializer<IPropertyDeserializer>>(
                new SerializableDeserializer<IPropertyDeserializer>(), key);

            doTestRecord1Test(sc, "temp/jsonSerializableTest.json");
        }


        [TestMethod]
        public void AggregateSerializationContainerTest()
        {
            var sc = new _SerializationContainer();
            doTestRecord1Test(sc, "temp/aggregateSerializationContainerTest.json");
        }


        [TestMethod]
        public void SerializerFactoryTest()
        {
            var sc = new SerializationProvider();

            var a = new AggregateFactory<Type, ISerializer<IPropertySerializer>>();
            var a1 = new AggregateFactory<Type, IDeserializer<IPropertyDeserializer>>();

            var tsf = new TypeSerializerFactory<IPropertyDeserializer, IPropertySerializer>();
            var ssf = new SerializableSerializerFactory<IPropertyDeserializer, IPropertySerializer>();
            var frsf = new FieldReflectionSerializerFactory();

            tsf.Register(new FieldReflectionSerializer(o => "record1"), typeof(TestRecord1));

            a.Add(tsf);
            a1.Add(tsf);
            a.Add(ssf);
            a1.Add(ssf);
            a.Add(frsf);
            a1.Add(frsf);

            sc.Register(a);
            sc.Register(a1);

            doTestRecord1Test(sc, "temp/serializerFactoryTest.json");
        }


        [TestMethod]
        public void SerializerFactoryTest2()
        {
            var sc = new SerializationProvider();

            var p = new AggregatePersistor<IPropertyDeserializer, IPropertySerializer>();

            var tsf = new TypeSerializerFactory<IPropertyDeserializer, IPropertySerializer>();

            tsf.Register(new FieldReflectionSerializer(o => "record1"), typeof(TestRecord1));

            p.Add(tsf);
            p.AddSerializable();
            p.AddFieldReflection();

            sc.Register(p);

            doTestRecord1Test(sc, "temp/serializerFactoryTest2.json");
        }



        [TestMethod]
        public void SerializerFactoryTest3()
        {
            var sc = new SerializationProvider();

            sc.ConfigurePropertySerializer(tsf =>
            {
                tsf.RegisterFieldReflection<TestRecord1>(o => "record1");
            });

            doTestRecord1Test(sc, "temp/serializerFactoryTest3.json");
        }



        static void doTestRecord1Test(ISerializationProvider sc, string fileName)
        {
            var record = new TestRecord1();
            var newValue = 77;

            record.field1 = newValue;

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
        public static void SerializeToJsonFile<T>(this ISerializationProvider sc, string fileName, T instance, bool autoWrap = true)
        {
            using (var file = File.CreateText(fileName))
            using (var writer = new JsonTextWriter(file))
            {
                if(autoWrap) writer.WriteStartObject();
                sc.SerializeToJsonWriter(writer, instance);
                if(autoWrap) writer.WriteEndObject();
            }
        }

        public static void SerializeToJsonWriter<T>(this ISerializationProvider sc, JsonWriter writer, T instance)
        {
            IPropertySerializer jps = new JsonPropertySerializer(writer);

            sc.Serialize(jps, instance);
        }


        public static T DeserializeFromJsonFile<T>(this ISerializationProvider sc, string fileName, bool autoUnwrap = true)
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


        public static T DeserializeFromJsonReader<T>(this ISerializationProvider sc, JsonReader reader)
        {
            var jpds = new JsonPropertyDeserializer(reader);

            return sc.Deserialize<T, IPropertyDeserializer>(jpds);
        }
    }
}
