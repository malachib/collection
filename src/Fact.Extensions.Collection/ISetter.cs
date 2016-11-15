using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public interface ISetter<TKey, TValue>
    {
        void Set(TKey key, TValue value, Type type = null);
    }


    public interface ISetterAsync<TKey, TValue>
    {
        Task SetAsync(TKey key, TValue value, Type type = null);
    }


    public interface ISetter : ISetter<string, object> { }
    public interface ISetterAsync : ISetterAsync<string, object> { }
}
