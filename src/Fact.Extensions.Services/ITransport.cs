using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
    /// <summary>
    /// Signature for event-firing notification of transport
    /// sourced item acquisition
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IItemAcquirer<T>
    {
        /// <summary>
        /// Fired when item of interest has appeared on the transport
        /// </summary>
        event Action<T> ItemAcquired;
    }

    /// <summary>
    /// Raw signature interface for async sending capability
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISenderAsync<T>
    {
        /// <summary>
        /// Send an item over the transport, asynchronously
        /// </summary>
        /// <param name="output"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>true if send was successful</returns>
        Task<bool> SendAsync(T output, CancellationToken cancellationToken);
    }


    public interface ISender<T>
    {
        /// <summary>
        /// Send an item over the transport
        /// </summary>
        /// <param name="output"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>true if send was successful</returns>
        bool Send(T output, CancellationToken cancellationToken);
    }


    public interface IReceiverAsync<T>
    {
        Task<T> ReceiveAsync(CancellationToken cancellationToken);
    }


    public interface IReceiverService<T> :
        IReceiverAsync<T>,
        IService
    { }


    public interface ISenderService<T> :
        ISender<T>,
        ISenderAsync<T>,
        IService
    {

    }


    public interface IItemAcquirerService<T> :
        IItemAcquirer<T>,
        IService
    {

    }


    public interface ITransport<T, TSender, TReceiver>
        where TSender: ISender<T>, ISenderAsync<T>
    {
        TSender Sender { get; }
        TReceiver Receiver { get; }
    }


    public interface ITransportService<T, TReceiver> : 
        ITransport<T, ISenderService<T>, TReceiver>,
        IService
    {

    }
}
