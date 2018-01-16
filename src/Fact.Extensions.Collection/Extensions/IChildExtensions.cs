using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection
{
    public static class IChildExtensions
    {
        static bool DetectNullNameParent<T>(T node)
            where T : INamed, IChild<T>
        {
            return node.Parent.Name == null;
        }

        /// <summary>
        /// String child nodes together to produce something similar to a FQDN
        /// (fully qualified domain name)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="delimiter"></param>
        /// <param name="experimentalAbortProcessor"></param>
        /// <returns></returns>
        public static string GetFullName<T>(this T node, char delimiter = '/',
            Func<T, bool> experimentalAbortProcessor = null)
            where T : INamed, IChild<T>
        {
            var fullName = node.Name;

            while (node.Parent != null)
            {
                if (experimentalAbortProcessor != null && experimentalAbortProcessor(node)) return fullName;

                node = node.Parent;

                // TODO: Ideally this would be more configurable, but will do
                // we skip path building/delimiter concatination if the node has no name
                if (node.Name == null) continue;

                fullName = node.Name + delimiter + fullName;
            }

            return fullName;
        }
    }
}
