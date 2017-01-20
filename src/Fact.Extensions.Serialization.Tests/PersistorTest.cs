using Fact.Extensions.Collection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Tests
{
    [TestClass]
    public class PersistorTest
    {
        class TestRecord2
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
                return jsonTextReader;
            };

            var p = new JsonPersistor(readerFactory, writerFactory);

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
            var p = new Method3Persistor(_p);

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



        [TestMethod]
        public void JsonPersistorContainerTest()
        {
            // since we don't have a full IoC container with named resolution , and/or
            // the resolution we want to do is kind of a type factory anyway, try to 
            // use the factory pattern (ala ILoggerFactory) with an IServiceProvider
            // techniques discussed here:
            // http://stackoverflow.com/questions/39029344/factory-pattern-with-open-generics
            // http://dotnetliberty.com/index.php/2016/05/09/asp-net-core-factory-pattern-dependency-injection
            var serviceCollection = new ServiceCollection();
            var sd = new ServiceDescriptor(typeof(Persistor<>), provider =>
            {
                return null;
            }, ServiceLifetime.Singleton);
            serviceCollection.AddMethod3Persistor<TestRecord2>(new TestRecord2Persistor());
            //serviceCollection.AddSingleton(new Persistor<TestRecord2>())
            //serviceCollection.Append(new Srev)
        }
    }
}
