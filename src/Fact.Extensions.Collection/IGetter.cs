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

    public interface IGetter : IGetter<string, object> { }
    public interface IGetterAsync : IGetterAsync<string, object> { }
}
