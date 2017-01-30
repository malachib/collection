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
            where T : IPolicy;

        /// <summary>
        /// Associate this policy provider with the specified type
        /// </summary>
        void Register(Type t);
    }


    /// <summary>
    /// Identifies which class is the default policy for an interface
    /// </summary>
    public class DefaultPolicyAttribute : Attribute
    {
        readonly Type type;

        public DefaultPolicyAttribute(Type type) { this.type = type; }

        public Type Type => type;
    }


    public static class IPolicyProvider_Extensions
    {
        public static void Register<T>(this IPolicyProvider policyProvider)
        {
            policyProvider.Register(typeof(T));
        }
    }
}
