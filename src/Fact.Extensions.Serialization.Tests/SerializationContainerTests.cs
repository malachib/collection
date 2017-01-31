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
            //sc.Register(s, typeof(TestRecord1));
            sc.container.Register<ISerializer<IPropertySerializer>>(s, key);
            sc.container.Register<IDeserializer<IPropertyDeserializer>>(s, key);
            var ds = sc.GetSerializer<IPropertySerializer>(typeof(TestRecord1));
            using (var file = File.CreateText("temp/serializationContainer2.json"))
            {
                using (var writer = new JsonTextWriter(file))
                {
                    var jps = new JsonPropertySerializer(writer);

                    ds.Serialize(jps, record);
                }
            }
        }
    }
}
