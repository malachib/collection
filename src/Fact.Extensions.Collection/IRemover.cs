using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public interface IRemover<TKey>
    {
        void Remove(TKey key);
    }


    public interface IRemoverAsync<TKey>
    {
        Task RemoveAsync(TKey key);
    }


    public interface IRemover : IRemover<string> { }
    public interface IRemoverAsync : IRemoverAsync<string> { }
}
