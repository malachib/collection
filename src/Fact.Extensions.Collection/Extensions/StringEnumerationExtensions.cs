using System;
using System.Collections;
using System.Collections.Generic;
#if NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD1_6_2 || NETSTANDARD2_0 || NET46
using System.Linq;
#endif
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public static class StringEnumerationExtensions
    {
        /// <summary>
        /// Note that null strings don't get included
        /// </summary>
        /// <param name="delim"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static string Concat(string delim, IEnumerable<string> s)
        {
            string result = null;

            foreach (var _s in s)
            {
                if (!string.IsNullOrEmpty(_s))
                {
                    if (result != null)
                        result += delim + _s;
                    else
                        result = _s;
                }
            }

            return result;
        }

#if NETSTANDARD1_3 || NETSTANDARD1_6 || NETSTANDARD1_6_2 || NETSTANDARD2_0 || NET46
        /// <summary>
        /// Converts the given enumeration to a string, each item separated by a delimiter.  Be mindful 
        /// empty strings don't get included
        /// </summary>
        /// <returns>
        /// Concat'd string OR null if enumerable was empty
        /// </returns>
        public static string ToString(this IEnumerable enumerable, string delim)
        {
            return Concat(delim, enumerable.Cast<object>().Select(x => x.ToString()));
        }

        /*
        /// <summary>
        /// Converts the given enumeration to a string, each item separated by a delimiter.  Be mindful 
        /// empty strings don't get included
        /// </summary>
        /// <returns>
        /// Concat'd string OR null if enumerable was empty
        /// </returns>
        public static string ToString(this object[] enumerable, string delim)
        {
            return Concat(delim, enumerable.Select(x => x.ToString()));
        }*/
#endif


        /// <summary>
        /// Converts the string enumeration to one string, each item separated by the given delimeter
        /// </summary>
        /// <param name="This"></param>
        /// <param name="delim"></param>
        /// <returns>If enumeration is empty, NULL is returned</returns>
        public static string ToString(this IEnumerable<string> This, string delim)
        {
            return Concat(delim, This);
        }
    }
}
