using Fact.Extensions.Experimental;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services.Tests
{
    [TestClass]
    public class ExperimentalTests
    {
        [TestMethod]
        public void AwaitableCollectionTest()
        {
            var list = new LinkedList<int>();
            var ac = new AwaitableCollection<int>(list);

            ac.Add(5);

            Task.Run(async () =>
            {
                await ac.AwaitEmpty();
            }).Wait(1000);
        }


        [TestMethod]
        public void AwaitableCollection2Test()
        {
            var list = new LinkedList<int>();
            var ac = new AwaitableCollection2<int>(list);
            var ct = new CancellationTokenSource().Token;

            ac.Add(5);

            var task = Task.Run(async () =>
            {
                await ac.AwaitEmpty(ct);
                Assert.AreEqual(0, ac.Count);
            });

            Thread.Sleep(250);
            //Task.Delay(500);
            ac.Remove(5);

            Assert.IsTrue(task.Wait(1000));

        }


        [TestMethod]
        public void ExperimentalServiceManagementTest()
        {
            var sc = new ServiceCollection();
            sc.AddServiceMangement();
            sc.AddSingleton<DummyService>();
            var sp = sc.BuildServiceProvider();
            var sd = sp.GetService<IServiceDescriptor<DummyService>>();
            var context = new ServiceContext(sp, sd);
            sd.Startup(context);
        }
    }
}
