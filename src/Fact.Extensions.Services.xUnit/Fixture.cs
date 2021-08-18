using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services.xUnit
{
    public class Fixture : 
        IDisposable,
        IAsyncDisposable
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public void ConfigureServices(IServiceCollection sc)
        {
            sc.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddDebug();
            });
            sc.AddSingleton(services => new ServiceContext(services, cancellationTokenSource.Token));
        }


        public void Configure(IServiceProvider services)
        {

        }

        // NOTE: xUnit does not appear to call this one
        public ValueTask DisposeAsync()
        {
            cancellationTokenSource.Cancel();
            return new ValueTask();
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
        }

        public IServiceProvider Services { get; }


        public Fixture()
        {
            var sc = new ServiceCollection();
            ConfigureServices(sc);
            Configure(Services = sc.BuildServiceProvider());
        }
    }
}
