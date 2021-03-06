﻿using System;
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


        /// <summary>
        /// TEMPORARY
        /// until we get pluggable/composables up, fiddle with this layer
        /// </summary>
        internal class NamedBagWrapper<TKey> : IBag, IRemover, ITryGetter
        {
            protected readonly IIndexer<TKey, object> indexer;

            internal NamedBagWrapper(IIndexer<TKey, object> indexer)
            {
                this.indexer = indexer;
            }

            TKey GetKey(string key)
            {
                var k = (TKey)Convert.ChangeType(key, typeof(TKey));
                return k;
            }

            public object this[string key, Type type]
            {
                set
                {
                    indexer[GetKey(key)] = value;
                }
            }

            public object Get(string key, Type type)
            {
                return indexer[GetKey(key)];
            }

            public void Remove(string key)
            {
                ((IRemover<TKey>)indexer).Remove(GetKey(key));
            }

            public bool TryGet(string key, Type type, out object value)
            {
                return ((ITryGetter<TKey>)indexer).TryGet(GetKey(key), type, out value);
            }
        }


        internal class NamedBagWrapperWithKeys<TKey> : NamedBagWrapper<TKey>, IKeys<TKey>
        {
            internal NamedBagWrapperWithKeys(IIndexer<TKey, object> indexer)
            : base(indexer)
            {
            }

            IEnumerable<TKey> IKeys<TKey>.Keys => ((IKeys<TKey>) indexer).Keys;
        }

        public static IBag ToNamedBag<TKey>(this IIndexer<TKey, object> indexer)
        {
            return new NamedBagWrapper<TKey>(indexer);
        }


        public static IBag ToNamedBag(this NamedIndexerWrapperWithKeys<object> indexer)
        {
            return new NamedBagWrapperWithKeys<string>(indexer);
        }

        public static NamedIndexerWrapperWithKeys<TValue> ToIndexer<TValue>(this IDictionary<string, TValue> dictionary)
        {
            return new NamedIndexerWrapperWithKeys<TValue>(
                key => dictionary[key],
                (key, value) => dictionary[key] = value,
                () => dictionary.Keys);
        }
    }
}
