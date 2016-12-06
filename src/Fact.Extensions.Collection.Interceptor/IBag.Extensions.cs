using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection.Interceptor
{
    public static class IBag_Extensions
    {
        // FIX: temporarily in use until we move this into actual interceptor lib
        public static T ToInterface<T>(this IBag bag)
            where T : class
        {
            var interceptor = new BagInterceptor(bag);
            return AssemblyGlobal.Proxy.CreateInterfaceProxyWithoutTarget<T>(interceptor);
        }
    }
}
