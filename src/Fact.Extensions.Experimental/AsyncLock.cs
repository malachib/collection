using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Experimental
{
    /// <summary>
    /// Simulates to a certain degree inbuilt lock() keyword, but with async capability
    /// Be advised this is NOT reentrant
    /// </summary>
    public class AsyncLock
    {
        SemaphoreSlim mutex = new SemaphoreSlim(1, 1);

        public struct SemWrapper : IDisposable
        {
            readonly SemaphoreSlim mutex;

            internal SemWrapper(SemaphoreSlim mutex)
            {
                this.mutex = mutex;
            }


            public void Dispose()
            {
                mutex.Release();
            }
        }


        /// <summary>
        /// Acquire a scoped lock helper
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public IDisposable Lock(CancellationToken ct = default(CancellationToken))
        {
            mutex.Wait(ct);
            return new SemWrapper(mutex);
        }


        /// <summary>
        /// Acquires a scoped lock helper
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IDisposable> LockAsync(CancellationToken ct = default(CancellationToken))
        {
            await mutex.WaitAsync(ct);
            return new SemWrapper(mutex);
        }
    }
}
