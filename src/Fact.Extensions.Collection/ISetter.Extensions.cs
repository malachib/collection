using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public static class ISetter_Extensions
    {
        public static void Set<TValue>(this ISetter<string, object> setter, string key, TValue value)
        {
            setter[key, typeof(TValue)] = value;
        }


        public static void Set<TValue>(this ISetter<object, object> setter, string key, TValue value)
        {
            setter[key, typeof(TValue)] = value;
        }
    }
}
