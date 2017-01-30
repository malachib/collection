#define NETCORE
#define LWC_ENABLED
#define CODECONTRACTS

using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if CODECONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Configuration
{
    /// <summary>
    /// Shall replace tuning.  Very extension-based to overcome MI-style difficulties
    /// </summary>
    public class PolicyProvider : IPolicyProvider
    {
        /// <summary>
        /// System-wide tuning settings
        /// </summary>
        public static readonly PolicyProvider System = new PolicyProvider();

        /// <summary>
        /// TODO: Not used yet.  Keep in mind PolicyProviders are fully containerized amongst each other.
        /// See GetPolicy for future plans for parent
        /// (as opposed to not at all or lightweight container, the latter being used *internally* to policy provider)
        /// </summary>
        readonly IPolicyProvider parent;

        /// <summary>
        /// If true, any policies queried but not found here will cascade up to System policy provider and be queried for there
        /// If false, any policies queried but not found shall be created new
        /// </summary>
        /// <remarks>
        /// INACTIVE
        /// Remember that if no policyprovider is present, then regardless of this setting, the System policy shall be provided
        /// </remarks>
        public bool Cascade;

        /// <summary>
        /// Retrieve policy provider by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static PolicyProvider Get(string key)
        {
#if CASTLE_IOC_ENABLED
            return Global.Container.TryResolve<PolicyProvider>(key) ?? System;
#elif LWC_ENABLED
            PolicyProvider pp;
            bool result = globalContainer.TryResolve<PolicyProvider>(key, out pp);
            return result ? pp : System;
#else
            PolicyProvider pp;
            bool result = Global.Container.TryResolve<PolicyProvider>(key, out pp);
            return result ? pp : System;
#endif
        }


        /// <summary>
        /// Retrieve policy provider associated with provided class
        /// </summary>
        /// <typeparam name="T">the policy provider associated with the class</typeparam>
        /// <returns></returns>
        public static PolicyProvider Get<T>()
        {
            return Get(typeof(T).FullName);
        }



#if STACKFRAME_ENABLED
        /// <summary>
        /// Retrieve policy provider associated with calling class (via stack inspection)
        /// </summary>
        /// <returns></returns>
        public static PolicyProvider Get()
        {
            var frame = new System.Diagnostics.StackFrame(1);
            var callingMethod = frame.GetMethod();
            var callingType = callingMethod.DeclaringType;

            return Get(callingType.FullName);
        }
#endif

        /// <summary>
        /// Like TinyIoC or Castle, but even lighter weight.  Manages policies tracked by only this
        /// level of policy provider
        /// </summary>
        readonly LightweightContainer container = new LightweightContainer();

#if LWC_ENABLED
        /// <summary>
        /// To track policy providers themselves
        /// </summary>
        static readonly LightweightContainer globalContainer = new LightweightContainer();
#endif

        /// <summary>
        /// Register a policy with this provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="policy"></param>
        /// <param name="key">If NULL, key is inferred from Alias attribute on class</param>
        public void AddPolicy<T>(T policy, string key = null)
            where T : IPolicy
        {
            AddPolicy(typeof(T), policy, key);
        }

        void AddPolicy(Type t, IPolicy policy, string key = null)
        {
#if CODECONTRACTS
            Contract.Assert(policy != null);
#endif

            container.Register(t, policy, key ?? GetPolicyName(t));
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
        public object GetPolicy(Type t, string key = null, bool addToContainer = false)
        {
            object policy;

#if CODECONTRACTS
            var ti = t.GetTypeInfo();
            Contract.Requires(ti.IsAssignableFrom(typeof(IPolicy).GetTypeInfo()));
#endif

            if (key == null)
                key = GetPolicyName(t);

            if (!TryResolve(t, key, out policy))
            {
                // TODO: Make policy creation itself policy-based to fine tune how
                // policies even get created, rather than just when
                //policy = new T();
#if !CODECONTRACTS
                var ti = t.GetTypeInfo();
#endif
                if (ti.IsInterface)
                {
                    var defaultPolicy = ti.GetCustomAttribute<DefaultPolicyAttribute>();
                    Debug.Assert(defaultPolicy != null, "No default policy found for " + ti.Name);
                    policy = Activator.CreateInstance(defaultPolicy.Type);
                }
                else
                {
                    policy = Activator.CreateInstance(t);
                }
                if (addToContainer)
                    container.Register(policy, key);
            }

            return policy;
        }



        /// <summary>
        /// Attempt to acquire policy from local container and then System container, in that order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="policy"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool TryResolve(Type t, string key, out object policy)
        {
            // try for local container first
            if (container.TryResolve(t, key, out policy))
                return true;

            // then do a simple cascade check:
            // try system container, if 'this' is not system container
            if (this != System)
                return System.TryResolve(t, key, out policy);

            // if neither succeed, no such policy exists
            return false;
        }
        

        /// <summary>
        /// Associate this policy provider with the specified type
        /// </summary>
        /// <param name="t"></param>
        public void Register(Type t)
        {
            var name = t.FullName;
#if CASTLE_IOC_ENABLED
            Global.Container.Register(Component.For<PolicyProvider>().Instance(this).Named(name));
#elif LWC_ENABLED
            globalContainer.Register(typeof(PolicyProvider), this, name);
#else
            // TinyIoC version
            Global.Container.Register(typeof(PolicyProvider), this, name);
#endif
        }

        static string GetPolicyName(Type policyType)
        {
            var attr = policyType.
#if NETCORE
                GetTypeInfo().
#endif
                GetCustomAttribute<AliasAttribute>();

            if (attr == null) throw new KeyNotFoundException("Policy type must be marked up with AliasAttribute");

            return attr.Name;
        }


        /// <summary>
        /// Ascertain whether a particular policy type is registered (does not evaluate keys)
        /// </summary>
        /// <param name="policyType"></param>
        /// <returns></returns>
        public bool IsRegistered(Type policyType)
        {
            if (container == null) return false;

            return container.IsRegistered(policyType);//.Registrations.Any(x => x.Key == policyType);
        }


        public void RemovePolicy(Type policy)
        {
        }


        /// <summary>
        /// Temporary function to smooth access to policy config-level settings
        /// NOTE: always returns null for MonoDroid
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static string GetSetting(Type policy, string setting)
        {
            // MonoDroid (and NETCORE, soon) doesn't get at config this way,
            // so for now this call is a noop 
#if !LEGACY_CONFIGURATION_ENABLED
            return null;
#else
            var fullSettingName = GetPolicyName(policy) + "." + setting;

            var value = global::System.Configuration.ConfigurationManager.AppSettings[fullSettingName];

            return value;
#endif
        }
    }
}
