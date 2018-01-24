using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Specialized and dedicated value holder class whose main purpose is to notify
    /// listeners when an *actual* state change occurs, not just merely an assignment
    /// (which may end up being the same value assigned)
    /// </summary>
    public struct State<T>
    {
        T value;

        /// <summary>
        /// Fired when state is changing from one to the other, but before we assign it
        /// </summary>
        /// <remarks>parameters are old, new</remarks>
        public event Action<T, T> Changing;

        /// <summary>
        /// Fired after state has changed
        /// </summary>
        /// <remarks>
        /// Consider carrying metadata here like property name or something
        /// </remarks>
        public event Action<T> Changed;

        public T Value
        {
            get => value;
            set
            {
                if (!Equals(this.value, value))
                {
                    Changing?.Invoke(this.value, value);
                    this.value = value;
                    Changed?.Invoke(value);
                }
            }
        }

        public static implicit operator T(State<T> state) => state.value;

        // this one wont work cause state.Changed -= d can't run due to it being a ref
#if UNUSED
#if !NET40
        public static SemaphoreSlim GetSemaphoreWaitFor(ref State<T> state, Func<T, bool> condition)
        {
            SemaphoreSlim conditionMet = new SemaphoreSlim(condition(state.value) ? 1 : 0);

            Action<T> d = v =>
            {
                if (condition(v))
                {
                    conditionMet.Release();
                    state.Changed -= d;
                }
            };

            state.Changed += d;

            // FIX: Unsure if it helps us to do this default check
            if (ct == default(CancellationToken))
                await conditionMet.WaitAsync(millisecondsTimeout);
            else
                await conditionMet.WaitAsync(millisecondsTimeout, ct);

            Changed -= d;
        }
#endif
#endif

        // This code doesn't work because even 'this pointer' is actually a value copy,
        // so events never fire
#if UNUSED
#if !NET40
        public async System.Threading.Tasks.Task WaitFor(Func<T, bool> condition, 
            int millisecondsTimeout = Timeout.Infinite, 
            CancellationToken ct = default(CancellationToken))
        {
            SemaphoreSlim conditionMet = new SemaphoreSlim(condition(value) ? 1 : 0);

            Action<T> d = v =>
            {
                if (condition(v)) conditionMet.Release();
            };

            Changed += d;

            // FIX: Unsure if it helps us to do this default check
            if (ct == default(CancellationToken))
                await conditionMet.WaitAsync(millisecondsTimeout);
            else
                await conditionMet.WaitAsync(millisecondsTimeout, ct);

            Changed -= d;
        }
#endif
#endif
    }
}
