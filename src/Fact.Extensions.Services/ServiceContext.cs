using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
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


    public interface IServiceContext
    {
        IServiceProvider ServiceProvider { get; }
    }


    public class ServiceContext : AsyncContext, IServiceContext
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
            ServiceProvider = copyFrom.ServiceProvider;

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


    public static class IServiceContextExtensions
    {
        /// <summary>
        /// Get a service of type T from context's service provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceContext"></param>
        /// <returns></returns>
        public static T GetService<T>(this IServiceContext serviceContext)
        {
            return serviceContext.ServiceProvider.GetService<T>();
        }
    }
}
