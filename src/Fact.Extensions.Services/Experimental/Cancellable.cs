using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services.Experimental
{
    /// <summary>
    /// Wraps up a cancellation token source and hides the Cancel methods, making
    /// a "is a" kind of relationship
    /// Experimental because C# favors "has a" so much, perhaps we shouldn't do this
    /// </summary>
    public class Cancellable
    {
        readonly CancellationTokenSource localCts;

        protected Cancellable(CancellationToken combineWith)
        {
            localCts = CancellationTokenSource.CreateLinkedTokenSource(combineWith);
        }

        protected Cancellable()
        {
            localCts = new CancellationTokenSource();
        }


        /// <summary>
        /// Acquire Token associated with local cancellations
        /// OK to be public since we can only read and not act on this
        /// </summary>
        public CancellationToken Token => localCts.Token;


        /// <summary>
        /// Utilizing our own local cancellation token, initiate a Task cancel operation
        /// </summary>
        protected void Cancel() => localCts.Cancel();


        /// <summary>
        /// Utilizing our own local cancellation token, initiate a Task cancel operation
        /// </summary>
        protected void CancelAfter(int millisecondDelay) =>
            localCts.CancelAfter(millisecondDelay);
    }
}
