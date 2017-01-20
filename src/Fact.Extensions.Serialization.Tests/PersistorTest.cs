using Fact.Extensions.Collection;
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
            var p = new JsonPersistor(null, null, new TestRecord2Persistor());

            p.Mode = Persistor.ModeEnum.Serialize;
            p.Persist(testRecord);

            p.Mode = Persistor.ModeEnum.Deserialize;

            testRecord.field2 = "Got blasted over";
            p.Persist(testRecord);
            Assert.AreEqual(TestRecord2.FIELD2_INITIAL_VALUE, testRecord.field1);
        }
    }
}
