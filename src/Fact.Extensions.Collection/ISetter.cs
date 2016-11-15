using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public interface ISetter<TKey, TValue>
    {
        TValue this[TKey key, Type type] { set; }
    }


    public interface ISetterAsync<TKey, TValue>
    {
        Task SetAsync(TKey key, TValue value, Type type);
    }


    public interface ISetter : ISetter<string, object> { }
    public interface ISetterAsync : ISetterAsync<string, object> { }
}
