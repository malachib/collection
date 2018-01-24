using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

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
                // make sure nobody fiddles with Count
                await mutexSem.WaitAsync();
                if (Count == 0) return; // No awaiting needs to happen if no elements
                                        // wait until flag indicating has no elements is set
            }
            finally
            {
                mutexSem.Release();
            }
            // can safely wait for no elements anytime, once we
            // think we should wait - and if we DON'T have to wait anymore,
            // that's OK too
            await hasNoElementsSem.WaitAsync();
            // it's briefly not set here (but since it's a distcint semaphore
            // from has elements, should cause no confusion) - so we re-set
            // it.  Only side effect is external consumer will briefly wait
            // on AwaitEmpty as we reset the flag
            hasNoElementsSem.Release();
        }

        public bool Contains(T item) => wrapped.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            wrapped.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() => wrapped.GetEnumerator();

        public bool Remove(T item)
        {
            mutexSem.Wait();
            bool result = wrapped.Remove(item);
            // don't really want to block here, but necessary to decrement
            // hasElemensSem counter.  NOTE: it's VERY likely we'll hit an
            // exception first before reaching a wait state here
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


    // Does not work, and semaphore seems a bad fit for it
    // In fact starting to look like kind of a disaster... damn
    public class AwaitableCollection2<T> : ICollection<T>
    {
        ICollection<T> wrapped;

        // reverse mutex means we ARE doing something.  Used to block
        // UNTIL an activity occurs
        readonly SemaphoreSlim reverseMutexSem = new SemaphoreSlim(0, 1);
        readonly SemaphoreSlim mutexSem = new SemaphoreSlim(1, 1);
        // for brief times whether the above two mutexes need to interact
        readonly SemaphoreSlim blackZoneSem = new SemaphoreSlim(1, 1);

        public AwaitableCollection2(ICollection<T> wrapped)
        {
            this.wrapped = wrapped;
        }

        public int Count => wrapped.Count;

        public bool IsReadOnly => wrapped.IsReadOnly;

        public void Add(T item)
        {
            Enter();
            wrapped.Add(item);
            Exit();
        }

        void Enter()
        {
            mutexSem.Wait();
            //blackZoneSem.Wait();
        }

        void Exit()
        {
            // Counting on this Release() to be immediately picked up by
            // any awaiting WaitFor
            reverseMutexSem.Release();
            //blackZoneSem.Release();
            mutexSem.Release();
            reverseMutexSem.Wait();
        }

        public void Clear()
        {
            Enter();
            wrapped.Clear();
            Exit();
        }

        /// <summary>
        /// Waits for a condition.  While condition resolves to false,
        /// we wait for *any* write activity to this collection, then
        /// check condition again
        /// </summary>
        /// <returns></returns>
        public async Task WaitFor(Func<bool> condition, CancellationToken ct)
        {
            // in our activities, mutexSem is always released before initiating
            // a reverseMutex so this is fully safe
            await mutexSem.WaitAsync(ct);

            while (!condition())
            {
                // we've not yet released our mutex, so condition() results
                // should not have changed yet when we get here.  In that
                // protection, now activate blackZoneSem interstice protection
                // for the case where we zip out of the activity and blast back
                // into a new activity, past the mutexSem.wait().  In that case
                // the blackZoneSem.Wait() will stop us from modifying condition()
                // related things
                await blackZoneSem.WaitAsync(ct);
                // we've checked the condition under safe mutexed scenario,
                mutexSem.Release();

                // operations are now protected by black zone semaphore during
                // this brief interstice where mutex is not protecting condition()

                // wait until SOMETHING to happen.  When it does, we will
                // be safe within our mutex
                await reverseMutexSem.WaitAsync(ct);
                // once an activity happens, be sure to release so that the
                // end of that activity can continue
                reverseMutexSem.Release();

                // and interstice protection now can be relieved as well
                blackZoneSem.Release();
                // block again just before we check our condition
                await mutexSem.WaitAsync(ct);
            }
        }


        public async Task AwaitEmpty(CancellationToken ct)
        {
            await WaitFor(() => Count == 0, ct);
        }

        public bool Contains(T item) => wrapped.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            Enter();
            wrapped.CopyTo(array, arrayIndex);
            Exit();
        }

        public IEnumerator<T> GetEnumerator() => wrapped.GetEnumerator();

        public bool Remove(T item)
        {
            Enter();
            bool result = wrapped.Remove(item);
            Exit();
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator() => wrapped.GetEnumerator();
    }



    internal class LocalCancellationTokenBase
    {
        protected readonly CancellationTokenSource localCts;

        protected void Cancel() => localCts.Cancel();

        protected CancellationToken Combine(CancellationToken cancellationToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(
                localCts.Token,
                cancellationToken).Token;
        }
    }
}
