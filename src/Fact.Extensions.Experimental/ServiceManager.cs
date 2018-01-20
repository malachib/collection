using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Experimental
{
    public interface IServiceDescriptor2 : IServiceDescriptor, INamed { }
    /// <summary>
    /// We have many incarnations of this, here's another
    /// A hierarchical manager which can manage many services inclusive of other servicemanagers
    /// </summary>
    public class ServiceManager :
        TaxonomyBase.NodeBase<IServiceDescriptor2>,
        IServiceDescriptor2
    {
        internal class ServiceDescriptor : IServiceDescriptor2
        {
            public IService Service => throw new NotImplementedException();

            public LifecycleEnum LifecycleStatus => throw new NotImplementedException();

            public string Name => throw new NotImplementedException();

            public event Action<object> LifecycleStatusUpdated;
        }

        public ServiceManager() : base("test") { }

        public IService Service => throw new NotImplementedException();

        public LifecycleEnum LifecycleStatus { get; set; }

        public event Action<object> LifecycleStatusUpdated;

        public Task Shutdown()
        {
            throw new NotImplementedException();
        }

        public Task Startup(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
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

        // replace with proper State helper
        internal void SetState(LifecycleEnum state)
        {
            LifecycleStatus = state;
            // TODO: Do this with our State helper to avoid unnecessary invocations
            LifecycleStatusUpdated?.Invoke(this);
        }


        private void Child_LifecycleStatusUpdated(object sender)
        {
            var sd = (IServiceDescriptor2)sender;

            // TODO: can optimize since we know which child state is changing
            LifecycleStatus = AscertainCompositeState();
            // TODO: Do this with our State helper to avoid unnecessary invocations
            LifecycleStatusUpdated?.Invoke(this);
        }
    }
}
