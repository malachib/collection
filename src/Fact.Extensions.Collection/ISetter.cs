using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public interface ISetter<TKey, TValue>
    {
        void Set(TKey key, TValue value, Type type);
    }


    public interface ISetterAsync<TKey, TValue>
    {
        Task SetAsync(TKey key, TValue value, Type type);
    }


    public interface ISetter : ISetter<string, object> { }
    public interface ISetterAsync : ISetterAsync<string, object> { }
}
