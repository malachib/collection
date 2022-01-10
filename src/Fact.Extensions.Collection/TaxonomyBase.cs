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


    public abstract class TaxonomyBaseBase<TKey, TNode> : TaxonomyBase
    {
        public abstract TNode RootNode { get; }

        protected abstract TNode CreateNode(TNode parent, TKey key);

        public event Action<object, TNode> NodeCreated;

        /// <summary>
        /// Helper since cast didn't automatically happen via FindChildByPath
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        /// <remarks>
        /// DEBT: Clean up naming to disambiguate from core CreateNode
        /// </remarks>
        protected TNode _CreateNode(TNode parent, TKey name)
        {
            var createdNode = CreateNode(parent, name);

            NodeCreated?.Invoke(this, createdNode);

            return createdNode;
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TNode"></typeparam>
    /// <remarks>
    /// Different from regular TaxonomyBase[TNode] which presumes string key and named child provider
    /// </remarks>
    public abstract class TaxonomyBase<TKey, TNode> : TaxonomyBaseBase<TKey, TNode>, 
        IKeyedTaxonomy<TKey, TNode>
        where TNode: IKeyed<TKey>, IChildProvider<TKey, TNode>
    {
        TNode Get(IEnumerable<TKey> keys) =>
            RootNode.FindChildByPath(keys, _CreateNode, (node, _key) => node.Key.Equals(_key));

        public TNode this[IEnumerable<TKey> keys] => Get(keys);

        public TNode this[params TKey[] keys] => Get(keys);
    }

    /// <summary>
    /// Base wrapper/accessor for nodes, which are expected to be INamedChildProvider
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public abstract class TaxonomyBase<TNode> : TaxonomyBaseBase<string, TNode>, ITaxonomy<TNode>
        where TNode :
            INamedChildProvider<TNode>,
            INamed
    {


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
