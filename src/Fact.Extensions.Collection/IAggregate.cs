using Fact.Extensions.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// A self-contained black boxed collection which exposes itself as if
    /// it were one singlar type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAggregate<T>
    {
        void Add(T value);
        void Remove(T value);

        T Aggregated { get; }
    }


    public abstract class AggregateBase<T> : IAggregate<T>
    {
        LazyLoader<LinkedList<T>> aggregates;

        public void Add(T value) => aggregates.Value.AddLast(value);

        public void Remove(T value) => aggregates.Value.Remove(value);

        protected IEnumerable<T> Aggregates =>
            aggregates.IsAllocated ? aggregates.Value : Enumerable.Empty<T>();

        public abstract T Aggregated { get; }
    }
}
