using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// For any collection which reveals a set of keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKeys<TKey>
    {
        IEnumerable<TKey> Keys { get; }
    }

    /// <summary>
    /// Async flavor of IKeys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKeysAsync<TKey>
    {
        Task<IEnumerable<TKey>> GetKeysAsync();
    }

    /// <summary>
    /// For any collection which can quer a key (and presumably owned value) existence
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IContainsKey<TKey>
    {
        bool ContainsKey(TKey key);
    }
}
