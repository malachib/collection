﻿using Fact.Extensions.Collection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Experimental
{
    /// <summary>
    /// </summary>
    public interface IServiceDescriptor2 : IServiceDescriptor, INamed { }

    /// <summary>
    /// We have many incarnations of this, here's another
    /// A hierarchical manager which can manage many services inclusive of other servicemanagers
    /// </summary>
    public class ServiceManager :
        TaxonomyBase.NodeBase<IServiceDescriptor2>,
        IService,
        IServiceDescriptor2
    {
        internal class ServiceDescriptor : IServiceDescriptor2
        {
            readonly IService service;

            internal ServiceDescriptor(IService service)
            {
                this.service = service;
            }

            public IService Service => service;

            public LifecycleEnum LifecycleStatus => throw new NotImplementedException();

            public string Name => service.Name;

            public event Action<object> LifecycleStatusUpdated;
        }

        public ServiceManager(string name = "unnamed") : base(name)
        {
            lifecycle.Changed += v => LifecycleStatusUpdated?.Invoke(this);
        }

        public IService Service => this;

        State<LifecycleEnum> lifecycle;

        public LifecycleEnum LifecycleStatus
        {
            get => lifecycle.Value;
            // mainly used for unit test access
            internal set => lifecycle.Value = value;
        }

        public event Action<object> LifecycleStatusUpdated;

        public async Task Shutdown()
        {
            lifecycle.Value = LifecycleEnum.Stopping;
            // TODO: Add provision for sequential startup/shutdown also
            var childrenShutdownTasks = Children.Select(x => x.Service.Shutdown());
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
            var childrenStartingTasks = Children.Select(x => x.Service.Startup(serviceProvider));
            await Task.WhenAll(childrenStartingTasks);
            // FIX: child may set this in a degraded state or something, inconsistency in behavior
            // here
            lifecycle.Value = LifecycleEnum.Started;
            lifecycle.Value = LifecycleEnum.Running;
        }


        /// <summary>
        /// FIX: Will be confusing against base AddChild
        /// Probably making AddChild virtual would be better
        /// </summary>
        /// <param name="child"></param>
        public void AddService(IServiceDescriptor2 child)
        {
            // NOTE: Perhaps catch AddChild event instead of making virtual
            child.LifecycleStatusUpdated += Child_LifecycleStatusUpdated;
            AddChild(child);
        }


        public void RemoveService(IServiceDescriptor2 child)
        {
            // TBD, no base remover
        }


        LifecycleEnum AscertainCompositeState()
        {
            LifecycleEnum state = LifecycleEnum.Running;

            foreach (var child in Children)
            {
                if (child.IsNominal())
                {

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
            var sd = (IServiceDescriptor2)sender;

            // TODO: can optimize since we know which child state is changing
            lifecycle.Value = AscertainCompositeState();
        }
    }
}
