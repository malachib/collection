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

    public interface IResource : 
        ILifecycle, 
        ILockable,
        INamed
    {
    }


    public interface ISubsystem : 
        IResource,
        IChildProvider<IResource>
    {
        event Action<IResource> Starting;
        event Action<IResource> Started;
    }


    public abstract class Subsystem : ISubsystem
    {
        readonly string name;

        public string Name => name;

        public event Action<IResource> Starting;
        public event Action<IResource> Started;

        public Subsystem(string name) { this.name = name; }

        public abstract IEnumerable<IResource> Children { get; }

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


    public class LifecycleManager
    {
        readonly IServiceProvider serviceProvider;

        // describes parent then child
        public event Action<IResource, IResource> Starting;
        public event Action<IResource, IResource> Started;

        internal class Descriptor : ILifecycleDescriptor
        {
            public bool IsStarting { get; set; }
            public bool IsStarted { get; set; }

            public bool IsShuttingDown { get; set; }

            public bool IsShutdown { get; set; }
        }

        public async void Start(ISubsystem subsystem)
        {
            var d = new Descriptor();
            Action<IResource> startingResponder = new Action<IResource>(r =>
            {
                Starting?.Invoke(subsystem, r);
            });
            Action<IResource> startedResponder = new Action<IResource>(r =>
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
}
