using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
    public abstract class WorkerItemAcquirerService<T> :
        WorkerServiceBase,
        IItemAcquirerService<T>
    {
        public event Action<T> ItemAcquired;

        public WorkerItemAcquirerService(ServiceContext context) : base(context) { }

        protected abstract T GetItem(CancellationToken ct);

        protected override async Task Worker(ServiceContext context)
        {
            bool cancelled = false;

            var itemAcquirerTask = Task.Run(() =>
            {
                try
                {
                    return GetItem(context.CancellationToken);
                }
                catch(AggregateException e)
                {
                    // FIX: Feel like we could make this a lot cleaner, but this
                    // should work at a minimum
                    if (e.InnerException is OperationCanceledException)
                    {
                        cancelled = true;
                        return default(T);
                    }
                    else
                        throw;
                }
            });

            var acquiredItem = await itemAcquirerTask;

            if (cancelled) return;

            ItemAcquired?.Invoke(acquiredItem);
        }
    }
}
