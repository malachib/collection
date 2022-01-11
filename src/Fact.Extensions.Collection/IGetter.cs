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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// DEBT: Document why we must specify Type here and not infer it from TValue.  I believe it's
        /// because frequently TValue works out to be object -- and even when it cascades from a strong
        /// type down to just object the original TValue isn't always as fully down the tree as Type
        /// can know about
        /// </remarks>
        bool TryGet(TKey key, Type type, out TValue value);
    }

    public interface ITryGetterAsync<TKey, TValue>
    {
        Task<Tuple<bool, TValue>> TryGetAsync(TKey key, Type type);
    }

    public interface IGetter : IGetter<string, object> { }
    public interface IGetterAsync : IGetterAsync<string, object> { }

    public interface ITryGetter<TKey> : ITryGetter<TKey, object> { }
    public interface ITryGetter : ITryGetter<string, object> { }

    public interface ITryGetterAsync : ITryGetterAsync<string, object> { }

    public interface IGetterOptions
    {
        /// <summary>
        /// When true, a failed key lookup on IGetter.Get or IGetterAsync.GetAsync throws an exception
        /// When false, a failed key lookup will merely return null
        /// </summary>
        bool UnavailableThrowsException { get; set; }
    }


    /// <summary>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// Some providers combine these operations for convenience and maybe to avoid race conditions
    /// </remarks>
    public interface IGetOrFetch<TKey, TValue>
    {
        TValue GetOrFetch(TKey key, Type type, Func<TValue> factory);
    }


    public interface IGetOrFetch : IGetOrFetch<string, object> { }


    /// <summary>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// Some providers combine these operations for convenience and maybe to avoid race conditions
    /// </remarks>
    public interface IGetOrFetchAsync<TKey, TValue>
    {
        Task<TValue> GetOrFetchAsync(TKey key, Type type, Func<Task<TValue>> factory);
    }

    public interface IGetOrFetchAsync : IGetOrFetchAsync<string, object> { }
}
