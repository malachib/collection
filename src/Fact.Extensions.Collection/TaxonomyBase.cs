using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Consider changing to a better name
    /// </summary>
    public class TaxonomyBase
    {
        [Obsolete("Use NamedChildCollection now")]
        public class NodeBase<TNode> : NamedChildCollection<TNode>
            where TNode : INamed
        {
            public NodeBase(string name) : base(name) { }
        }
    }

    /// <summary>
    /// Base wrapper/accessor for nodes, which are expected to be INamedChildProvider
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public abstract class TaxonomyBase<TNode> : TaxonomyBase, ITaxonomy<TNode>
        where TNode :
            INamedChildProvider<TNode>,
            INamed
    {
        public abstract TNode RootNode { get; }

        protected abstract TNode CreateNode(TNode parent, string name);

        public event Action<object, TNode> NodeCreated;

        /// <summary>
        /// Helper since cast didn't automatically happen via FindChildByPath
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private TNode _CreateNode(TNode parent, string name)
        {
            var createdNode = CreateNode(parent, name);

            NodeCreated?.Invoke(this, createdNode);

            return createdNode;
        }

        public TNode this[string path]
        {
            get
            {
                string[] splitPaths = path.Split('/');

                return RootNode.FindChildByPath(splitPaths, _CreateNode);
            }
        }
    }
}
