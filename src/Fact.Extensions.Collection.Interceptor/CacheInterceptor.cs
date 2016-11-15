using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Fact.Extensions.Collection;

namespace Fact.Extensions.Collection.Interceptor
{
    public class CacheInterceptor : PropertyInterceptor
    {
        readonly IBag cache;
        readonly IRemover cacheRemover;
        readonly string prefix;

        CacheMethodInterceptor methodInterceptor;

        public CacheInterceptor(IBag cache, string prefix = null)
        {
            this.prefix = prefix;
            this.cache = cache;
            this.cacheRemover = (IRemover)cache;
        }


        string GetPropCacheKey(PropertyInfo prop)
        {
            var key = prefix + "." + prop.DeclaringType.Name + "." + prop.Name;
            return key;
        }


        string GetMethodCacheKey(IInvocation invocation)
        {
            var method = invocation.Method;
            // TODO: Revise argument flattener, overloaded method with different argument count
            // could cause collision
            // FIX: Unexpected behavior, .NET Standard 1.1 filtering is applying to this
            // .NET Standard 1.6 assembly, so I can't reach the alternate ToString(delim) code
            var key = prefix + method.Name + ":" + 
                invocation.Arguments.Select(x => x.ToString()).ToString(",");
            return key;
        }

        protected override object Get(IInvocation invocation, PropertyInfo prop)
        {
            throw new NotImplementedException();
        }

        protected override void Set(IInvocation invocation, PropertyInfo prop, object value)
        {
            throw new NotImplementedException();
        }

        protected override void InterceptNonProperty(IInvocation invocation)
        {
            methodInterceptor.Intercept(invocation);
        }
    }


    public class CacheMethodInterceptor : IInterceptor
    {
        IBag cache;

        public void Intercept(IInvocation invocation)
        {

        }
    }
}
