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


#if !NETSTANDARD1_1
//#if NETSTANDARD1_6
        /// <summary>
        /// TEMPORARY
        /// until we get pluggable/composables up, fiddle with this layer
        /// </summary>
        internal class NamedBagWrapper<TKey> : IBag, IRemover, ITryGetter
        {
            readonly IIndexer<TKey, object> indexer;

            internal NamedBagWrapper(IIndexer<TKey, object> indexer)
            {
                this.indexer = indexer;
            }

            public object this[string key, Type type]
            {
                set
                {
                    var k = (TKey)Convert.ChangeType(key, typeof(TKey));
                    indexer[k] = value;
                }
            }

            public object Get(string key, Type type)
            {
                var k = (TKey) Convert.ChangeType(key, typeof(TKey));
                return indexer[k];
            }

            public void Remove(string key)
            {
                var k = (TKey)Convert.ChangeType(key, typeof(TKey));
                ((IRemover<TKey>)indexer).Remove(k);
            }

            public bool TryGet(string key, Type type, out object value)
            {
                throw new NotImplementedException();
            }
        }

        public static IBag ToNamedBag<TKey>(this IIndexer<TKey, object> indexer)
        {
            return new NamedBagWrapper<TKey>(indexer);
        }
#endif
    }
}
