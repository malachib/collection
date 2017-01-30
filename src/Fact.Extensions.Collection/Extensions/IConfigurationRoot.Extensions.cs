using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Fact.Extensions.Configuration
{
    public static class IConfigurationRoot_Extensions
    {
        /// <summary>
        /// Present configuration root as a stock-standard indexer
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static INamedIndexer<string> ToIndexer(this IConfigurationRoot config)
        {
            return new NamedIndexerWrapperWithKeys<string>(
                key => config[key],
                (key, value) => config[key] = value,
                () => config.AsEnumerable().Select(x => x.Key));
        }
    }
}
