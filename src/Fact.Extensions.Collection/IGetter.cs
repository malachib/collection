using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Apprentice.Collection
{
    public interface IGetter<TKey, TValue>
    {
        TValue Get(TKey key, Type type);
    }

    public interface IGetterAsync<TKey, TValue>
    {
        Task<TValue> GetAsync(TKey key, Type type);
    }
}
