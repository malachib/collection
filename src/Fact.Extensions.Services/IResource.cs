using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

using Fact.Extensions.Collection;
using System.Collections.Generic;

namespace Fact.Extensions.Services
{
    /// <summary>
    /// Describes any kind of lockable behavior such as virtual memory locking,
    /// DB table locking, etc
    /// </summary>
    public interface ILockable
    {
        /// <summary>
        /// Lock resource down for access only by calling thread
        /// </summary>
        Task Lock(object key = null);

        /// <summary>
        /// Release resource for either no access or read only access
        /// by any thread
        /// </summary>
        void Unlock(object key = null);
    }


    public interface IResourceProvider<TResource> : 
        IService,
        ILockable
    {
        TResource Resource { get; }
    }


    public interface IResource
    {

    }


    /// <summary>
    /// FIX: Rename ISubsystem, right now the name represents a service with
    /// children - but a subsytem , if you think about it, represents more the child
    /// node, not the parent
    /// </summary>
    public interface ISubsystemExperimental : IService, 
        IChild<IService>,
        IChildProvider<IService>
    {

    }



    public interface ISubsystem<TResource> : 
        IService,
        IChildProvider<IResourceProvider<TResource>>
    {
        /// <summary>
        /// Fired per child resource provider which fires up
        /// </summary>
        event Action<IResourceProvider<TResource>> Starting;
        /// <summary>
        /// Fired per child resource provider which halts up
        /// </summary>
        event Action<IResourceProvider<TResource>> Started;
    }


    public abstract class Subsystem<TResource> : ISubsystem<TResource>
    {
        readonly string name;

        public string Name => name;

        public event Action<IResourceProvider<TResource>> Starting;
        public event Action<IResourceProvider<TResource>> Started;

        public Subsystem(string name) { this.name = name; }

        public abstract IEnumerable<IResourceProvider<TResource>> Children { get; }

        public async Task Startup(IServiceProvider serviceProvider)
        {
            var startupTasks = 
                Children.Select(x => 
                Task.Run(() =>
                {
                    Starting?.Invoke(x);
                    x.Startup(serviceProvider);
                    Started?.Invoke(x);
                })).ToArray();

            foreach(var child in startupTasks)
                await child;
        }

        public async Task Shutdown()
        {
            var shutdownTasks =
                Children.Select(x => Task.Run(() => x.Shutdown())).ToArray();

            foreach (var child in shutdownTasks)
                await child;
        }
    }


    public class LifecycleManager
    {
        readonly IServiceProvider serviceProvider;

        public LifecycleManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        // describes parent then child.  Operations are always
        // on the child
        public event Action<IService, IService> Starting;
        public event Action<IService, IService> Started;

        internal class Descriptor : IServiceDescriptorBase
        {
            LifecycleEnum status;
            readonly IService service;

            public IService Service => service;

            internal Descriptor(IService service)
            {
                this.service = service;
            }

            public LifecycleEnum LifecycleStatus
            {
                get => status;
                internal set
                {
                    status = value;
                    LifecycleStatusUpdated?.Invoke(this);
                }
            }

            public event Action<object> LifecycleStatusUpdated;
        }

        public async void Start<TResource>(ISubsystem<TResource> subsystem)
        {
            var d = new Descriptor(subsystem);
            var startingResponder = new Action<IResourceProvider<TResource>>(r =>
            {
                Starting?.Invoke(subsystem, r);
            });
            var startedResponder = new Action<IResourceProvider<TResource>>(r =>
            {
                Started?.Invoke(subsystem, r);
            });

            d.LifecycleStatus = LifecycleEnum.Starting;
            Starting?.Invoke(null, subsystem);
            subsystem.Starting += startingResponder;
            subsystem.Started += startedResponder;
            await subsystem.Startup(serviceProvider);
            subsystem.Starting -= startingResponder;
            subsystem.Started -= startedResponder;
            Started?.Invoke(null, subsystem);
            d.LifecycleStatus = LifecycleEnum.Started;
            d.LifecycleStatus = LifecycleEnum.Running;
        }
    }

    public struct LockScope : IDisposable
    {
        ILockable lockable;

        LockScope(ILockable lockable)
        {
            this.lockable = lockable;

            lockable.Lock();
        }

        public void Dispose()
        {
            lockable.Unlock();
        }
    }


    public struct ResourceHelper<T> : IDisposable
    {
        IResourceProvider<T> provider;

        public ResourceHelper(IResourceProvider<T> provider, object key = null)
        {
            this.provider = provider;

            provider.Lock();
        }

        public T Resource => provider.Resource;

        public void Dispose()
        {
            provider.Unlock();
        }

        public static implicit operator T(ResourceHelper<T> resourceHelper)
        {
            return resourceHelper.Resource;
        }
    }


    public static class ILifecycleDescriptorExtensions
    {
        public static void test(this LifecycleEnum lifecycleEnum)
        {

        }

        /// <summary>
        /// Is in an online kind of a state
        /// </summary>
        /// <param name="ld"></param>
        /// <returns></returns>
        public static bool IsNominal(this ILifecycleDescriptor ld)
        {
            var status = ld.LifecycleStatus;

            return status == LifecycleEnum.Running;
        }


        /// <summary>
        /// Is in a short term state between long-running states
        /// </summary>
        /// <param name="ld"></param>
        /// <returns></returns>
        public static bool IsTransitioning(this ILifecycleDescriptor ld)
        {
            var status = ld.LifecycleStatus;

            switch(status)
            {
                case LifecycleEnum.Online:
                case LifecycleEnum.Pausing:
                case LifecycleEnum.Resuming:
                case LifecycleEnum.Starting:
                case LifecycleEnum.Waking:
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Is definitely, positively in an offline kind of state.  Excludes transitioning from one state
        /// Not necessarily an error state, but could be
        /// </summary>
        /// <param name="ld"></param>
        /// <returns></returns>
        public static bool IsNotRunning(this ILifecycleDescriptor ld)
        {
            var status = ld.LifecycleStatus;

            switch(status)
            {
                case LifecycleEnum.Offline:
                case LifecycleEnum.Paused:
                case LifecycleEnum.Slept:
                case LifecycleEnum.Unstarted:
                case LifecycleEnum.Error:
                    return true;

                default:
                    return false;
            }
        }
    }

    public static class IResourceProviderExtensions
    {
        public static ResourceHelper<T> GetResourceHelper<T>(this IResourceProvider<T> resourceProvider)
        {
            // FIX: may do a lock/unlock/lock/unlock so beware
            return new ResourceHelper<T>(resourceProvider);
        }

        public static async Task<TResource> GetResource<TResource>(this IResourceProvider<TResource> resourceProvider)
        {
            await resourceProvider.Lock();
            TResource resource = resourceProvider.Resource;
            resourceProvider.Unlock();
            return resource;
        }
    }
}
