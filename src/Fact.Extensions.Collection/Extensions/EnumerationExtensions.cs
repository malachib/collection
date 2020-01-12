using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace Fact.Extensions.Collection
{
    public static class EnumerationExtensions
    {
        /// <summary>
        /// If enumeration is natively array, forward cast to that.  Otherwise, calls ToArray()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <returns></returns>
        public static T[] AsArray<T>(this IEnumerable<T> enumeration)
        {
            if (enumeration is T[])
                return (T[])enumeration;
            else
                return enumeration.ToArray();
        }
    }


    // TODO: Put this elsewhere
    public static class TypeExtensions
    {
        /// <summary>
        /// Return true if underlying type is comparable to null
        /// Be advised, this includes string types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullable(this Type type)
        {
#if NET40
            if (!type.IsValueType) return true;
#else
            if (!type.GetTypeInfo().IsValueType) return true;
#endif

            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}

namespace Fact.Extensions.Collection.Compat
{
    /// <summary>
    /// Utilize for pre-netstandard2 compatibility
    /// Not #if'ing because dependencies can break if DLLs depend on this
    /// and #if removed it for netstandard2 scenarios
    /// </summary>
    public static class EnumerationExtensions
    {
        /// <summary>
        /// Prepend particular value at the head of the enumeration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <param name="prependedValue"></param>
        /// <returns></returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumeration, T prependedValue)
        {
            yield return prependedValue;

            foreach (T item in enumeration) yield return item;
        }


        /// <summary>
        /// Appends a particular value to the end of a (returned) enumeration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration"></param>
        /// <param name="appendedValue"></param>
        /// <returns></returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumeration, T appendedValue)
        {
            foreach (T item in enumeration) yield return item;

            yield return appendedValue;
        }
    }
}
