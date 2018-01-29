using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
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
