using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Configuration
{
    public interface IPolicy
    {

    }


    public interface IPolicyProvider
    {
        T GetPolicy<T>(string key = null, bool addToContainer = true)
            where T : IPolicy, new();
    }
}
