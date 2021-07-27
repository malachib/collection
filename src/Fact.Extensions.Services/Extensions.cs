using Fact.Extensions.Collection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
    public static class ILifecycleDescriptorExtensions
    {
        public static async Task WaitFor(this ILifecycleDescriptor ld, Func<LifecycleEnum, bool> condition, 
            CancellationToken ct = default(CancellationToken))
        {
            SemaphoreSlim conditionMet = new SemaphoreSlim(0);

            Action<object> responder = v =>
            {
                if (condition(((ILifecycleDescriptor)v).LifecycleStatus)) conditionMet.Release();
            };

            // We don't prime conditionMet until after we attach to this event.  This way
            // the space between prime and event attachment is not left open and unaccounted for
            ld.LifecycleStatusUpdated += responder;

            // AFTER we attach LifecycleStatusUpdated we prime conditionmet semaphore.
            // this MAY result in semaphore reaching TWO, but that is acceptable
            responder(ld);

            await conditionMet.WaitAsync(ct);

            ld.LifecycleStatusUpdated -= responder;

            responder(ld);
        }

        /*
         * Does work, just not using this technique yet
        public static void test(this LifecycleEnum lifecycleEnum)
        {

        } */

        /// <summary>
        /// Is in an online kind of a state
        /// </summary>
        /// <param name="ld"></param>
        /// <returns></returns>
        public static bool IsNominal(this ILifecycleDescriptor ld)
        {
            var status = ld.LifecycleStatus;

            return status == LifecycleEnum.Running;
        }


        /// <summary>
        /// Is in a short term state between long-running states
        /// </summary>
        /// <param name="ld"></param>
        /// <returns></returns>
        public static bool IsTransitioning(this ILifecycleDescriptor ld)
        {
            return ld.LifecycleStatus.IsTransitioning();
        }

        /// <summary>
        /// Is in a short term state between long-running states
        /// </summary>
        /// <returns></returns>
        public static bool IsTransitioning(this LifecycleEnum status)
        {
            switch (status)
            {
                case LifecycleEnum.Online:
                case LifecycleEnum.Pausing:
                case LifecycleEnum.Resuming:
                case LifecycleEnum.Starting:
                case LifecycleEnum.Started:
                case LifecycleEnum.Stopping:
                case LifecycleEnum.Waking:
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Is definitely, positively in an offline kind of state.  Excludes transitioning from one state
        /// Not necessarily an error state, but could be
        /// </summary>
        /// <param name="ld"></param>
        /// <returns></returns>
        public static bool IsNotRunning(this ILifecycleDescriptor ld)
        {
            var status = ld.LifecycleStatus;

            switch (status)
            {
                case LifecycleEnum.Offline:
                case LifecycleEnum.Paused:
                case LifecycleEnum.Slept:
                case LifecycleEnum.Stopped:
                case LifecycleEnum.Unstarted:
                case LifecycleEnum.Error:
                    return true;

                default:
                    return false;
            }
        }
    }


    public static class ServiceManagerExtensions
    {
        public static IServiceDescriptor AddService(this 
            IChildCollection<IServiceDescriptor> serviceManager, IService service, IServiceProvider sp, string name)
        {
            var sd = new ServiceDescriptorBase(sp, service, name);

            serviceManager.AddChild(sd);

            return sd;
        }


        public static IServiceDescriptor<TService> AddService<TService>(
            this IChildCollection<IServiceDescriptor> serviceManager, TService service, IServiceProvider sp, string name)
            where TService : IService
        {
            var sd = new ServiceDescriptorBase<TService>(sp, service, name);

            serviceManager.AddChild(sd);

            return sd;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceManager"></param>
        /// <returns></returns>
        /// <remarks>
        /// Uses DI from serviceManager's ServiceProvider
        /// </remarks>
        public static IServiceDescriptor<TService> AddService<TService>(this ServiceManager serviceManager)
            where TService : IService
        {
            var sp = serviceManager.ServiceProvider;
            var service = sp.GetRequiredService<TService>();

            return serviceManager.AddService(service, sp);
        }


        /// <summary>
        /// Wrap a service with a descriptor and add it to the child collection
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceManager"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IServiceDescriptor<TService> AddService<TService>(this 
            IChildCollection<IServiceDescriptor> serviceManager, TService service, ServiceContext context, string name)
            where TService : IService
        {
            var sd = new ServiceDescriptorBase<TService>(context, service, name);

            serviceManager.AddChild(sd);

            return sd;
        }


        /// <summary>
        /// Acquire the service descriptor associated with a particular service type who is
        /// added to the specified serviceManager.  Also typecasts it to strong
        /// IServiceDescriptor of TService for convenience
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="serviceManager"></param>
        /// <returns></returns>
        public static IServiceDescriptor<TService> GetChild<TService>(
            this IChildProvider<IServiceDescriptor> serviceManager)
            where TService: IService
        {
            IServiceDescriptor sd = serviceManager.Children.Single(x => x.Service is TService);

            return (IServiceDescriptor<TService>)sd;
        }
    }
}
