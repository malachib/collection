using Fact.Extensions.Collection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
    /// <summary>
    /// Wrapper class which wraps up a provided service and combines it with a ILifecycleDescriptor
    /// helper - so that the underlying service itself is alleviated from managing that plubming
    /// </summary>
    /// <remarks>
    /// I see a day where ServiceManager and ServiceDescriptorBase are more alike
    /// </remarks>
    internal class ServiceDescriptorBase :
        LifecycleDescriptorBase,
        IServiceDescriptor
    {
        readonly IService service;
        readonly ILogger logger;

        public Exception Exception { get; private set; }

        internal ServiceDescriptorBase(IServiceProvider sp, IService service)
        {
            this.logger = sp.GetService<ILogger<ServiceDescriptorBase>>();
            this.service = service;
            var context = new ServiceContext(sp, this);

            // NOTE: perhaps not best location for this, but we can capture idea here at least
            if (service is IOnlineEvents oe)
            {
                oe.Online += async () =>
                {
                    LifecycleStatus = LifecycleEnum.Online;
                    // Beware, this might not be the SP you are after!
                    await Startup(context);
                };
                oe.Offline += () => LifecycleStatus = LifecycleEnum.Offline;
            }

            if (service is ISleepableEvents se)
            {
                se.Sleeping += () => LifecycleStatus = LifecycleEnum.Sleeping;
                se.Slept += () => LifecycleStatus = LifecycleEnum.Slept;
                se.Awake += async () =>
                {
                    LifecycleStatus = LifecycleEnum.Awake;
                    // Beware, this might not be the SP you are after!
                    await Startup(context);
                };
                se.Waking += () => LifecycleStatus = LifecycleEnum.Waking;
            }

            if (service is IExceptionEventProvider ep)
            {
                ep.ExceptionOccurred += e =>
                {
                    Exception = e;
                    LifecycleStatus = LifecycleEnum.Error;
                };
            }
        }

        public IService Service => service;

        public string Name => service.Name;

        public virtual async Task Shutdown(ServiceContext context)
        {
            logger.LogTrace($"Shutdown: Initiated on {Name}");

            try
            {
                LifecycleStatus = LifecycleEnum.Stopping;
                await service.Shutdown(context);
                LifecycleStatus = LifecycleEnum.Stopped;
            }
            catch (Exception e)
            {
                Exception = e;
                LifecycleStatus = LifecycleEnum.Error;
                logger.LogError(0, e, $"Shutdown failed: {Name}");
            }
        }

        public virtual async Task Startup(ServiceContext context)
        {
            logger.LogTrace($"Startup: {Name}");

            try
            {
                LifecycleStatus = LifecycleEnum.Starting;
                await service.Startup(context);
                LifecycleStatus = LifecycleEnum.Started;
                LifecycleStatus = LifecycleEnum.Running;
            }
            catch (Exception e)
            {
                Exception = e;
                logger.LogError(0, e, $"Startup failed: {Name}");
                LifecycleStatus = LifecycleEnum.Error;
            }
        }
    }


    internal class ServiceDescriptorBase<TService> : ServiceDescriptorBase,
        IServiceDescriptor<TService>
        where TService : IService
    {
        public ServiceDescriptorBase(ServiceContext context, IService service) :
            base(context.ServiceProvider, service)
        { }

        public ServiceDescriptorBase(IServiceProvider sp, IService service) :
            base(sp, service)
        { }

        TService IServiceDescriptor<TService>.Service => (TService)Service;
    }


    public class LifecycleDescriptorBase : ILifecycleDescriptor
    {
        public LifecycleEnum LifecycleStatus
        {
            get => lifecycle;
            protected set => lifecycle.Value = value;
        }

        State<LifecycleEnum> lifecycle;

        public event Action<object> LifecycleStatusUpdated;

        protected LifecycleDescriptorBase()
        {
            lifecycle.Changed += v => LifecycleStatusUpdated?.Invoke(this);
        }
    }
}
