using System;
using System.Collections.Generic;
using System.Linq;

using Fact.Extensions.States;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// A State Accessor is a more powerful type of session/state bag.  It adds the following abilities:
    /// 
    /// 1) queryable type awareness of contents of bag
    /// 2) a general repeatability as to the order and type of contents (see IParameterProvider)
    /// 3) serializability for the entire bag
    /// 4) dirty state tracking
    /// </summary>
    public interface IStateAccessor :
        IStateAccessorBase,
        IDirtyMarker,
        INamedIndexer<object>,
        IIndexer<int, object>
    {

    }


    /// <summary>
    /// The most basic IStateAccessor, get/set parameters directly by IParameterInfo
    /// </summary>
    public interface IStateAccessorBase : IIndexer<IParameterInfo, object>
    {
        IParameterProvider ParameterProvider { get; }

        /// <summary>
        /// Fired when a parameter is reassigned
        /// </summary>
        event Action<IParameterInfo, object> ParameterUpdated;
    }
}
