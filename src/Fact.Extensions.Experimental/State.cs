using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Experimental
{
    /// <summary>
    /// ONly in experimental because I am not sure what other namespace to put it in
    /// Otherwise not considered experimental
    /// </summary>
    public struct State<T>
    {
        T value;

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
                    this.value = value;
                    Changed?.Invoke(value);
                }
            }
        }
    }
}
