using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Experimental.Tests
{
    [TestClass]
    public class AsyncTests
    {
        [TestMethod]
        public void AsyncManualWaitEventTest()
        {
            var amwe = new AsyncManualWaitEvent();
            DateTime timestamp = DateTime.MinValue;

            bool result1 = Task.Run(async () =>
            {
                await Task.Delay(500);
                timestamp = DateTime.Now;
                amwe.Set();

            }).Wait(2000);

            bool result2 = Task.Run(async () =>
            {
                await amwe.WaitAsync();
                var elapsed = DateTime.Now.Subtract(timestamp);

                // a little fuzzy, but should be a good test
                Assert.IsTrue(elapsed.TotalMilliseconds < 100);

            }).Wait(2000);

            bool result3 = Task.Run(async () =>
            {
                await amwe.WaitAsync();
                var elapsed = DateTime.Now.Subtract(timestamp);

                // a little fuzzy, but should be a good test
                Assert.IsTrue(elapsed.TotalMilliseconds < 100);

            }).Wait(2000);

            Assert.IsTrue(result1, "Timed out 1");
            Assert.IsTrue(result2, "Timed out 2");
            Assert.IsTrue(result3, "Timed out 3");
        }


        [TestMethod]
        public void AsyncLockTest()
        {
            var al = new AsyncLock();
            int counter = 0;
            DateTime timestamp = DateTime.MinValue;

            bool result1 = Task.Run(async () =>
            {
                await Task.Delay(250);
                using (await al.LockAsync())
                {
                    Assert.AreEqual(0, counter);
                    counter++;
                    await Task.Delay(250);
                }
                timestamp = DateTime.Now;

            }).Wait(2000);

            bool result2 = Task.Run(async () =>
            {
                using (await al.LockAsync())
                {
                    Assert.AreEqual(1, counter);
                    counter++;
                    var elapsed = DateTime.Now.Subtract(timestamp);
                    Assert.IsTrue(elapsed.TotalMilliseconds < 100);
                }

            }).Wait(2000);
        }
    }
}
