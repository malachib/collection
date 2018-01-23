using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Fact.Extensions.Services.Tests")]


namespace Fact.Extensions.Services
{
    public interface IService :
        ILifecycle,
        INamed
    {
    }


    /// <summary>
    /// TODO: Migrate this into IService itself, combining with ILifecycle
    /// </summary>
    public interface IServiceExtended : IService
    {
        Task Startup(ServiceContext context);

        Task Shutdown(ServiceContext context);
    }


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
    public interface IServiceDescriptor : IServiceDescriptorBase, IServiceExtended
    {
        Exception Exception { get; }

        /// <summary>
        /// List of services which depend on this one - specifically,
        /// when it's time for this service to shut down, it has to wait
        /// until dependers list is completely free.  This way, dependers
        /// can do what they need to do with this service before allowing
        /// it to shutdown
        /// </summary>
        //ICollection<IServiceDescriptor> Dependers { get; }
    }


    public interface IServiceDescriptor<TService> : IServiceDescriptor
        where TService : IService
    {
        new TService Service { get; }
    }


    /// <summary>
    /// Basic context helper to pass around between async/await tasks
    /// </summary>
    public class AsyncContext
    {
        public AsyncContext(IProgress<float> progress)
        {
            Progress = progress;
        }

        public AsyncContext(CancellationToken token, IProgress<float> progress)
        {
            CancellationToken = token;
            Progress = progress;
        }

        public AsyncContext(AsyncContext copyFrom)
        {
            Progress = copyFrom.Progress;
            CancellationToken = copyFrom.CancellationToken;
        }


        protected AsyncContext() { }

        /// <summary>
        /// from 0-100, not 0-1
        /// </summary>
        public IProgress<float> Progress { get; protected set; }
        public CancellationToken CancellationToken { get; set; }
    }


    public class ServiceContext : AsyncContext
    {
        public ServiceContext(IServiceProvider serviceProvider, IProgress<float> progress = null)
            : base(progress)
        {
            ServiceProvider = serviceProvider;
        }


        public ServiceContext(IServiceProvider serviceProvider, IServiceDescriptor descriptor)
            : this(serviceProvider)
        {
            Descriptor = descriptor;

            if (descriptor is IProgress<float> descriptorWithProgress)
            {
                Progress = descriptorWithProgress;
            }
        }


        public ServiceContext(IServiceProvider serviceProvider, 
            CancellationToken cancellationToken, 
            IServiceDescriptor descriptor = null) :
            this(serviceProvider, descriptor)
        {
            CancellationToken = cancellationToken;
        }

        public ServiceContext(ServiceContext copyFrom, IServiceDescriptor descriptor) : base(copyFrom)
        {
            Descriptor = descriptor;

            if (copyFrom.Progress == null && descriptor is IProgress<float> descriptorWithProgress)
            {
                Progress = descriptorWithProgress;
            }
        }


        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Descriptor which is 1:1 associated with the running service
        /// </summary>
        public IServiceDescriptor Descriptor { get; set; }
    }
}
