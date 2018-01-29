using Fact.Extensions.Collection;
using Fact.Extensions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if ALPHA
namespace Fact.Extensions.Services.Experimental
{
    /// <summary>
    /// ALPHA pre release experimental shim service
    /// Name probably will change
    /// </summary>
    public class ShimService : IService
    {
        readonly string name;

        public string Name => name;

        readonly Func<ServiceContext, Task> startup;
        readonly Func<ServiceContext, Task> shutdown;

        public ShimService(string name,
            Func<ServiceContext, Task> startup,
            Func<ServiceContext, Task> shutdown)
        {
            this.name = name;
            this.startup = startup;
            this.shutdown = shutdown;
        }

        public Task Shutdown(ServiceContext context) => shutdown(context);

        public Task Startup(ServiceContext context) => startup(context);
    }


    public static class ShimServiceExtensions
    {
        /// <summary>
        /// Add a service wholly comprised of startup and showdown delegates
        /// </summary>
        /// <param name="childCollection"></param>
        /// <param name="name"></param>
        /// <param name="startup"></param>
        /// <param name="shutdown"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IServiceDescriptor<ShimService> AddService(this IChildCollection<IServiceDescriptor> childCollection,
            string name,
            Func<ServiceContext, Task> startup,
            Func<ServiceContext, Task> shutdown,
            ServiceContext context)
        {
            return childCollection.AddService(new ShimService(name, startup, shutdown), context);
        }
    }
}
#endif