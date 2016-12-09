using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Fact.Extensions.Collection;

namespace Fact.Extensions.Caching
{
    public interface ICacheBase
    {
        IEnumerable<Type> SupportedOptions { get; }

    }

    public interface ICache : ICacheBase, IBag, IRemover, ITryGetter
    {
        void Set(string key, object value, Type type, params ICacheItemOption[] options);
    }


    public interface ICacheAsync : ICacheBase, IBagAsync, IRemoverAsync
    {
        Task SetAsync(string key, object value, Type type, params ICacheItemOption[] options);
    }


    public interface ICacheItemOption { }


    public class CacheItemOption : ICacheItemOption
    {
        /// <summary>
        /// Only allow a set operation if item does not already exist in cache
        /// </summary>
        public bool CreateOnly { get; set; }
        /// <summary>
        /// What priority this item has compared to other items in the cache for when to be flushed
        /// </summary>
        public CacheItemPriority Priority { get; set; }
    }

    public interface ICacheItemExpiration : ICacheItemOption
    {
    }

    public class AbsoluteTimeExpiration : ICacheItemExpiration
    {
        public DateTime Expiry { get; private set; }

        public AbsoluteTimeExpiration(DateTime expiryTime)
        {
            Expiry = expiryTime;
        }
    }


    public class SlidingTimeExpiration : ICacheItemExpiration
    {
        public TimeSpan Duration { get; private set; }

        public SlidingTimeExpiration(TimeSpan duration)
        {
            Duration = duration;
        }

        public static SlidingTimeExpiration From(TimeSpan duration)
        {
            return new SlidingTimeExpiration(duration);
        }
    }

    /// <summary>
    /// How important the item is, determining how soon it will be flushed from the cache
    /// </summary>
    public enum CacheItemPriority
    {
        /// <summary>
        /// Of all flushable items, low priority items are more likely to be flushed
        /// than the other priorities
        /// </summary>
        Low,
        Normal,
        /// <summary>
        /// Of all flushable items, high priority items are least likely to be flushed
        /// </summary>
        High,
        NotRemovable,
        Default
    }
}
