using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Accessor like IIndexer, but for read-only operations
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IAccessor<TKey, TValue>
    {
        TValue this[TKey key] { get; }
    }


    /// <summary>
    /// Reusable template for standard read-only indexer operations whose lookup key is a string.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// Be mindful to mainly use this on implementing classes, not
    /// so much on consumers due to the way IIndexer and IAccessor mesh 
    /// together
    /// </remarks>
    public interface INamedAccessor<TValue> : IAccessor<string, TValue> { }
}
