using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Experimental
{
    using Collection;

    public class SyncDataProviderNode : NamedChildCollection<SyncDataProviderNode>
    {
        readonly Func<object> getter;

        public object Value => getter?.Invoke();

        public SyncDataProviderNode(string name, Func<object> getter) : base(name)
        {
            this.getter = getter;
        }
    }

    public class SyncDataProvider : TaxonomyBase<SyncDataProviderNode>
    {
        public override SyncDataProviderNode RootNode { get; }

        public SyncDataProvider(SyncDataProviderNode root) => RootNode = root;

        protected override SyncDataProviderNode CreateNode(SyncDataProviderNode parent, string name)
        {
            // Only used when node isn't found -- so for this case, it really will be an exception
            // DEBT: That could use some cleanup somehow
            throw new NotImplementedException();
        }
    }
}
