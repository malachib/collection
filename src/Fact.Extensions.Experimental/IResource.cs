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

    }


    public abstract class Subsystem : ISubsystem
    {
        readonly string name;

        public string Name => name;

        public Subsystem(string name) { this.name = name; }

        public abstract IEnumerable<IResource> Children { get; }

        public async Task Startup(IServiceProvider serviceProvider)
        {
            var startupTasks = 
                Children.Select(x => Task.Run(() => x.Startup(serviceProvider))).ToArray();

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

            d.IsStarting = true;
            await subsystem.Startup(serviceProvider);
            d.IsStarting = false;
            d.IsStarted = true;
        }
    }
}
