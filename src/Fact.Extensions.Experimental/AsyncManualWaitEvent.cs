using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Experimental
{
    // Because development state of AsyncEx is difficult to ascertain, especially for netstandard targets
    // these are low-rent clones of the aformentioned library classes

    public class AsyncManualWaitEvent
    {
        // FIX: arbitrary number just so that competing threads won't think we are unsignalled
        // will have to do better than this when we implement a reset.  At the moment doesn't
        // acutally matter since the mutex stops extra people from coming in
        const int releaseValue = 10;

        SemaphoreSlim eventSem = new SemaphoreSlim(0);
        AsyncLock mutex = new AsyncLock();

        public async Task WaitAsync(CancellationToken ct = default(CancellationToken))
        {
            using (await mutex.LockAsync(ct))
            {
                await eventSem.WaitAsync(ct);
                eventSem.Release();
            }
        }


        /// <summary>
        /// Set to signalled
        /// </summary>
        public void Set()
        {
            // TODO: Need to mutex this
            if (eventSem.CurrentCount == 0)
            {
                eventSem.Release(releaseValue);
            }
        }
    }
}
