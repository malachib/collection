using Fact.Extensions.Serialization.Newtonsoft;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Tests
{
    [TestClass]
    public class JsonSerializationTests
    {
        public class TestRecord
        {
            public string Name { get; set; }
            public string Color { get; set; }
        }

        [TestMethod]
        public void JsonTest()
        {
            var sm = new JsonSerializationManager();
            var testRecord = new TestRecord
            {
                Name = "Fred",
                Color = "Blue"
            };

            var output1 = sm.SerializeToString(testRecord, typeof(TestRecord), System.Text.Encoding.ASCII);
            var badOutput = sm.SerializeToString(testRecord, typeof(TestRecord), System.Text.Encoding.Unicode);
            var byteArray = sm.SerializeToByteArray(testRecord, typeof(TestRecord));

            var encoding = new System.Text.ASCIIEncoding();
            var output = encoding.GetString(byteArray);

            var output3 = sm.Deserialize(byteArray, typeof(TestRecord));
        }
    }
}
