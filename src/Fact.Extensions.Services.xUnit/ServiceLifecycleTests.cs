using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fact.Extensions.Services.xUnit
{
    public class ServiceLifecycleTests : IClassFixture<Fixture>
    {
        readonly IServiceProvider services;

        public ServiceLifecycleTests(Fixture fixture)
        {
            services = fixture.Services;
        }

        [Fact]
        public async Task Test1()
        {
            var sm = new ServiceManager(services, "test1");
            var context = services.GetRequiredService<ServiceContext>();
            var dummyService = new Synthetic.DummyService(services);

            await sm.Startup(context);
            await sm.Shutdown(context);
        }
    }
}
