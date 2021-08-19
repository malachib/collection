using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Services.xUnit
{
    public class Fixture : Fact.Extensions.Synthetic.xUnit.FixtureBase
    {
        public override void ConfigureServices(IServiceCollection sc)
        {
            base.ConfigureServices(sc);

            sc.AddSingleton(services => new ServiceContext(services, cancellationTokenSource.Token));
        }
    }
}
