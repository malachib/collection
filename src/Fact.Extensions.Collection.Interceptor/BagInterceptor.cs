using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection.Interceptor
{
    public class BagInterceptor : NamedPropertyInterceptor
    {
        readonly IBag bag;

        // FIX: Needs cleanup, definitely redundant and sloppy
        protected override string ResolveName(System.Reflection.PropertyInfo prop)
        {
            if (resolveName != null)
                return resolveName(prop);

            return nameResolver != null ? nameResolver.ResolveName(prop) : prop.Name;
        }

        public BagInterceptor(IBag bag, ResolveNameDelegate resolveName = null)
            : base(null, null, resolveName)
        {
            this.bag = bag;
        }

        protected override object Get(IInvocation invocation, PropertyInfo prop)
        {
            return bag.Get(ResolveName(prop), prop.PropertyType);
        }

        protected override void Set(IInvocation invocation, PropertyInfo prop, object value)
        {
            bag.Set(ResolveName(prop), value, prop.PropertyType);
        }
    }
}
