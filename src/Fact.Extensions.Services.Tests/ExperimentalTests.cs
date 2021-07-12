using Fact.Extensions.Experimental;
using Fact.Extensions.Services.Experimental;

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


        // Broken, not sure why.
        // Seemed to be working pre 07JUL21
#if UNUSED
        [TestMethod]
        public void ExperimentalServiceManagementTest()
        {
            var sc = new ServiceCollection();
            sc.AddLogging();
            sc.AddServiceMangement();
            sc.AddSingleton<DummyService>();
            var sp = sc.BuildServiceProvider();
            var sd = sp.GetRequiredService<IServiceDescriptor<DummyService>>();
            var context = new ServiceContext(sp, sd);
            sd.Startup(context);
        }
#endif


        [TestMethod]
        public void TenantServiceProviderTest()
        {
            var sc1 = new ServiceCollection();
            sc1.AddSingleton("test 1");
            var sc2 = new ServiceCollection();
            sc2.AddSingleton("test 2");
            var sctop = new ServiceCollection();
            //sctop.AddSingleton("top");

            var ssp = new TenantServiceProvider("toplevel", sctop.BuildServiceProvider());

            // NOTE! build service provider has a scopes ability... 
            // but that's more of an instance-lifetyime thing after you discover it
            ssp.Add("test1-provider", sc1.BuildServiceProvider());
            ssp.Add("test2-provider", sc2.BuildServiceProvider());

            string test1 = ssp.GetService<string>();

            Assert.AreEqual("test 1", test1);
        }
    }
}
