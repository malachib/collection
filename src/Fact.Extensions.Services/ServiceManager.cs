using Fact.Extensions.Collection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Fact.Extensions.Experimental;

namespace Fact.Extensions.Services
{
    /// <summary>
    /// We have many incarnations of this, here's another
    /// A hierarchical manager which can manage many services inclusive of other servicemanagers
    /// </summary>
    public class ServiceManager :
        NamedChildCollection<IServiceDescriptor>,
        IServiceDescriptor
    {
        readonly ILogger logger;

        /// <summary>
        /// ServiceManager doesn't use this just yet, merely because haven't decided what
        /// best way to utilize it for this class is
        /// </summary>
        public Exception Exception { get; private set; }

        public ServiceManager(IServiceProvider sp, string name, IService self = null) : base(name)
        {
            logger = sp.GetService<ILogger<ServiceManager>>();
            ChildAdded += (o, c) => c.LifecycleStatusUpdated += Child_LifecycleStatusUpdated;
            ChildRemoved += (o, c) => c.LifecycleStatusUpdated -= Child_LifecycleStatusUpdated;
            lifecycle.Changing += (old, @new) => LifecycleStatusUpdating?.Invoke(this, @new);
            lifecycle.Changed += v => LifecycleStatusUpdated?.Invoke(this);
            if(self != null)
            {
                this.self = new ServiceDescriptorBase(sp, self);
            }
        }


        /// <summary>
        /// Normally a service manager serves mainly to look after child services or other child
        /// service managers.  Additionaly though, the service manager can have a 1:1 relationship
        /// itself with a service.  This 'self' service manages its own lifecycle state just like
        /// a child service, and composite takes 'self' lifecycle state into account as if it 
        /// where a child.  So except for startup/shutdown order and general accessibility, 'self'
        /// operates like any other child service
        /// </summary>
        IServiceDescriptor self;

        public IService Service => this;

        State<LifecycleEnum> lifecycle;

        public LifecycleEnum LifecycleStatus
        {
            get => lifecycle;
            // mainly used for unit test access
            internal set => lifecycle.Value = value;
        }

        /// <summary>
        /// Fired when we are updating lifecycle status, but before we've actually updated it
        /// Use this to do any special state-change logic on the consumer side
        /// provided LifecycleEnum is new state, old state will still exist in 'object'
        /// </summary>
        public event Action<ILifecycleDescriptor, LifecycleEnum> LifecycleStatusUpdating;

        public event Action<object> LifecycleStatusUpdated;

        async Task Noop() { }

        public async Task Shutdown(ServiceContext context)
        {
            lifecycle.Value = LifecycleEnum.Stopping;

            // start self awaiter first, but it can finish anytime
            // FIX: Will need provision to "unlock" dependent-on resources,
            // or otherwise (hope we can void this) have a two-phase shutdown
            Task selfAwaiter = self != null ? self.Shutdown(context) : Noop();

            // TODO: Add provision for sequential startup/shutdown also
            var childrenShutdownTasks = Children.Select(x => x.Shutdown(context)).Append(selfAwaiter);

            try
            {
                await Task.WhenAll(childrenShutdownTasks);
                lifecycle.Value = LifecycleEnum.Stopped;
            }
            catch (Exception e)
            {
                logger.LogError(0, e, $"Shutdown: Unable to proceed");
                // If an exception is thrown while shutting down children, an improper shutdown
                // has occurred and this is an error state
                // FIX: Don't want to abort shutdown of other children though, so rework code to
                // accomodate that
                lifecycle.Value = LifecycleEnum.Error;
            }
        }

        /// <summary>
        /// Evaluate all children and return a state which attempts to summarize
        /// their statuses
        /// </summary>
        /// <returns></returns>
        LifecycleEnum AscertainCompositeState()
        {
            LifecycleEnum state = LifecycleEnum.Running;
            IEnumerable<IServiceDescriptor> children;

            if (self != null)
                children = Children.Prepend(self);
            else
                children = Children;

            foreach (var child in children)
            {
                var status = child.LifecycleStatus;

                if(status == LifecycleEnum.PartialRunning)
                {
                    // we keep looking, because degraded state supercedes this one, if we find it
                    state = LifecycleEnum.PartialRunning;
                }
                else if(status == LifecycleEnum.Degraded)
                {
                    // degraded child bubbles up and makes for a degraded parent
                    return LifecycleEnum.Degraded;
                }
                else if (child.IsTransitioning())
                {
                    // child transitioning into or away from running state means
                    // it's basically offline-ish, so report partial running state
                    state = LifecycleEnum.PartialRunning;
                }
                else if (child.IsNotRunning())
                {
                    if (child.LifecycleStatus == LifecycleEnum.Error)
                        return LifecycleEnum.Degraded;

                    // we keep looking, because degraded state supercedes this one, if we find it
                    state = LifecycleEnum.PartialRunning;
                }
            }

            return state;
        }

        private void Child_LifecycleStatusUpdated(object sender)
        {
            var sd = (IServiceDescriptor)sender;

            var status = LifecycleStatus;

            // wait until starting state is over before computing state value for composite
            // so that we don't get a bunch of confusing states if the children are goofed up

            // FIX: Because we exclude stopped here, shutdown failures won't be reported as 
            // an overall error, which they should be
            if (status != LifecycleEnum.Starting &&
                status != LifecycleEnum.Stopping &&
                status != LifecycleEnum.Stopped)
            {
                // TODO: can optimize since we know which child state is changing
                lifecycle.Value = AscertainCompositeState();
            }
        }

        // Copy/paste of regular startup but with extra goodies
        // If we like it, replace primary one with this one
        public async Task Startup(ServiceContext context)
        {
            var progress = context.Progress;
            lifecycle.Value = LifecycleEnum.Starting;

            var children = Children;
            if (self != null) children = children.Prepend(self);

            Action<object> progressObserver = null;

            if (progress != null)
            {
                int progressCount = 0;
                int totalChildren = children.Count();

                progress.Report(0);

                progressObserver = new Action<object>(o =>
                {
                    var child = (ILifecycleDescriptor)o;
                    // TODO: If children themselves also report progress,
                    // we can divide it up here and make the overall 
                    // more smooth (cool!)
                    if (child.LifecycleStatus == LifecycleEnum.Running)
                        progress.Report(100f * ++progressCount / totalChildren);
                });

                foreach (var child in children)
                    child.LifecycleStatusUpdated += progressObserver;
            }

            // TODO: get the cancellation token going either per child
            // or tacked onto WhenAll.  Where's that extension...
            var childrenStartingTasks = children.
                Select(x =>
                {
                    var childContext = new ServiceContext(context, x);
                    return x.Startup(childContext);
                });

            await Task.WhenAll(childrenStartingTasks);

            if (progressObserver != null)
            {
                foreach (var child in children)
                    child.LifecycleStatusUpdated -= progressObserver;
            }

            lifecycle.Value = LifecycleEnum.Started;
            lifecycle.Value = AscertainCompositeState();
        }
    }


    public static class ServiceManagerExtensions
    {
        public static IServiceDescriptor AddService(this ServiceManager serviceManager, IService service, IServiceProvider sp)
        {
            var sd = new ServiceDescriptorBase(sp, service);

            serviceManager.AddChild(sd);

            return sd;
        }


        public static IServiceDescriptor<TService> AddService<TService>(this ServiceManager serviceManager, TService service, IServiceProvider sp)
            where TService : IService
        {
            var sd = new ServiceDescriptorBase<TService>(sp, service);

            serviceManager.AddChild(sd);

            return sd;
        }

        public static IServiceDescriptor<TService> AddService<TService>(this ServiceManager serviceManager, TService service, ServiceContext context)
            where TService: IService
        {
            var sd = new ServiceDescriptorBase<TService>(context, service);

            serviceManager.AddChild(sd);

            return sd;
        }
    }
}
