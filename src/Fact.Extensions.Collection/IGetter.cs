using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public interface IGetter<TKey, TValue>
    {
        TValue Get(TKey key, Type type);
    }

    public interface IGetterAsync<TKey, TValue>
    {
        Task<TValue> GetAsync(TKey key, Type type);
    }

    public interface ITryGetter<TKey, TValue>
    {
        bool TryGet(TKey key, Type type, out TValue value);
    }

    public interface IGetter : IGetter<string, object> { }
    public interface IGetterAsync : IGetterAsync<string, object> { }
    public interface ITryGetter : ITryGetter<string, object> { }

    public interface IGetterOptions
    {
        /// <summary>
        /// When true, a failed key lookup on IGetter.Get or IGetterAsync.GetAsync throws an exception
        /// When false, a failed key lookup will merely return null
        /// </summary>
        bool UnavailableThrowsException { get; set; }
    }
}
