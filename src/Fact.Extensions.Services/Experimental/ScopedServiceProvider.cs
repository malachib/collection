using System;
using System.Collections.Generic;
using System.Text;
#if NETSTANDARD2_0
using System.Linq;
#else
using Fact.Extensions.Collection.Compat;
#endif

using Fact.Extensions.Collection;

namespace Fact.Extensions.Services.Experimental
{
    public interface ITenantServiceProvider : INamed, IServiceProvider
    {

    }

    /// <summary>
    /// Kind of like an aggregate really, except each scoped service provider can itself
    /// be a service provider too
    /// </summary>
    public class TenantServiceProvider :
        NamedChildCollection<ITenantServiceProvider>,
        ITenantServiceProvider
    {
        readonly IServiceProvider serviceProvider;

        public TenantServiceProvider(string name, IServiceProvider serviceProvider = null) : base(name)
        {
            this.serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            foreach(var child in Children.Prepend(serviceProvider))
            {
                object service = child?.GetService(serviceType);

                if (service != null) return service;
            }

            return null;
        }
    }


    public static class ScopedServiceProviderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ssp"></param>
        /// <param name="name"></param>
        /// <param name="sp"></param>
        public static void Add(this TenantServiceProvider ssp, string name, IServiceProvider sp)
        {
            ssp.AddChild(new TenantServiceProvider(name, sp));
        }
    }
}
