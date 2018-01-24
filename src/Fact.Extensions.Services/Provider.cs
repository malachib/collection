using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
    public interface IAsyncProvider<T>
    {
        /// <summary>
        /// Acquire the newest/current item - potentially waiting a long time
        /// if necessary (think blocking queue)
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<T> Retrieve(CancellationToken ct);
    }


    public interface IAggregateAsyncProvider<T> :
        IAggregate<IAsyncProvider<T>>,
        IAsyncProvider<T>
    { }
}
