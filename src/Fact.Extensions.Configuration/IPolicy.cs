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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Be sure this inherits from IPolicy</param>
        /// <param name="key"></param>
        /// <param name="addToContainer"></param>
        /// <returns></returns>
        object GetPolicy(Type t, string key = null, bool addToContainer = true);

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


        /// <summary>
        /// Try to grab the policy from the provider and if it doesn't exist, make a new default one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="addToContainer">If true, adds this policy to container so that next retrieve will find it and
        /// not create a new policy.  Defaults to false</param>
        /// <returns></returns>
        /// <remarks>
        /// Keep in mind that policy providers cascade among each other, but no cascading as of now occurs from within
        /// a policy provider.  In other words, if a class has no policy provider at all, everything will cascade to the System
        /// provider.  If a class DOES have a policy provider, no cascading shall occur.  
        /// 
        /// TODO: We do want a parent-cascade method also, we'll need to reflect, grab the parent chain and try to acquire policy providers
        /// walking up that chain - with any luck the "just in time" static intialization will cover us there, and once we get to "object"
        /// that will be System-singleton provider level
        /// </remarks>
        public static T GetPolicy<T>(this IPolicyProvider policyProvider, string key = null, bool addToContainer = false)
            where T : IPolicy
        {
            return (T)policyProvider.GetPolicy(typeof(T), key, addToContainer);
        }
    }
}
