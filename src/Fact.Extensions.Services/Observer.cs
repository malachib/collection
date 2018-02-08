using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
    /// <summary>
    /// Implements the observer pattern in a manner friendly to async-await
    /// paradigm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncObserver<T>
    {
        /// <summary>
        /// When someone generates a value of interest, we may inspect it here
        /// Async so time-consuming reactions to observation can be accounted for
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task Observe(T value);
    }


    public interface IAggregateAsyncObserver<T> : 
        IAsyncObserver<T>,
        IAggregate<IAsyncObserver<T>>
    {

    }
}
