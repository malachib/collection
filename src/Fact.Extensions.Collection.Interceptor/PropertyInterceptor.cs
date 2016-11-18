#define TYPEINFO_ENABLED

using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection.Interceptor
{
    /// <summary>
    /// Because of (lack of) MI behaviors, actual PropertyInterceptor has to have a few extra
    /// references around for delegates and converters which normally would be an MI thing.
    /// To fuel conditions which don't need all those pluggable conversions, use 
    /// PropertyInterceptorBase directly
    /// </summary>
    public abstract class PropertyInterceptorBase : IInterceptor
    {
        protected abstract void Set(IInvocation invocation, System.Reflection.PropertyInfo prop, object value);
        protected abstract object Get(IInvocation invocation, System.Reflection.PropertyInfo prop);

        /// <summary>
        /// When an invocation is encountered that *isn't* a property, go here
        /// </summary>
        /// <param name="invocation"></param>
        protected virtual void InterceptNonProperty(IInvocation invocation)
        {
            // Frequently we are operating directly on interfaces, but
            // it's conceivable we are working on concrete objects too
            if (invocation.InvocationTarget != null)
                invocation.Proceed();
        }

        protected virtual object GetCoerce(object value, System.Reflection.PropertyInfo prop) { return value; }

        protected virtual object SetCoerce(object value, System.Reflection.PropertyInfo prop) { return value; }

        /// <summary>
        /// If FastMode = true, then only compares for "get_" method prefix and if
        /// that is not present, assumes "set_";
        /// </summary>
        public bool FastMode = true;

        void IInterceptor.Intercept(IInvocation invocation)
        {
            var method = invocation.Method;

            if (method.IsSpecialName)
            {
                // Acquire actual property name
                var name = method.Name.Substring(4);
                var prop = invocation.Method.DeclaringType.GetProperty(name);

                if (method.Name.StartsWith("set_"))
                {
                    var value = SetCoerce(invocation.Arguments[0], prop);

                    Set(invocation, prop, value);
                }
                else if (FastMode || method.Name.StartsWith("get_"))
                {
                    var value = Get(invocation, prop);

                    value = GetCoerce(value, prop);

                    invocation.ReturnValue = value;
                }
                else
                {
                    InterceptNonProperty(invocation);
                }
            }
            else
            {
                InterceptNonProperty(invocation);
            }
        }
    }

    /// <summary>
    /// This hierarchy's core ("is a" behavior) is to know how to push and pull data values to a particular datastore
    /// Other behaviors, i.e. data conversion, default value processing are planned to be pluggable ("has a" behavior)
    /// </summary>
    /// <remarks>
    /// Temporarily living in Config.cs until we decide its fate
    /// Initializer independent
    /// TODO: Make this optionally use a cache
    /// </remarks>
    public abstract class PropertyInterceptor : PropertyInterceptorBase
    {
        public interface IConverter
        {
            object ChangeType(object value, Type type);
        }

        public delegate object ChangeTypeDelegate(object value, Type type);
        /// <summary>
        /// Delegate to use when value comes out NULL, can coerce to something else
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public delegate bool HasDefaultDelegate(System.Reflection.PropertyInfo prop, out object defaultValue);

        protected ChangeTypeDelegate changeTypeGetter;
        protected ChangeTypeDelegate changeTypeSetter;
        /// <summary>
        /// Delegate to use when value comes out NULL, do we want to coerce to something else
        /// </summary>
        protected HasDefaultDelegate hasDefault;

        protected PropertyInterceptor(HasDefaultDelegate hasDefault = null, ChangeTypeDelegate changeType = null)
        {
            this.changeTypeGetter = changeType;
            this.changeTypeSetter = changeType;
            this.hasDefault = hasDefault;
        }


        /// <summary>
        /// Basic supplier of default values, promoting NULL to proper value-based defaults (false, 0, etc)
        /// </summary>
        /// <param name="property"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool HasDefaultValueType(System.Reflection.PropertyInfo property, out object defaultValue)
        {
            if (property.PropertyType.
#if TYPEINFO_ENABLED
                GetTypeInfo().
#endif
                IsValueType)
            {
                defaultValue = Activator.CreateInstance(property.PropertyType);
                return true;
            }

            defaultValue = null;
            return false;
        }


        public interface IDefaultFinder
        {
            bool HasDefault(PropertyInfo prop, out object defaultValue);
        }


#if TEMPORARILY_DISABLED
        public class DefaultFinder : IDefaultFinder
        {
            public static readonly DefaultFinder Singleton = new DefaultFinder();
            static TypeReturnDispatcher<object, object> defaultValueDispatcher = new TypeReturnDispatcher<object, object>();

            static DefaultFinder()
            {
                defaultValueDispatcher.Add<Fact.Apprentice.Configuration.DefaultValueAttribute>(x => x.Default);
                defaultValueDispatcher.Add<System.ComponentModel.DefaultValueAttribute>(x => x.Value);
            }

            public bool HasDefault(System.Reflection.PropertyInfo prop, out object defaultValue)
            {
                // evaluate against all custom attributes, and if one is found, retrieve value as indicated
                // by static constructor dispatcher initializer
                var returnValue = defaultValueDispatcher.Dispatch(prop.GetCustomAttributes(true), true);
                // retrieve value, even if none is found. If none is found, this will be ignored
                defaultValue = returnValue.value;
                // return whether a default value was found
                return returnValue.valueFound;
            }
        }
#endif

        /// <summary>
        /// FIX: Kludge just to get things compiling
        /// </summary>
        internal class DBNull
        {
            internal static readonly DBNull Value = new DBNull();
        }

        /// <summary>
        /// Due to lack of multiple inheritence, making these "has a" vs. "is a".  Only thing I don't like about C#... 
        /// </summary>
        protected IConverter converter;
        protected IDefaultFinder defaultFinder;

        protected override object GetCoerce(object value, System.Reflection.PropertyInfo prop)
        {
            if (hasDefault != null && (value == null || value == DBNull.Value))
                hasDefault(prop, out value);
            // If default is present, it is expected in the native type format, so do not try to convert
            // it from underlying store format to native format
            else if (changeTypeGetter != null)
                value = changeTypeGetter(value, prop.PropertyType);

            if (defaultFinder != null && (value == null || value == DBNull.Value))
                defaultFinder.HasDefault(prop, out value);
            // If default is present, it is expected in the native type format, so do not try to convert
            // it from underlying store format to native format
            else if (converter != null)
                value = converter.ChangeType(value, prop.PropertyType);

            return value;
        }

        protected override object SetCoerce(object value, System.Reflection.PropertyInfo prop)
        {
            if (changeTypeSetter != null)
                value = changeTypeSetter(value, prop.PropertyType);

            if (converter != null)
                value = converter.ChangeType(value, prop.PropertyType);

            return value;
        }
    }

    /// <summary>
    /// Specialized property interceptor which resolves properties down to names,
    /// and implicitly those names apply to whatever underlying data store feeds this interceptor.
    /// also performs "default" attribute value checking.
    /// </summary>
    /// <remarks>
    /// Consider moving "default" attribute value checking all the way down into PropertyInterceptor
    /// </remarks>
    public abstract class NamedPropertyInterceptor : PropertyInterceptor
    {
        protected abstract string ResolveName(System.Reflection.PropertyInfo prop);

        public delegate string ResolveNameDelegate(System.Reflection.PropertyInfo prop);

        protected readonly ResolveNameDelegate resolveName;

        /// <summary>
        /// Due to lack of multiple inheritance, this has to be a has-a instead of an is-a
        /// </summary>
        /// <remarks>Not being used, resolveName delegate seems to be supplanting it</remarks>
        protected INameResolver nameResolver;

        protected NamedPropertyInterceptor(HasDefaultDelegate hasDefault = null, ChangeTypeDelegate changeType = null, ResolveNameDelegate resolveName = null)
            : base(hasDefault, changeType)
        {
            this.resolveName = resolveName;
        }

        /// <summary>
        /// Interface to override default name resolution behaviors
        /// </summary>
        public interface INameResolver
        {
            /// <summary>
            /// Acquire a name associated with propertyInfo, can be one other than
            /// the default .Name provides
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <returns></returns>
            string ResolveName(System.Reflection.PropertyInfo propertyInfo);
        }
    }

}
