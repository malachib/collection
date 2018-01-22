using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Experimental
{
    // Does not work, and semaphore seems a bad fit for it
    // In fact starting to look like kind of a disaster... damn
    public class AwaitableCollection<T> : ICollection<T>
    {
        ICollection<T> wrapped;

        //readonly EventWaitHandle evhHasElements = new EventWaitHandle(false, EventResetMode.ManualReset);
        //readonly EventWaitHandle evhHasNoElements = new EventWaitHandle(true, EventResetMode.ManualReset);
        readonly SemaphoreSlim hasElementsSem = new SemaphoreSlim(0);
        readonly SemaphoreSlim hasNoElementsSem = new SemaphoreSlim(1, 1);
        readonly SemaphoreSlim mutexSem = new SemaphoreSlim(1, 1);

        public AwaitableCollection(ICollection<T> wrapped)
        {
            this.wrapped = wrapped;
        }

        public int Count => wrapped.Count;

        public bool IsReadOnly => wrapped.IsReadOnly;

        public void Add(T item)
        {
            mutexSem.Wait();
            // FIX: Make this thread safe, obviously
            if (hasNoElementsSem.CurrentCount == 1)
                // Should not block ever, wait should immediately
                // consume the '1'
                hasNoElementsSem.Wait();

            // Increment has elements counter
            hasElementsSem.Release();
            wrapped.Add(item);
            mutexSem.Release();
        }

        public void Clear()
        {
            throw new InvalidOperationException();
            if (hasNoElementsSem.CurrentCount == 0)
                hasNoElementsSem.Release();
        }

        /// <summary>
        /// Waits for any elements to appear
        /// </summary>
        /// <returns></returns>
        public async Task AwaitElements()
        {
            await hasElementsSem.WaitAsync();
            // give it back, since has elements sem should have a 1:1
            // count with wrapped.Count
            hasElementsSem.Release();
        }


        public async Task AwaitEmpty()
        {
            try
            {
                await mutexSem.WaitAsync();
                if (Count == 0) return; // No awaiting needs to happen if no elements
                                        // wait until flag indicating has no elements is set
                await hasNoElementsSem.WaitAsync();
                // it's briefly not set here (but since it's a distcint semaphore
                // from has elements, should cause no confusion) - so we re-set
                // it.  Only side effect is external consumer will briefly wait
                // on AwaitEmpty as we reset the flag
                hasNoElementsSem.Release();
            }
            finally
            {
                mutexSem.Release();
            }
        }

        public bool Contains(T item) => wrapped.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            wrapped.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() => wrapped.GetEnumerator();

        public bool Remove(T item)
        {
            bool result = wrapped.Remove(item);
            // don't really want to block here, but necessary to decrement
            // hasElemensSem counter.  NOTE: it's VERY likely we'll hit an
            // exception first before reaching a wait state here
            mutexSem.Wait();
            hasElementsSem.Wait();
            if (Count == 0)
            {
                // shouldn't need to worry about releasing more than once,
                // because we mutex ourself during this remove.  One thing though,
                // AwaitEmtpy might happen during this phase...
                hasNoElementsSem.Release();
            }
            mutexSem.Release();
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator() => wrapped.GetEnumerator();
    }
}
