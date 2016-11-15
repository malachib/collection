using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public static class IIndexer_Extensions
    {
		internal class BagWrapper<TKey, TValue> : IBag<TKey, TValue>
        {
            readonly IIndexer<TKey, TValue> indexer;

			internal BagWrapper(IIndexer<TKey, TValue> indexer)
            {
                this.indexer = indexer;
            }

            public TValue Get(TKey key, Type type)
            {
                return indexer[key];
            }

            public TValue this[TKey key, Type type]
            {
                set { indexer[key] = value;  }
            }
        }

		public static IBag<TKey, TValue> ToBag<TKey, TValue>(this IIndexer<TKey, TValue> indexer)
        {
            return new BagWrapper<TKey, TValue>(indexer);
        }

        public static IBag<string, TValue> ToNamedBag<TKey, TValue>(this IIndexer<TKey, TValue> indexer)
        {
            return null;
        }
    }
}
