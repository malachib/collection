using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Synthetic.xUnit
{
    public class FixtureBase : 
        IDisposable,
        IAsyncDisposable
    {
        readonly protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public virtual void ConfigureServices(IServiceCollection sc)
        {
            sc.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddDebug();
            });
        }


        public virtual void Configure(IServiceProvider services)
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


        public FixtureBase()
        {
            var sc = new ServiceCollection();
            ConfigureServices(sc);
            Configure(Services = sc.BuildServiceProvider());
        }
    }
}
