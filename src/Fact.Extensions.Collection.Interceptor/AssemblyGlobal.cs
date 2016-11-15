using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Fact.Extensions.Collection.Interceptor
{
    internal class AssemblyGlobal
    {
        static readonly ProxyGenerator Proxy = new ProxyGenerator();

        static AssemblyGlobal()
        {
        }
    }
}
