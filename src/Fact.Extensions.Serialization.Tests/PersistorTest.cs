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
            [Persist]
            internal string field1 = FIELD1_INITIAL_VALUE;

            internal string field2 = "I am field 2";

            internal int field3 = 3;
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
    }
}
