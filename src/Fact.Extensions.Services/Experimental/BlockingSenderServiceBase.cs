using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services.Experimental
{
#if ALPHA
    /// <summary>
    /// ISenderService base class for services which wait on a one-at-a-time blocking send operation
    /// If the operation is not blocking, or partially blocking (ala queued/semaphored) do not use
    /// this class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BlockingSenderServiceBase<T> :
        Cancellable, ISenderService<T>
    {
        protected Fact.Extensions.Experimental.AsyncLock mutex = new Fact.Extensions.Experimental.AsyncLock();

        readonly int shutdownTimeoutMilliseconds;

        public virtual string Name => GetType().Name;

        /// <summary>
        /// Fired when item actually sends
        /// </summary>
        public event Action<T> Sent;

        protected BlockingSenderServiceBase(ServiceContext context, 
            int shutdownTimeoutMilliseconds = 50) : 
            base(context.CancellationToken)
        {
            this.shutdownTimeoutMilliseconds = shutdownTimeoutMilliseconds;
        }

        protected BlockingSenderServiceBase(int shutdownTimeoutMilliseconds = 50)
        {
            this.shutdownTimeoutMilliseconds = shutdownTimeoutMilliseconds;
        }

        /// <summary>
        /// Send which has no async-await capabilities but in fact does block
        /// </summary>
        /// <param name="output"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected abstract bool BlockingSend(T output, CancellationToken ct);

        public bool Send(T output, CancellationToken cancellationToken)
        {
            bool result;

            using (mutex.LockAsync().Result)
            {
                var ct = Combine(cancellationToken);
                result = BlockingSend(output, ct);
            }

            if(result)  Sent?.Invoke(output);

            return result;
        }

        public async Task<bool> SendAsync(T output, CancellationToken cancellationToken)
        {
            bool result;

            using (await mutex.LockAsync())
            {
                var ct = Combine(cancellationToken);

                // effectively turns task/thread pool into the queue
                result = await Task.Run(() => BlockingSend(output, ct), ct);
            }

            if (result) Sent?.Invoke(output);

            return result;
        }

        public abstract Task Startup(ServiceContext context);

        public virtual async Task Shutdown(ServiceContext context)
        {
            CancelAfter(shutdownTimeoutMilliseconds);
            // acquire our final lock only to wait for that last packet send and grab this instance
            // for ourselves
            await mutex.LockAsync(Combine(context.CancellationToken));
        }
    }
#endif
}
