using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Indicates the implemnting class provides a well-known name
    /// </summary>
    public interface INamed
    {
        string Name { get; }
    }


    /// <summary>
    /// Useful for classes which wrap and manage one prominently important value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValueProvider<T>
    {
        T Value { get; }
    }


    public interface IKeyed<TKey>
    {
        TKey Key { get; }
    }

    namespace Experimental
    {
        public interface INameAndValueProvider<T> :
            INamed,
            IValueProvider<T>
        { }

        public interface IKeyAndValueProvider<TKey, TValue> :
            IKeyed<TKey>,
            IValueProvider<TValue>
        { }
    }
}
