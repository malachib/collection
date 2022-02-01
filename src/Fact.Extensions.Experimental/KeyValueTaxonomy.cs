using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Experimental
{
    using Collection;

    public abstract class KeyValueTaxonomy<TKey, TNode> :
        TaxonomyBaseBase<TKey, KeyValuePair<TKey, TNode>>
        where TNode :
            IChildProvider<TKey, KeyValuePair<TKey, TNode>>

    {
        protected KeyValuePair<TKey, TNode> __CreateNode(TNode parent, TKey name)
        {
            var _parent = new KeyValuePair<TKey, TNode>(default(TKey), parent);

            return _CreateNode(_parent, name);
        }

        /// <summary>
        /// Get a key at the given 'path', where in this case 'path' is a sequence of keys
        /// loosely analogous to a traditional string path i.e. 'key1/key2/key3/' pointing to
        /// an end node
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        TNode Get(IEnumerable<TKey> keys) =>
            RootNode.FindChildByPath2(keys, __CreateNode);

        public TNode this[IEnumerable<TKey> keys] => Get(keys);

        public TNode this[params TKey[] keys] => Get(keys);
    }


    public static class KeyValueTaxonomyExtensions
    {
        public static void AddChild<TKey, TNode>(this IChildCollection<TKey, KeyValuePair<TKey, TNode>> parent, TKey key, TNode child)
            where TNode : IChildProvider<TKey, KeyValuePair<TKey, TNode>>
        {
            parent.AddChild(new KeyValuePair<TKey, TNode>(key, child));
        }


        /*
        public static void AddChild<TKey, TNode>(
            this KeyValuePair<TKey, IChildCollection<TKey, KeyValuePair<TKey, TNode>>> parent, TKey key, TNode child)
            where TNode : IChildProvider<TKey, KeyValuePair<TKey, TNode>>
        {
            parent.Value.AddChild(key, child);
        } */

        public static void AddChild<TKey, TNode>(this KeyValuePair<TKey, TNode> parent, TKey key, TNode child)
            where TNode : IChildCollection<TKey, KeyValuePair<TKey, TNode>>
        {
            parent.Value.AddChild(key, child);
        }
    }
}
