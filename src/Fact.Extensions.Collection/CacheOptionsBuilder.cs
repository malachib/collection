using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Caching
{
    /// <summary>
    /// Experimental
    /// </summary>
    public class CacheOptionsBuilder
    {
        internal LinkedList<ICacheItemOption> options = new LinkedList<ICacheItemOption>();
    }


    public static class CacheOptionBuilder_Extensions
    {
        public static CacheOptionsBuilder SetSlidingExpiry(this CacheOptionsBuilder builder, TimeSpan duration)
        {
            builder.options.AddLast(new SlidingTimeExpiration(duration));
            return builder;
        }

        public static CacheOptionsBuilder SetAbsoluteExpiry(this CacheOptionsBuilder builder, DateTime expiry)
        {
            builder.options.AddLast(new AbsoluteTimeExpiration(expiry));
            return builder;
        }
    }
}
