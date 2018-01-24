using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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


    public static class TaskExtensions
    {
        public static Task ContinueWithErrorLogger(this Task task, IServiceProvider sp, string name)
        {
            return ContinueWithErrorLogger(task, sp.GetService<ILoggerFactory>().CreateLogger(name), name);
        }

        /// <summary>
        /// Wire in a generic post-task error logger
        /// </summary>
        /// <param name="task"></param>
        /// <param name="logger"></param>
        /// <param name="name"></param>
        public static Task ContinueWithErrorLogger(this Task task, ILogger logger, string name = null)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    if (name == null) name = t.Id.ToString();

                    if (t.Exception.InnerException is OperationCanceledException)
                    {
                        logger.LogWarning($"Task {name} shut down via operation canceled exception");

                    }
                    else
                        logger.LogError(0, t.Exception.InnerException,
                            $"Task {name} did not complete as expected");
                }

                if (t.IsCanceled)
                {
                    logger.LogDebug($"Task {name} completed unexceptionally");
                }
            });
        }
    }
}
