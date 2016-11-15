using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public static class IGetter_Extensions
    {
        public static TValue Get<TValue>(this IGetter<string, object> getter, string key)
        {
            return (TValue) getter.Get(key, typeof(TValue));
        }

        public static TValue Get<TValue>(this IGetter<object, object> getter, object key)
        {
            return (TValue)getter.Get(key, typeof(TValue));
        }


        public static async Task<TValue> GetAsync<TValue>(this IGetterAsync<string, object> getter, string key)
        {
            return (TValue)await getter.GetAsync(key, typeof(TValue));
        }
    }
}
