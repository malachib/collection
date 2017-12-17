using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

using Fact.Extensions.Collection;
using Fact.Extensions.Collection.Taxonomy;
using System.Collections.Generic;

namespace Fact.Extensions.Experimental
{
    public interface ILifecycle
    {
        Task Startup(IServiceProvider serviceProvider);
        Task Shutdown();
    }

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


    public interface ILifecycleDescriptor
    {
        bool IsStarting { get; }
        bool IsStarted { get; }

        bool IsShuttingDown { get; }
        bool IsShutdown { get; }
    }

    public interface IResourceProvider<TResource> : 
        ILifecycle, 
        ILockable,
        INamed
    {
        TResource Resource { get; }
    }


    public interface IResource
    {

    }


    public interface ISubsystem<TResource> : 
        IResourceProvider<TResource>,
        IChildProvider<IResourceProvider<TResource>>
    {
        event Action<IResourceProvider<TResource>> Starting;
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

        public TResource Resource => throw new NotImplementedException();

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

        public Task Lock(object key = null)
        {
            return null;
        }

        public void Unlock(object key = null)
        {
        }
    }


    public class LifecycleManager<TResource>
    {
        readonly IServiceProvider serviceProvider;

        public LifecycleManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        // describes parent then child
        public event Action<IResourceProvider<TResource>, IResourceProvider<TResource>> Starting;
        public event Action<IResourceProvider<TResource>, IResourceProvider<TResource>> Started;

        internal class Descriptor : ILifecycleDescriptor
        {
            public bool IsStarting { get; set; }
            public bool IsStarted { get; set; }

            public bool IsShuttingDown { get; set; }

            public bool IsShutdown { get; set; }
        }

        public async void Start(ISubsystem<TResource> subsystem)
        {
            var d = new Descriptor();
            var startingResponder = new Action<IResourceProvider<TResource>>(r =>
            {
                Starting?.Invoke(subsystem, r);
            });
            var startedResponder = new Action<IResourceProvider<TResource>>(r =>
            {
                Started?.Invoke(subsystem, r);
            });

            d.IsStarting = true;
            Starting?.Invoke(subsystem, null);
            subsystem.Starting += startingResponder;
            subsystem.Started += startedResponder;
            await subsystem.Startup(serviceProvider);
            subsystem.Starting -= startingResponder;
            subsystem.Started -= startedResponder;
            Started?.Invoke(subsystem, null);
            d.IsStarting = false;
            d.IsStarted = true;
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
