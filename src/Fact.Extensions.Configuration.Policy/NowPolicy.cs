using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Configuration
{
    [Alias("fact.policy.now")]
    [DefaultPolicy(typeof(RealtimeNowPolicy))]
    public interface INowPolicy : IPolicy
    {
        DateTime Now { get; }
    }

    public class SpecificNowPolicy : INowPolicy
    {
        readonly DateTime now;

        public SpecificNowPolicy(DateTime now)
        {
            this.now = now;
        }

        public DateTime Now => now;
    }


    public class RealtimeNowPolicy : INowPolicy
    {
        public DateTime Now => DateTime.Now;
    }


    public static class INowPolicy_Extensions
    {
        public static DateTime Today(this INowPolicy nowPolicy)
        {
            return nowPolicy.Now.Date;
        }
    }
}
