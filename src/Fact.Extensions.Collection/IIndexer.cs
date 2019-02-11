using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Indexer reusable base interface
    /// Hides base IAccessor - so if implementing explicitly, will need two "gets"
    /// since .NET can't mesh the get from one interface and the set from the other
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IIndexer<TKey, TValue> : IAccessor<TKey, TValue>
    {
        new TValue this[TKey key] { get; set; }
    }

    /// <summary>
    /// Reusable template for stock standard indexer operations
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface INamedIndexer<TValue> : INamedAccessor<TValue>, IIndexer<string, TValue>
    {
    }

    namespace Experimental
    {
        public interface IIndexerAsync<TKey, TValue> : IAccessorAsync<TKey, TValue>
        {
            Task SetAsync(TKey key, TValue value);
        }
    }
}
