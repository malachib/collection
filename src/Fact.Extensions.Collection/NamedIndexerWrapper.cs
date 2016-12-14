using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Present an indexer whose underlying getters/setters are delegates
    /// </summary>
    public class NamedIndexerWrapper<T> :
#if DYNAMICBASE
        NamedAccessorBase<T>,
#endif
        INamedIndexer<T>
    {
        readonly Func<string, T> getter;
        readonly Action<string, T> setter;

        public NamedIndexerWrapper(Func<string, T> getter, Action<string, T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }
        public T this[string name]
        {
            get { return getter(name); }
            set { setter(name, value); }
        }
    }


    public class NamedIndexerWrapperWithKeys<TValue> : NamedIndexerWrapper<TValue>, IKeys<string>
    {
        readonly Func<IEnumerable<string>> getKeys;

        public NamedIndexerWrapperWithKeys(
            Func<string, TValue> getter,
            Action<string, TValue> setter,
            Func<IEnumerable<string>> getKeys) :
            base(getter, setter)
        {
            this.getKeys = getKeys;
        }

        public IEnumerable<string> Keys { get { return getKeys(); } }
    }
}
