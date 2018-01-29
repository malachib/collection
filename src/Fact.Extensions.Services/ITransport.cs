using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
    public interface IItemAcquirer<T>
    {
        event Action<T> ItemAcquired;
    }


    public interface ISenderAsync<T>
    {
        Task<bool> SendAsync(T packet, CancellationToken cancellationToken);
    }


    public interface ISender<T>
    {
        bool Send(T packet, CancellationToken cancellationToken);
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
