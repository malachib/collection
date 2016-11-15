using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Tests
{
    [TestClass]
    public class MessagePackTests
    {
        [TestMethod]
        public void MessagePackTest()
        {
            var sm = new MessagePack.MessagePackSerializationManager();

            var testRecord = new TestRecord
            {
                Name = "Fred",
                Color = "Blue"
            };

            var output = sm.SerializeToByteArray(testRecord);
            var output1 = sm.SerializeToString(testRecord);
            var reverted = sm.Deserialize<TestRecord>(output);

            Assert.AreEqual(testRecord.Name, reverted.Name);
        }
    }
}
