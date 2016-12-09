using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
}
