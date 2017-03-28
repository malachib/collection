#define FEATURE_ENUMEXTENSIONS_PREPEND

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


#if FEATURE_ENUMEXTENSIONS_PREPEND
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
#endif
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
            if(!type.GetTypeInfo().IsValueType) return true;

            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}
