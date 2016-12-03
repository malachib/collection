using Fact.Extensions.Serialization.Newtonsoft;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Tests
{
    public class TestRecord
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }

    [TestClass]
    public class JsonSerializationTests
    {
        [TestMethod]
        public void JsonTest()
        {
            var sm = new JsonSerializationManager();
            var testRecord = new TestRecord
            {
                Name = "Fred",
                Color = "Blue"
            };

            var output1 = sm.SerializeToString(testRecord);
            var badOutput = sm.SerializeToString(testRecord, typeof(TestRecord), Encoding.Unicode);
            var byteArray = sm.SerializeToByteArray(testRecord, typeof(TestRecord));

            var output = Encoding.UTF8.GetString(byteArray);

            var output3 = sm.Deserialize(byteArray, typeof(TestRecord));
            var output4 = sm.Deserialize<TestRecord>(output1);

            Assert.AreEqual(output, output1);
        }


#if NETCOREAPP1_2
        [TestMethod]
        public void JsonAsyncTest()
        {
            var sm = new JsonSerializationManagerAsync();
        }
#endif

        [TestMethod]
        public void BsonTest()
        {
            var sm = new BsonSerializationManager();
            var testRecord = new TestRecord
            {
                Name = "Fred",
                Color = "Blue"
            };

            var output1 = sm.SerializeToByteArray(testRecord);
            var output2 = sm.Deserialize<TestRecord>(output1);

            Assert.AreEqual(testRecord.Name, output2.Name);
        }


        [TestMethod]
        public void ReadonlyStringStreamTest()
        {
            var encoding = Encoding.ASCII;
            var original = "testdata";
            var stream = new ReadonlyStringStream(original, encoding);
            
            var bytes = stream.Read().ToArray();

            var converted = encoding.GetString(bytes);

            Assert.AreEqual(original, converted);

            // Code contracts not compiling in just yet
            //new ReadonlyStringStream(null, null);
        }
    }
}
