using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        readonly ILogger logger;

        public event Action<T> ItemAcquired;

        public WorkerItemAcquirerService(ServiceContext context) : base(context)
        {
            logger = context.ServiceProvider.GetService<ILogger<WorkerItemAcquirerService<T>>>();
        }

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
                // FIX: Feel like we could make this a lot cleaner, but this
                // should work at a minimum
                catch (OperationCanceledException)
                {
                    cancelled = true;
                    return default(T);
                }
                catch (AggregateException ae)
                {
                    // FIX: Feel like we could make this a lot cleaner, but this
                    // should work at a minimum
                    if (ae.InnerException is OperationCanceledException)
                    {
                        cancelled = true;
                        return default(T);
                    }
                    else
                    {
                        logger.LogWarning(0, ae, $"Unexpected agg-exception during Worker ({Name})");
                        throw;
                    }
                }
                catch(Exception e)
                {
                    logger.LogWarning(0, e, $"Unexpected exception during Worker ({Name})");
                    throw;
                }
            });

            var acquiredItem = await itemAcquirerTask;

            if (cancelled) return;

            ItemAcquired?.Invoke(acquiredItem);
        }
    }
}
