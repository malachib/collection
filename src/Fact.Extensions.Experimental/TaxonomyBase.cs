using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Experimental
{
    /// <summary>
    /// Consider changing to a better name
    /// </summary>
    public class TaxonomyBase
    {
        public class NodeBase<TNode> :
            INamed,
            INamedChildCollection<TNode>
            where TNode : INamed
        {
            SparseDictionary<string, TNode> children;
            readonly string name;

            public string Name => name;

            public IEnumerable<TNode> Children => children.Values;

            public event Action<object, TNode> ChildAdded;

            public NodeBase(string name)
            {
                this.name = name;
            }

            /// <summary>
            /// TODO: Very likely would prefer a null back if no child, not an exception
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public TNode GetChild(string name)
            {
                children.TryGetValue(name, out TNode value);
                return value;
            }

            public void AddChild(TNode node)
            {
                children.Add(node.Name, node);
                ChildAdded?.Invoke(this, node);
            }
        }
    }

    public abstract class TaxonomyBase<TNode> : TaxonomyBase, ITaxonomy<TNode>
        where TNode :
            INamedChildProvider<TNode>,
            INamed
    {
        public abstract TNode RootNode { get; }

        protected virtual TNode CreateNode(TNode parent, string name) { return default(TNode); }

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
