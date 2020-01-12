using Fact.Extensions.Experimental;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services.Tests
{
    [TestClass]
    public class LifecycleTest
    {
        [TestMethod]
        public void BasicLifecycleTest()
        {
            var sc = new ServiceCollection();
            var sp = sc.BuildServiceProvider();
            var lm = new LifecycleManager(sp);
        }



        IServiceProvider Setup()
        {
            var sc = new ServiceCollection();
            sc.AddLogging(builder =>
                builder.AddConsole(c =>
                {
                    c.LogToStandardErrorThreshold = LogLevel.Trace;
                })
            );
            var sp = sc.BuildServiceProvider();
            //var lf = sp.GetService<ILoggerFactory>();
            //lf.AddConsole
            //lf.AddConsole(LogLevel.Trace);
            return sp;
        }


        (ServiceContext context, CancellationTokenSource cts) Setup2()
        {
            var cts = new CancellationTokenSource();
            var context = new ServiceContext(Setup(), cts.Token);

            return (context, cts);
        }

        [TestMethod]
        public void BasicServiceManagerTest()
        {
            var sp = Setup();
            var sm = new ServiceManager(sp, "parent");
            var childSm = new ServiceManager(sp, "child");
            var dummyService = new DummyService(sp);
            var dummySem = new SemaphoreSlim(0, 1);

            sm.AddChild(childSm);
            var dummyServiceDescriptor = sm.AddService(dummyService, sp);

            dummyServiceDescriptor.LifecycleStatusUpdated += o =>
            {
                Console.WriteLine($"{DateTime.Now.Millisecond}: dummy = {dummyServiceDescriptor.LifecycleStatus}");
            };

            sm.LifecycleStatusUpdated += o =>
            {
                Console.WriteLine($"{DateTime.Now.Millisecond}: parent: {sm.LifecycleStatus}");
            };

            dummyService.Generic += () => dummySem.Release();

            Assert.AreEqual(LifecycleEnum.Unstarted, sm.LifecycleStatus);

            Task.Run(async () =>
            {
                var context = new ServiceContext(sp);
                await sm.Startup(context);

                Assert.AreEqual(LifecycleEnum.Running, sm.LifecycleStatus);

                childSm.LifecycleStatus = LifecycleEnum.Error;

                Assert.AreEqual(LifecycleEnum.Degraded, sm.LifecycleStatus);

                childSm.LifecycleStatus = LifecycleEnum.Running;

                await dummySem.WaitAsync(10000);

                // FIX: Something is wrong, dummy service is claiming worker
                // is awaiting activation when we reach here
                await sm.Shutdown(context);

            }).Wait();

        }

        [TestMethod]
        public void AsyncContextTest()
        {
            var progress = new Progress<float>();

            progress.ProgressChanged += (o, value) => Console.WriteLine($"Progress %: {value}");
            ServiceContext context = new ServiceContext(Setup(), progress);

            var sp = context.ServiceProvider;

            var sm = new ServiceManager(sp, "parent");
            var childSm = new ServiceManager(sp, "child");
            var dummyService = new DummyService(sp);
            var dummySem = new SemaphoreSlim(0, 1);

            sm.AddChild(childSm);
            sm.AddService(dummyService, sp);

            dummyService.Generic += () => dummySem.Release();

            Task.Run(async () =>
            {
                await sm.Startup(context);

                await dummySem.WaitAsync();

                await sm.Shutdown(context);
            }).Wait(3000);
        }


        public class DummyService2 : WorkerServiceBase
        {
            public override string Name => "Dummy Service 2";

            public DummyService2(ServiceContext context) : base(context) { }

            protected override async Task Worker(ServiceContext context)
            {
                await Task.Delay(250);
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void CancelledServiceTest()
        {
            var cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                var p = Setup();
                ServiceContext context = new ServiceContext(p);
                DummyService2 service = new DummyService2(context);
                var descriptor = new ServiceDescriptorBase(p, service);
                context = new ServiceContext(context, descriptor);
                context.CancellationToken = cts.Token;
                await descriptor.Startup(context);
                //await descriptor.WaitFor(x => x == LifecycleEnum.Running, cts.Token);
                await Task.Delay(500);
                Assert.AreEqual(LifecycleEnum.Error, descriptor.LifecycleStatus);
                await descriptor.Shutdown(context);
            }).Wait(2000);
        }


        class DummyWorkerItemAcquirerService : WorkerItemAcquirerService<int>
        {
            int counter = 0;

            public override string Name => "Dummy worker item acquirer";

            protected override int GetItem(CancellationToken ct)
            {
                Thread.Sleep(250);
                return counter++;
            }

            internal DummyWorkerItemAcquirerService(ServiceContext context) :
                base(context) {}
        }

        [TestMethod]
        public void WorkerItemAcquirerServiceTest()
        {
            var result = Setup2();
            var context = result.context;
            var service = new DummyWorkerItemAcquirerService(context);
            var descriptor = new ServiceDescriptorBase(context.ServiceProvider, service);

            bool completedOnTime = Task.Run(async () =>
            {
                service.ItemAcquired += v => Console.WriteLine($"Got item: {v}");
                await descriptor.Startup(context);

                await Task.Delay(1500);

                await descriptor.Shutdown(context);

            }).Wait(3000);

            Assert.AreEqual(LifecycleEnum.Stopped, descriptor.LifecycleStatus);
            Assert.IsTrue(completedOnTime, "Did not complete on time");
        }
    }
}
