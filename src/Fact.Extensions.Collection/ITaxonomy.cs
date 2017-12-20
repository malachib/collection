using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection
{
    public interface ITaxonomy<TNode> : INamedAccessor<TNode>
        where TNode: INamed, IChildProvider<TNode>
    {
        TNode RootNode { get; }
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
        /// Acquire a child by its key.  If non exists, default(TChild) is returned
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
}
