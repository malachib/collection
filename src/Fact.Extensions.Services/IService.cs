using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Fact.Extensions.Services.Tests")]


namespace Fact.Extensions.Services
{
    public interface IService :
        ILifecycle,
        INamed
    { }


    /// <summary>
    /// More or less a lifecycle descriptor with a service providing interface
    /// </summary>
    public interface IServiceDescriptorBase : ILifecycleDescriptor
    {
        IService Service { get; }
    }


    /// <summary>
    /// A service descriptor provides metadata about a service, namely status information exposed by
    /// ILifecycleDescriptor - things that are pertinent to a service, but it may not actually maintain
    /// on its own
    /// Furthermore, we implement IService interface itself as a light facade around our service
    /// mainly to assist in the aforementioned, tracking service state
    /// </summary>
    public interface IServiceDescriptor : IServiceDescriptorBase, IService
    {
    }

}
