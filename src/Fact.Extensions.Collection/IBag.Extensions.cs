using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public static class IBag_Extensions
    {
        public static IBag<string, object> ToBag(this IDictionary<string, object> dictionary)
        {
            return dictionary.ToIndexer().ToBag();
        }
    }
}