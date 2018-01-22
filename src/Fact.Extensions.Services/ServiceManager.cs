using Fact.Extensions.Collection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Fact.Extensions.Services
{
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


    /// <summary>
    /// Wrapper class which wraps up a provided service and combines it with a ILifecycleDescriptor
    /// helper - so that the underlying service itself is alleviated from managing that plubming
    /// </summary>
    internal class ServiceDescriptorBase : LifecycleDescriptorBase, IServiceDescriptor
    {
        readonly IService service;

        internal ServiceDescriptorBase(IService service)
        {
            this.service = service;

            // NOTE: perhaps not best location for this, but we can capture idea here at least
            if (service is IOnlineEvents oe)
            {
                oe.Online += async () =>
                {
                    LifecycleStatus = LifecycleEnum.Online;
                    await Startup(null);
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
                    await Startup(null);
                };
                se.Waking += () => LifecycleStatus = LifecycleEnum.Waking;
            }
        }

        public IService Service => service;

        public string Name => service.Name;

        public virtual async Task Shutdown()
        {
            LifecycleStatus = LifecycleEnum.Stopping;
            await service.Shutdown();
            LifecycleStatus = LifecycleEnum.Stopped;
        }

        public virtual async Task Startup(IServiceProvider serviceProvider)
        {
            LifecycleStatus = LifecycleEnum.Starting;
            await service.Startup(serviceProvider);
            LifecycleStatus = LifecycleEnum.Started;
            LifecycleStatus = LifecycleEnum.Running;
        }
    }


    public abstract class WorkerServiceBase : IService
    {
        string name;

        public string Name => name;
        readonly bool oneShot;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">FIX: Not active yet</param>
        protected WorkerServiceBase(string name, CancellationToken ct, bool oneShot = false)
        {
            this.ct = ct;
            this.name = name;
            this.oneShot = oneShot;
            localCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oneShot">FIX: Not active yet</param>
        protected WorkerServiceBase(string name, bool oneShot = false)
        {
            this.name = name;
            this.oneShot = oneShot;
            localCts = new CancellationTokenSource();
        }

        // FIX: making protected to handle IOnlineEvents class services, but
        // I think we can do better
        protected Task worker;
        protected readonly CancellationTokenSource localCts;
        readonly CancellationToken ct;

        // TODO: Decide if we want to keep passing IServiceProvider in, thinking probably
        // yes but let's see how it goes
        protected abstract Task Worker(CancellationToken cts);

        protected async Task RunWorker()
        {
            do
            {
                worker = Worker(localCts.Token);
                await worker;
            }
            while (!oneShot && !localCts.IsCancellationRequested);
        }

        public bool IsWorkerRunning => worker != null;

        public async Task Shutdown()
        {
            localCts.Cancel();
            await worker;
        }

        // FIX: would use "completedTask" but it doesn't seem to be available for netstandard1.1?
        public virtual async Task Startup(IServiceProvider serviceProvider)
        {
            // we specifically *do not* await here, we are starting up a worker thread
            RunWorker();
        }
    }


    /// <summary>
    /// We have many incarnations of this, here's another
    /// A hierarchical manager which can manage many services inclusive of other servicemanagers
    /// </summary>
    public class ServiceManager :
        NamedChildCollection<IServiceDescriptor>,
        IService,
        IServiceDescriptor
    {
        public ServiceManager(string name) : base(name)
        {
            ChildAdded += (o, c) => c.LifecycleStatusUpdated += Child_LifecycleStatusUpdated;
            ChildRemoved += (o, c) => c.LifecycleStatusUpdated -= Child_LifecycleStatusUpdated;
            lifecycle.Changing += (old, @new) => LifecycleStatusUpdating?.Invoke(this, @new);
            lifecycle.Changed += v => LifecycleStatusUpdated?.Invoke(this);
        }

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

        public async Task Shutdown()
        {
            lifecycle.Value = LifecycleEnum.Stopping;
            // TODO: Add provision for sequential startup/shutdown also
            var childrenShutdownTasks = Children.
                Select(x => x.Shutdown());
            try
            {
                await Task.WhenAll(childrenShutdownTasks);
                lifecycle.Value = LifecycleEnum.Stopped;
            }
            catch (Exception e)
            {
                // If an exception is thrown while shutting down children, an improper shutdown
                // has occurred and this is an error state
                // FIX: Don't want to abort shutdown of other children though, so rework code to
                // accomodate that
                lifecycle.Value = LifecycleEnum.Error;
            }
        }

        public async Task Startup(IServiceProvider serviceProvider)
        {
            lifecycle.Value = LifecycleEnum.Starting;
            var childrenStartingTasks = Children.
                Select(x => x.Startup(serviceProvider));
            await Task.WhenAll(childrenStartingTasks);
            lifecycle.Value = LifecycleEnum.Started;
            lifecycle.Value = AscertainCompositeState();
        }


        LifecycleEnum AscertainCompositeState()
        {
            LifecycleEnum state = LifecycleEnum.Running;

            foreach (var child in Children)
            {
                if (child.IsNominal())
                {

                }
                else if (child.IsTransitioning())
                {
                    // child transitioning into or away from running state means
                    // it's basically offline-ish, so report degraded state
                    return LifecycleEnum.PartialRunning;
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
    }


    public static class ServiceManagerExtensions
    {
        public static IServiceDescriptor AddService(this ServiceManager serviceManager, IService service)
        {
            var sd = new ServiceDescriptorBase(service);

            serviceManager.AddChild(sd);

            return sd;
        }
    }
}
