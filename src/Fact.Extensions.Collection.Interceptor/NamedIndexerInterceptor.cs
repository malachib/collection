using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection.Interceptor
{
    /// <summary>
    /// Wraps an interceptor around a NamedIndexer
    /// Needs lots of refinement, but core architecture is sound and class works at a
    /// basic level
    /// </summary>
    /// <remarks>Typical values for T are string or object</remarks>
    public class NamedIndexerInterceptor<TValue> : NamedPropertyInterceptor
    {
        IIndexer<string, TValue> indexer;

        protected override string ResolveName(System.Reflection.PropertyInfo prop)
        {
            if (resolveName != null)
                return resolveName(prop);

            return nameResolver != null ? nameResolver.ResolveName(prop) : prop.Name;
        }

        public NamedIndexerInterceptor(IIndexer<string, TValue> namedAccessor,
            HasDefaultDelegate hasDefault = null, ChangeTypeDelegate changeType = null, ResolveNameDelegate resolveName = null)
            : base(hasDefault, changeType, resolveName)
        {
            this.indexer = namedAccessor;
        }

        protected override object Get(IInvocation invocation, System.Reflection.PropertyInfo prop)
        {
            var name = ResolveName(prop);

            return indexer[name];
        }


        protected override void Set(IInvocation invocation, System.Reflection.PropertyInfo prop, object value)
        {
            var name = ResolveName(prop);

            indexer[name] = (TValue)value;
        }
    }
}
