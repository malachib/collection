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
        readonly ITryGetter cacheTryGet;
        readonly string prefix;

        public CacheInterceptor(IBag cache, string prefix = null)
        {
            this.prefix = prefix;
            this.cache = cache;
            this.cacheRemover = (IRemover)cache;
            this.cacheTryGet = (ITryGetter)cache;
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
            var method = invocation.Method;
            var attr = method.GetCustomAttribute<OperationCacheAttribute>();
            if (attr != null)
            {
                var type = method.ReturnType;
                var key = GetMethodCacheKey(invocation);

                object returnValue;

                if (cacheTryGet.TryGet(key, type, out returnValue))
                {
                    invocation.ReturnValue = returnValue;
                }
                else
                {
                    invocation.Proceed();
                    // FAIL :(
                    //cache[key, type] = invocation.ReturnValue;
                }
            }
        }
    }


    /// <summary>
    /// Marks this operation as something the cache interceptor should pick up
    /// </summary>
    public class OperationCacheAttribute : Attribute
    {
    }
}