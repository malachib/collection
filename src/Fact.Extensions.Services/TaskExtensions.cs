using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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


        /// <summary>
        /// Ensure we await with a cancellationtoken in mind
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        // follow guidelines here: https://stackoverflow.com/questions/19404199/how-to-to-make-udpclient-receiveasync-cancelable
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            return task.Result;
        }
    }
}
