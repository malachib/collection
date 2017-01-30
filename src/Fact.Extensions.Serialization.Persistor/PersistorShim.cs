using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Mainly exists as an abstract interface in front of PersistorShim
    /// so that DI mechanisms can pretend we are passing in a key via T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPersistor<T> : IPersistor { }


    /// <summary>
    /// Shim for existing persistor instances to register in a DI container
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Technique shamelessly lifted from ILoggerFactory
    /// </remarks>
    public class PersistorShim<T> : IPersistor<T>
    {
        public readonly IPersistor Persistor;

        public PersistorShim(IPersistorFactory persistorFactory)
        {
            Persistor = persistorFactory.Create(typeof(T));
        }

        public Persistor.ModeEnum Mode
        {
            set { Persistor.Mode = value; }
        }

        public void Persist(object instance)
        {
            Persistor.Persist(instance);
        }
    }
}
