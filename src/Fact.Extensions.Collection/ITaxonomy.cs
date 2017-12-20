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

    public interface IChildProvider<TChild>
    {
        IEnumerable<TChild> Children { get; }
    }

    public interface IChildProvider<TKey, TChild> : IChildProvider<TChild>
    {
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
