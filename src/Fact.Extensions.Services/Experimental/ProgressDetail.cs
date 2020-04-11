using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Services.Experimental
{
    /// <summary>
    /// To use as IProgress[ProgressDetail] rather than IProgress[float]
    /// </summary>
    public struct ProgressDetail
    {
        /// <summary>
        /// Typical progress value, our standard is float of 0-100
        /// Always cumulative
        /// </summary>
        public readonly float Percent;
        /// <summary>
        /// Status message which a human might interpret and enjoy
        /// </summary>
        public readonly string Message;

        public ProgressDetail(float percent, string message = null)
        {
            Percent = percent;
            Message = message;
        }
    }
}
