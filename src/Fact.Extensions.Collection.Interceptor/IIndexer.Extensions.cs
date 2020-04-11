using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection.Interceptor
{
    public static class IIndexer_Extensions
    {
        /// <summary>
        /// Experimental
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static T ToInterceptor<T>(this IIndexer<string, object> indexer)
            where T: class
        {
            var interceptor = new NamedIndexerInterceptor<object>(indexer);
            return AssemblyGlobal.Proxy.CreateInterfaceProxyWithoutTarget<T>(interceptor);
            
        }
    }
}
