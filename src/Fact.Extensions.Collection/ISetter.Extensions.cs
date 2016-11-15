using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public static class ISetter_Extensions
    {
        public static void Set<TValue>(this ISetter<string, object> setter, string key, TValue value)
        {
            setter.Set(key, value, typeof(TValue));
        }
    }
}
