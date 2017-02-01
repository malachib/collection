using Microsoft.Extensions.DependencyInjection;
using Fact.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Fact.Extensions.Serialization.Tests
{
    [TestClass]
    public class SerializationContainerTests
    {
        public class TestRecord1
        {
            [Persist]
            int field1 = 11;
            [Persist]
            string field2 = "Field #2";
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
            var s = new FieldReflectionSerializer();
            var key = typeof(TestRecord1).Name;
            var fileName = "temp/serializationContainer2.json";
            //sc.Register(s, typeof(TestRecord1));
            sc.container.Register<ISerializer<IPropertySerializer>>(s, key);
            sc.container.Register<IDeserializer<IPropertyDeserializer>>(s, key);

            sc.SerializeToJsonFile(fileName, record);
            var record2 = sc.DeserializeFromJsonFile<TestRecord1>(fileName);
        }
    }

    public static class ISerializationContainer_Extensions
    {
        public static void SerializeToJsonFile<T>(this ISerializationContainer sc, string fileName, T instance)
        {
            using (var file = File.CreateText(fileName))
            using (var writer = new JsonTextWriter(file))
            {
                sc.SerializeToJsonWriter(writer, instance);
            }
        }

        public static void SerializeToJsonWriter<T>(this ISerializationContainer sc, JsonWriter writer, T instance)
        {
            IPropertySerializer jps = new JsonPropertySerializer(writer);

            sc.Serialize(jps, instance);
        }


        public static T DeserializeFromJsonFile<T>(this ISerializationContainer sc, string fileName)
        {
            using (var file = File.OpenText(fileName))
            using (var reader = new JsonTextReader(file))
            {
                reader.Read();
                return sc.DeserializeFromJsonReader<T>(reader);
            }
        }


        public static T DeserializeFromJsonReader<T>(this ISerializationContainer sc, JsonReader reader)
        {
            var jpds = new JsonPropertyDeserializer(reader);

            return sc.Deserialize<T, IPropertyDeserializer>(jpds);
        }
    }
}
