using Fact.Extensions.Collection;
using Fact.Extensions.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Tests
{
    [TestClass]
    public class PersistorTest
    {
        public class TestRecord2
        {
            internal const string FIELD1_INITIAL_VALUE = "I am field 1";
            internal const string FIELD2_INITIAL_VALUE = "I am field 2";

            [Persist]
            internal string field1 = FIELD1_INITIAL_VALUE;

            internal string field2 = FIELD2_INITIAL_VALUE;

            internal int field3 = 3;
        }


        class TestRecord2Persistor : Persistor
        {
            //readonly IPropertySerializer propertySerializer;
            //readonly IPropertyDeserializer propertyDeserializer;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            public void Persist(ref string field2, ref int field3)
            {
                if(Mode == ModeEnum.Serialize)
                {
                    dictionary["field2_renamed"] = field2;
                    //propertySerializer.Set("field2_renamed", field2);
                }
                else
                {
                    // TODO:
                    // beware that field3 not getting assigned here essentially zeroes it
                    // add an on-by-default feature which reads out field values and stuffs them
                    // into propertyValues so that the defaults match what's in the instance already
                    field2 = (string) dictionary["field2_renamed"];
                    //field2 = propertyDeserializer.Get<string>("field2_renamed");
                }
            }
        }

        [TestMethod]
        public void JsonPersistorTest()
        {
            var testRecord = new TestRecord2();
            var stringWriter = new StringWriter();
            var jsonTextWriter = new JsonTextWriter(stringWriter);
            Func<JsonWriter> writerFactory = () => jsonTextWriter;
            Func<JsonReader> readerFactory = () =>
            {
                var stringReader = new StringReader(stringWriter.ToString());
                var jsonTextReader = new JsonTextReader(stringReader);
                jsonTextReader.Read();
                return jsonTextReader;
            };

            var p = new JsonReflectionPersistor(readerFactory, writerFactory);

            p.Mode = Persistor.ModeEnum.Serialize;

            p.Persist(testRecord);

            var written = stringWriter.ToString();

            p.Mode = Persistor.ModeEnum.Deserialize;

            testRecord.field1 = "Got blasted over";
            p.Persist(testRecord);

            Assert.AreEqual(TestRecord2.FIELD1_INITIAL_VALUE, testRecord.field1);
        }


        [TestMethod]
        public void JsonPersistor2Test()
        {
            var testRecord = new TestRecord2();
            var _p = new TestRecord2Persistor();
            var p = new RefPersistor(_p);

            p.Serialize(testRecord);

            testRecord.field2 = "Got blasted over";
            p.Deserialize(testRecord);

            Assert.AreEqual(TestRecord2.FIELD2_INITIAL_VALUE, testRecord.field2);
        }


        [TestMethod]
        public void JsonPersistor3Test()
        {
            /*
            var testRecord = new TestRecord2();
            var _p = new TestRecord2Persistor();
            // ERROR: following line has a method group conversion error, even though there's only one Persist
            // method in this class.  
            var p = new Method3DelegatePersistor(_p.Persist);

            p.Serialize(testRecord);

            testRecord.field2 = "Got blasted over";
            p.Deserialize(testRecord);

            Assert.AreEqual(TestRecord2.FIELD2_INITIAL_VALUE, testRecord.field2);
            */
        }


        public class TestRecord2Container
        {
            internal List<TestRecord2> records = new List<TestRecord2>();

            public TestRecord2Container()
            {
                records.Add(new TestRecord2());
                records.Add(new TestRecord2() { field1 = "record#2" });
                records.Add(new TestRecord2() { field1 = "record#3" });
            }
        }


        public class TestRecord2ContainerJsonPersistor : Persistor
        {
            readonly string fileName = "test.json";

            public void Persist(ref List<TestRecord2> records)
            {
                if(Mode == ModeEnum.Serialize)
                {
                    using (StreamWriter file = File.CreateText(fileName))
                    using (var writer = new JsonTextWriter(file))
                    {
                        writer.WriteStartArray();
                        //var p = new JsonReflectionPersistor_OLD(null, () => writer);
                        var p = new JsonReflectionPersistor(null, () => writer);
                        p.Mode = ModeEnum.Serialize;
                        foreach (var item in records)
                        {
                            p.Persist(item);
                        }
                    }
                }
                else
                {
                    using (StreamReader file = File.OpenText(fileName))
                    using (var reader = new JsonTextReader(file))
                    {
                        var p = new JsonReflectionPersistor(() => reader, null);
                        p.Mode = ModeEnum.Deserialize;
                        reader.Read();
                        Debug.Assert(reader.TokenType == JsonToken.StartArray);
                        reader.Read();
                        records = new List<Tests.PersistorTest.TestRecord2>();
                        while (reader.TokenType != JsonToken.EndArray)
                        {
                            //Debug.Assert(reader.TokenType == JsonToken.StartObject);
                            // FIX: could blame JSON for this, but actually its more of a shortcoming
                            // of my persist code
                            var record = new TestRecord2();
                            p.Persist(record);
                        }
                    }

                }
            }
        }

        [TestMethod]
        public void JsonPersistorContainerTest()
        {
            // FIX: Container test bad name (was related to old IoC references no longer present
            // in this test), we're actually testing ref-peering-into-list 
            var _p = new TestRecord2ContainerJsonPersistor();
            var container = new TestRecord2Container();
            var container2 = new TestRecord2Container();
            container2.records.Clear();

            var p = new RefPersistor(_p);

            p.Serialize(container);

            // TODO: Need to change JsonPropertySerializers to 'expect token is already present' mode instead
            // of 'read once to get token' mode for this to work right.
            p.Deserialize(container);
        }


        [TestMethod]
        public void PersistorContainer2Test()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddPersistorFactory();
            var sp = serviceCollection.BuildServiceProvider();

            var pf = sp.GetRequiredService<IPersistorFactory>();
            pf.AddRefPersistor<TestRecord2, TestRecord2Persistor>();

            var p = (PersistorShim<TestRecord2>)sp.GetService<IPersistor<TestRecord2>>();

            Assert.AreEqual(typeof(RefPersistor), p.Persistor.GetType());
        }

        public class ExperimentalContext : IPersistorContext<TestRecord2, object>
        {
            public IFactory<TestRecord2> InstanceFactory { get; set; }
            public object Context { get; set; }

            public TestRecord2 Instance { get; set; }

            public Persistor.ModeEnum Mode { get; set; }
        }

        [TestMethod]
        public void VariableDestinationPersistorTest()
        {

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddJsonPersistorFactory();
            var sp = serviceCollection.BuildServiceProvider();

            var pf = sp.GetRequiredService<IPersistorFactory>();

            var p = sp.GetService<IPersistor<TestRecord2>>();

            //var _context = sp.GetService<IPersistorContext<TestRecord2, object>>();

            var context = new ExperimentalContext() { Instance = new TestRecord2() };

            context.Context = "file2.json";

            p.Serialize(context);
        }
    }
}
