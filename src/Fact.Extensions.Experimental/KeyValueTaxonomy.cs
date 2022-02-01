using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Experimental
{
    using Collection;

    public abstract class KeyValueTaxonomy<TKey, TNode> :
        TaxonomyBaseBase<TKey, TNode>
        where TNode :
            IChildProvider<TKey, KeyValuePair<TKey, TNode>>

    {
        protected KeyValuePair<TKey, TNode> __CreateNode(TNode parent, TKey key)
        {
            var _parent = new KeyValuePair<TKey, TNode>(default(TKey), parent);

            return new KeyValuePair<TKey, TNode>(key, _CreateNode(_parent.Value, key));
        }

        /// <summary>
        /// Get a key at the given 'path', where in this case 'path' is a sequence of keys
        /// loosely analogous to a traditional string path i.e. 'key1/key2/key3/' pointing to
        /// an end node
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        TNode Get(IEnumerable<TKey> keys) =>
            IChildProviderExtensions.FindChildByPath2(RootNode, 
                keys, 
                (KeyValuePair<TKey, TNode> c, TKey key) => c.Key.Equals(key), 
                n => n as IChildProvider<KeyValuePair<TKey, TNode>>, 
                c => c.Value,
                __CreateNode);

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
