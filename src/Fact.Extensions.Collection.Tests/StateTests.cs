using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection.Tests
{
    [TestClass]
    public class StateTests
    {
        // Because WaitFor won't work due to the way value vs reference passing works
#if UNUSED
        [TestMethod]
        public void StateWaitForTest()
        {
            var s = new State<int>();

            Task.Run(async () =>
            {
                await Task.Delay(250);
                s.Value = 3;
                await Task.Delay(250);
                s.Value = 5;
            });

            Task.Run(async () =>
            {
                await s.WaitFor(x => x == 5);

            }).Wait();
        }
#endif
    }
}
