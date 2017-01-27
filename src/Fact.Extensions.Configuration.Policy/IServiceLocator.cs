using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Configuration
{
    /// <summary>
    /// Basically an IoC container facade.  Since .NET Core IoC containers are still
    /// in flux, facade in front of something (TinyIoC probably for now)
    /// </summary>
    /// <remarks>
    /// Pieces lifted and adapted from TinyIoC
    /// </remarks>
    public interface IServiceLocator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolveType"></param>
        /// <param name="name"></param>
        /// <param name="resolvedType"></param>
        /// <returns></returns>
        bool TryResolve(Type resolveType, string name, out object resolvedType);
    }


    public interface IServiceRegistrar
    {
        /// <summary>
        /// Singleton-specific registration.  Typical IoC singleton lifetime: not initialized until
        /// used, remains initialized for remaining app lifetime - only one initialized
        /// </summary>
        /// <param name="registerType"></param>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        void Register(Type registerType, object instance, string name = null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TRegistrationOptions">Container-specific options type for registration</typeparam>
    public interface IServiceRegistrar<TRegistrationOptions> : IServiceRegistrar
    {
        void Register(Type registerType, TRegistrationOptions options);
    }

    public static class IServiceLocator_Extensions
    {
        /// <summary>
        /// Retrieves service or throws exception if not available
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceLocator"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Resolve<T>(this IServiceLocator serviceLocator, string name = null)
        {
            object resolvedType;
            if (!serviceLocator.TryResolve(typeof(T), name, out resolvedType))
                throw new KeyNotFoundException($"Cannot locate name={name} with type={typeof(T)}");

            return (T)resolvedType;
        }
    }
}
