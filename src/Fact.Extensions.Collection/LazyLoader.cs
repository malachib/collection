using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Memory
{
    /// <summary>
    /// Follows the lazy-load paradigm, won't allocate until value is actually requested
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct LazyLoader<T> where T : class, new()
    {
        T value;

        public bool IsAllocated => value != default(T);

        public T Value
        {
            get
            {
                if (!IsAllocated) value = new T();

                return value;
            }
            set
            {
                // We can just brute force it too... still counts as a lazy load, 
                // incase we want to construct things differently
                this.value = value;
            }
        }


        /// <summary>
        /// Raw value, may be uninitialized
        /// </summary>
        public T RawValue => value;

        /// <summary>
        /// Auto typecast to the lazily loaded value
        /// </summary>
        /// <param name="lazyLoader"></param>
        public static implicit operator T(LazyLoader<T> lazyLoader)
        {
            return lazyLoader.Value;
        }
    }


    public static class LazyLoaderExtensions
    {
        public static TRetVal IfAllocated<T, TRetVal>(this LazyLoader<T> lazyLoader, Func<T, TRetVal> ifThen, Func<TRetVal> ifElse)
            where T : class, new()
        {
            if (lazyLoader.IsAllocated)
            {
                return ifThen(lazyLoader.RawValue);
            }
            else
            {
                return ifElse();
            }
        }


        /// <summary>
        /// Returns value acquired from ifThen delegate if allocated, otherwise returns the default of TRetVal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TRetVal"></typeparam>
        /// <param name="lazyLoader"></param>
        /// <param name="ifThen"></param>
        /// <returns></returns>
        public static TRetVal IfAllocated<T, TRetVal>(this LazyLoader<T> lazyLoader, Func<T, TRetVal> ifThen)
            where T: class, new()
        {
            return lazyLoader.IfAllocated(ifThen, () => default(TRetVal));
        }


        /// <summary>
        /// Performs ifThen delegate if allocated, otherwise performs no action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lazyLoader"></param>
        /// <param name="ifThen"></param>
        /// <returns></returns>
        public static void IfAllocated<T>(this LazyLoader<T> lazyLoader, Action<T> ifThen)
            where T : class, new()
        {
            if (lazyLoader.IsAllocated) ifThen(lazyLoader.RawValue);
        }


        public static TRetVal IfAllocated<T, TRetVal>(this LazyLoader<T> lazyLoader, Func<T, TRetVal> ifThen, Action ifElse)
            where T : class, new()
        {
            if (lazyLoader.IsAllocated)
            {
                return ifThen(lazyLoader.RawValue);
            }
            else
            {
                ifElse();
                return default(TRetVal);
            }
        }
    }
}
