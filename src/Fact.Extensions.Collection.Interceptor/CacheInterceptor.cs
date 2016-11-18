using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Fact.Extensions.Collection;

namespace Fact.Extensions.Collection.Interceptor
{
    public interface ICacheableService { }

    /// <summary>
    /// For services which wish to proactively participate in this caching scheme
    /// </summary>
    public interface ICacheAwareService
    {
        CacheInterceptor CacheInterceptor { get; set; }
    }


    /// <summary>
    /// TODO: Use this instead of object when returning via out param
    /// </summary>
    public interface ICacheInterceptor
    {

    }

    public class CacheInterceptor : PropertyInterceptor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceToCache"></param>
        /// <param name="cache"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static T Intercept<T>(T serviceToCache, IBag cache, string prefix = null)
            where T: class
        {
            return AssemblyGlobal.Proxy.CreateInterfaceProxyWithTarget(serviceToCache, new CacheInterceptor(cache, prefix));
        }

        // Use this if you don't want to use declarative markup on methods
        Dictionary<MethodInfo, OperationCacheAttribute> operativeMethods;

        Dictionary<MethodInfo, OperationCacheAttribute> OperativeMethods
        {
            get { return operativeMethods ?? (operativeMethods = new Dictionary<MethodInfo, OperationCacheAttribute>()); }
        }


        /// <summary>
        /// Use this as an alternative to marking up methods with the OperationCache attribute
        /// Be sure to use the MethodInfo from the service facade (typically the interface) and not
        /// the implementation instance itself
        /// </summary>
        /// <param name="method"></param>
        /// <param name="cache"></param>
        /// <param name="notify"></param>
        public void AddOperativeMethod(MethodInfo method, bool cache, bool notify = false)
        {
            OperativeMethods.Add(method, new OperationCacheAttribute
            {
                Cache = cache,
                Notify = notify
            });
        }

        public readonly IBag Cache;
        public readonly IRemover CacheRemover;
        public readonly ITryGetter CacheTryGet;
        string prefix;

        public CacheInterceptor(IBag cache, string prefix = null)
        {
            this.prefix = prefix;
            this.Cache = cache;
            this.CacheRemover = (IRemover)cache;
            this.CacheTryGet = (ITryGetter)cache;
        }


        string GetPropCacheKey(PropertyInfo prop)
        {
            var key = prefix + ".prop." + prop.Name;
            return key;
        }


        public virtual string GetMethodCacheKey(string methodName, params object[] arguments)
        {
            // TODO: Revise argument flattener, overloaded method with different argument count
            // could cause collision
            var key = prefix + "." + methodName + ":" + arguments.ToString(",");
            return key;
        }

        string GetMethodCacheKey(IInvocation invocation)
        {
            return GetMethodCacheKey(invocation.Method.Name, invocation.Arguments);
        }

        protected override object Get(IInvocation invocation, PropertyInfo prop)
        {
            var key = GetPropCacheKey(prop);
            object value;

            if (!CacheTryGet.TryGet(key, prop.PropertyType, out value))
            {
                invocation.Proceed();
                Cache.Set(key, invocation.ReturnValue, prop.PropertyType);

                // TODO: optimize PropertyInterceptorBase to not double-assign invocation.ReturnValue
                // for now, should not hurt anything
                value = invocation.ReturnValue;
            }

            return value;
        }

        protected override void Set(IInvocation invocation, PropertyInfo prop, object value)
        {
            // A set operation merely removes item from cache.  Reason being, the get operation
            // might be a different result than the set (although usually they are the same, so
            // consider a flag/option for that)
            CacheRemover.Remove(GetPropCacheKey(prop));
        }


        /// <summary>
        /// Removes entry for this method in the cache, including its arguments to qualify
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        public void RemoveCachedMethod(string methodName, params object[] arguments)
        {
            var key = GetMethodCacheKey(methodName, arguments);
            CacheRemover.Remove(key);
        }

        protected override void InterceptNonProperty(IInvocation invocation)
        {
            // FIX: Kludgey assignment of prefix here
            if (prefix == null) prefix = invocation.Method.DeclaringType.Name;

            var method = invocation.Method;
            var attr = method.GetCustomAttribute<OperationCacheAttribute>();
            if (attr == null && operativeMethods != null)
                operativeMethods.TryGetValue(method, out attr);

            if (attr != null)
            {
                if (attr.Notify)
                    MethodCalling?.Invoke(this, invocation);

                if (attr.Cache)
                {
                    var type = method.ReturnType;
                    var key = GetMethodCacheKey(invocation);

                    object returnValue;

                    if (CacheTryGet.TryGet(key, type, out returnValue))
                    {
                        invocation.ReturnValue = returnValue;
                    }
                    else
                    {
                        invocation.Proceed();

                        //cache.Set(key, invocation.ReturnValue, type);
                        Cache[key, type] = invocation.ReturnValue;
                    }
                }
                else
                    invocation.Proceed();
            }
        }

        /// <summary>
        /// Executes just before method is called
        /// </summary>
        public event Action<CacheInterceptor, IInvocation> MethodCalling;
    }


    /// <summary>
    /// Cache output (output parameters not yet supported)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OperationCacheAttribute : Attribute
    {
        /// <summary>
        /// When set to true, fires a notification event when method is called
        /// This is useful to clear out cache on certain operations
        /// Default is false
        /// </summary>
        public bool Notify { get; set; }

        /// <summary>
        /// When set to true, caches output of method.  Default is true
        /// </summary>
        public bool Cache { get; set; } = true;

    }


    public static class ICacheableService_Extensions
    {
        public static T AsCached<T>(this T serviceToCache, IBag cache, string prefix = null)
            where T : class, ICacheableService
        {
            //return AssemblyGlobal.Proxy.CreateInterfaceProxyWithTarget(serviceToCache, new CacheInterceptor(cache, prefix));
            return CacheInterceptor.Intercept<T>(serviceToCache, cache, prefix);
        }

        public static T AsCached<T>(this T serviceToCache, IBag cache, out CacheInterceptor cacheInterceptor, string prefix = null)
            where T: class, ICacheableService
        {
            return AssemblyGlobal.Proxy.CreateInterfaceProxyWithTarget(serviceToCache, cacheInterceptor = new CacheInterceptor(cache, prefix));
            //return CacheInterceptor.Intercept<T>(service, cache);
        }
    }
}