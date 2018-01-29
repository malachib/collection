using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Services
{
    /// <summary>
    /// Represents what stage of the life cycle has been reached
    /// </summary>
    public enum LifecycleEnum
    {
        Unstarted,
        Starting,
        /// <summary>
        /// only blips, then moves to running
        /// </summary>
        Started,
        /// <summary>
        /// When shutdown begins, and while shutting down
        /// </summary>
        Stopping,
        /// <summary>
        /// When shutdown completes.  May rest in this state
        /// </summary>
        Stopped,
        Sleeping,
        Slept,
        Waking,
        // only blips, then moves to running
        Awake,
        /// <summary>
        /// This is the state which all services seek to be.  The others are either offline 
        /// or transitional states
        /// </summary>
        Running,
        Pausing,
        Paused,
        Resuming,
        // only blips, then moves to running
        Resumed,
        /// <summary>
        /// Service may go offline of its own accord - so announce that with this
        /// Does not necessarily indicate an error (i.e. a modem service could go offline
        /// but not be in error)
        /// </summary>
        Offline,
        /// <summary>
        /// After service has been offline, it can announce it's back online with this
        /// Note that this is a fleeting status, expect immediately for a Running
        /// status to follow
        /// </summary>
        Online,
        /// <summary>
        /// For composite services where subservices are not running, but not in 
        /// an error state.  Needs a better name
        /// Note that since the composite service itself quasi-counts as a service,
        /// even if ALL children are not running, a PartialRunning is still reported
        /// </summary>
        PartialRunning,
        /// <summary>
        /// For composite services where subservices have error states, we report it
        /// this way
        /// </summary>
        Degraded,
        /// <summary>
        /// Service is an error state.  Like Offline except explicitly due to a malfunction
        /// </summary>
        Error
    }

    public interface ILifecycle<TContext>
    {
        Task Startup(TContext context);
        Task Shutdown(TContext context);
    }



    /// <summary>
    /// Pausible means the service can halt its processes immediately,
    /// but does not take any action to clear memory.  Merely a way to
    /// quickly halt and resume processing
    /// </summary>
    public interface IPausibleLifecycle
    {
        Task Pause();
        Task Resume();
    }


    /// <summary>
    /// TODO: A better name would be nice.  I would prefer IExceptionEvent but that
    /// is a bit too much like a delegate name
    /// </summary>
    public interface IExceptionEventProvider
    {
        /// <summary>
        /// Fire this when exceptions occur *outside* of the lifecycle event calls
        /// (i.e. durring running, sleeping, etc)
        /// </summary>
        event Action<Exception> ExceptionOccurred;
    }


    /// <summary>
    /// Optional online/offline events for services which can initiate these states
    /// </summary>
    public interface IOnlineEvents
    {
        event Action Offline;
        /// <summary>
        /// Notify listeners we are back online
        /// Expect a Startup(null) call from an external party
        /// </summary>
        event Action Online;
    }

    /// <summary>
    /// Optional sleepable events for services which can initiate these states
    /// These can be used with or without ISleepableLifecycle
    /// </summary>
    public interface ISleepableEvents
    {
        event Action Sleeping;
        event Action Slept;
        event Action Waking;
        /// <summary>
        /// Notify listeners that we are awake
        /// Expect a Startup() call after firing this from external managing party
        /// </summary>
        event Action Awake;
    }

    /// <summary>
    /// Sleepable means the service is capable of performing tasks necessary
    /// to prepare and recover from a system sleep mode.  Note that this
    /// implies all processes and data are still memory resident
    /// </summary>
    public interface ISleepableLifecycle
    {
        Task Sleep();
        Task Awaken();
    }


    /// <summary>
    /// This is inbetween a sleep and a shutdown.  Freeze persists all relevant
    /// data to a storage area (perhaps mate this to our ISerializable code)
    /// and then frees up as much memory as it can and halts the procsess
    /// </summary>
    public interface IHibernatableLifecycle
    {
        Task Freeze();
        Task Unfreeze();
    }


    public interface ILifecycleDescriptor
    {
        LifecycleEnum LifecycleStatus { get; }

        /// <summary>
        /// First parameter is sender
        /// </summary>
        event Action<object> LifecycleStatusUpdated;
    }
}
