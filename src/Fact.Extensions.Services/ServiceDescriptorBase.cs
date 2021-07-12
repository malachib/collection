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

        /// <summary>
        /// Wire up specified service to fire pertinent events back to this service descriptor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="service"></param>
        protected virtual void SetupServiceEvents(ServiceContext context, IService service)
        {
            SetupEvents(service);

            // These await Startup calls specifically trail the stock-standard LifecycleStatus
            // update call, so that first the event state is fired off (Online, Awake) then
            // sequentially afterwards an async startup happens to actually start the service
            // back up again from its semi-shutdown state

            if (service is IOnlineEvents oe)
            {
                // Beware, this might not be the SP you are after!
                oe.Online += async () => await Startup(context);
            }

            if (service is ISleepableEvents se)
            {
                // Beware, this might not be the SP you are after!
                se.Awake += async () => await Startup(context);
            }

            if (service is IExceptionEventProvider ep)
            {
                ep.ExceptionOccurred += e =>
                {
                    Exception = e; // important that we assign this *before* LifecycleStatus
                    LifecycleStatus = LifecycleEnum.Error;
                };
            }
        }

        internal ServiceDescriptorBase(IServiceProvider sp, IService service, string name)
        {
            var context = new ServiceContext(sp, this);
            this.logger = sp.GetRequiredService<ILogger<ServiceDescriptorBase>>();
            this.service = service;

            SetupServiceEvents(context, service);

            Name = name;
        }


        internal ServiceDescriptorBase(ServiceContext context, IService service, string name)
        {
            var sp = context.ServiceProvider;
            this.logger = sp.GetRequiredService<ILogger<ServiceDescriptorBase>>();
            this.service = service;

            SetupServiceEvents(context, service);

            Name = name;
        }

        public IService Service => service;

        public string Name { get; }

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
        public ServiceDescriptorBase(ServiceContext context, TService service, string name) :
            base(context.ServiceProvider, service, name)
        { }

        public ServiceDescriptorBase(IServiceProvider sp, TService service, string name) :
            base(sp, service, name)
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


        protected void SetupEvents(object eventSource)
        {
            // NOTE: perhaps not best location for this, but we can capture idea here at least
            if (eventSource is IOnlineEvents oe)
            {
                oe.Online += () => LifecycleStatus = LifecycleEnum.Online;
                oe.Offline += () => LifecycleStatus = LifecycleEnum.Offline;
            }

            if (eventSource is ISleepableEvents se)
            {
                se.Sleeping += () => LifecycleStatus = LifecycleEnum.Sleeping;
                se.Slept += () => LifecycleStatus = LifecycleEnum.Slept;
                se.Awake += () => LifecycleStatus = LifecycleEnum.Awake;
                se.Waking += () => LifecycleStatus = LifecycleEnum.Waking;
            }
        }

    }
}
