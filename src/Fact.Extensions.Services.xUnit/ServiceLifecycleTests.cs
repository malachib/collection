using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using FluentAssertions;
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
        public async Task OnlineOfflineTest()
        {
            var sm = new ServiceManager(services, "test1");
            var context = services.GetRequiredService<ServiceContext>();
            var dummyService = new Synthetic.DummyService(services);
            
            var tcs = new TaskCompletionSource<bool>(context.CancellationToken);

            dummyService.Generic += () => tcs.SetResult(true);

            var sd = sm.AddService(dummyService);

            LifecycleEnum lastStatus = LifecycleEnum.Unstarted;
            bool firstRun = true;

            sd.LifecycleStatusUpdated += sender =>
            {
                switch (sd.LifecycleStatus)
                {
                    case LifecycleEnum.Offline:
                        lastStatus.Should().Be(LifecycleEnum.Running);
                        break;
                    
                    case LifecycleEnum.Online:
                        lastStatus.Should().Be(LifecycleEnum.Offline);
                        break;
                    
                    case LifecycleEnum.Running:
                        if (firstRun)
                        {
                            lastStatus.Should().Be(LifecycleEnum.Started);
                            firstRun = false;
                        }
                        else
                            lastStatus.Should().Be(LifecycleEnum.Online);
                        break;
                    
                    case LifecycleEnum.Starting:
                        firstRun.Should().BeTrue();
                        lastStatus.Should().Be(LifecycleEnum.Unstarted);
                        break;
                    
                    case LifecycleEnum.Started:
                        // Making sure that online chain doesn't come through here
                        firstRun.Should().BeTrue();
                        lastStatus.Should().Be(LifecycleEnum.Starting);
                        break;
                    
                    default:
                        break;
                }

                lastStatus = sd.LifecycleStatus;
            };

            await sm.Startup(context);

            await tcs.Task;
            
            await sm.Shutdown(context);
            
            if (sd.Exception != null)
                throw sd.Exception;
        }
    }
}
