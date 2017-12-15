using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection.Taxonomy
{
    public interface ITaxonomy<TNode> : INamedAccessor<TNode>
        where TNode: INamed, IChildProvider<TNode>
    {
        TNode RootNode { get; }
    }

    public interface IChild<T>
    {
        T Parent { get; }
    }

    public interface IChildProvider<TNode>
    {
        IEnumerable<TNode> Children { get; }
    }
}
