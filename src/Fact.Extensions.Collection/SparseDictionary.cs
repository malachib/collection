using Fact.Extensions.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Flavor of IDictionary which is very lazy-loaded
    /// Has undergone light testing
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public struct SparseDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>
    {
        LazyLoader<Dictionary<TKey, TValue>> value;

        /*
        public SparseDictionary(IDictionary<TKey, TValue> copyFrom)
        {
            value.Value = new Dictionary<TKey, TValue>(copyFrom);
        } */

        public TValue this[TKey key]
        {
            get
            {
                return value.IfAllocated(
                    v => v[key], 
                    () => throw new KeyNotFoundException());
            }
            set => this.value.Value[key] = value;
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (value.IsAllocated) return value.RawValue.Keys;

                return new TKey[0];
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (value.IsAllocated) return value.RawValue.Values;

                // Can't use .Empty because that's just an IEnumeration
                return new TValue[0];
            }
        }

        //public int Count => value.IsAllocated ? value.value.Count : 0;
        public int Count => value.IfAllocated(v => v.Count);

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            this.value.Value.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)value.Value).Add(item);
        }

        public void Clear() => value.IfAllocated(v => v.Clear());

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (value.IsAllocated) return value.RawValue.Contains(item);

            throw new KeyNotFoundException();

        }

        public bool ContainsKey(TKey key) => value.IfAllocated(x => x.ContainsKey(key));

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (value.IsAllocated) return value.RawValue.GetEnumerator();

            return Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();
        }

        public bool Remove(TKey key) => value.IfAllocated(v => v.Remove(key));

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.value.IsAllocated)
            {
                return this.value.RawValue.TryGetValue(key, out value);
            }

            value = default(TValue);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
