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

    public interface IChildProvider<TNode>
    {
        IEnumerable<TNode> Children { get; }
    }
}
