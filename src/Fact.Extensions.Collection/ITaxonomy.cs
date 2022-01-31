using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection
{
    public interface ITaxonomyBase<TNode>
        where TNode: IChildProvider<TNode>
    {
        /// <summary>
        /// Root node under which all other nodes will fall.
        /// NOTE: Nodes are named or keyed, but usually the RootNode name is not seen
        /// </summary>
        TNode RootNode { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// TODO: Make a flag which includes or excludes root node as part of the INamedAccessor 
    /// lookup.  Flag will allow the following, example being with RootNode.Name = 'rootn':
    /// taxonomy["./child1"]        // root node -> child1 node
    /// taxonomy["rootn/child1"]    // root node -> child1 node
    /// taxonomy["/child1"]         // root node -> child1 node
    /// - right now, we only support:
    /// taxonomy["child1"]          // root node -> child1 node
    /// we don't track any kind of PWD/CWD nor do we wish to, this should be an external party's
    /// responsibility
    /// </remarks>
    /// <typeparam name="TNode"></typeparam>
    public interface ITaxonomy<TNode> : 
        ITaxonomyBase<TNode>,
        INamedAccessor<TNode>
        where TNode: INamed, IChildProvider<TNode>
    {
    }


    /// <summary>
    /// This form of taxonomy is more broken out with an explicit 'path'
    /// provided by a sequence of <typeparamref name="TKey"/> keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TNode"></typeparam>
    /// <remarks>
    /// DEBT: Broken out from regular named ITaxonomy for legacy reasons, would be nice to combine
    /// </remarks>
    public interface IKeyedTaxonomy<TKey, TNode> :
        ITaxonomyBase<TNode>,
        IAccessor<IEnumerable<TKey>, TNode>
        where TNode: 
            //IKeyed<TKey>, 
            IChildProvider<TNode>
    {

    }

    /// <summary>
    /// Represents a child node in a taxonomy.
    /// Note that taxonomies do not demand knowing the parent, only acquiring a list of children
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChild<T>
    {
        T Parent { get; }
    }

    /// <summary>
    /// Represents a class which can provide a simple enumeration of children
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    public interface IChildProvider<TChild>
    {
        IEnumerable<TChild> Children { get; }
    }


    /// <summary>
    /// Can provide both an enumeration of children as well as acquisition of children by
    /// a key
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    public interface IChildProvider<TKey, TChild> : IChildProvider<TChild>
    {
        /// <summary>
        /// Acquire a child by its key.  If none exists, default(TChild) is returned
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TChild GetChild(TKey key);
    }


    public interface INamedChildProvider<TChild> : IChildProvider<string, TChild>
        where TChild : INamed
    { }


    public interface IChildCollection<TChild> : IChildProvider<TChild>
    {
        /// <summary>
        /// Param #1 is sender
        /// Param #2 is added node
        /// </summary>
        event Action<object, TChild> ChildAdded;

        void AddChild(TChild child);
    }

    public interface IChildCollection<TKey, TChild> : 
        IChildCollection<TChild>,
        IChildProvider<TKey, TChild>
    {
    }


    public interface INamedChildCollection<TChild> :
        IChildCollection<TChild>,
        INamedChildProvider<TChild>
        where TChild : INamed
    {

    }


    namespace Experimental
    {
        public interface IKeyValueChildCollection<TKey, TChild> :
            IChildCollection<TKey, KeyValuePair<TKey, TChild>>
        {
            void AddChild(TKey key, TChild value);
        }
    }
}
