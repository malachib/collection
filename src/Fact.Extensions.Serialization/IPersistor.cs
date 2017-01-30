using Fact.Extensions.Factories;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Persister takes Serialization to the next level
    /// a) It encapsulates/hides the transport used for serialization
    /// b) it provides one of 3 mechanisms for serialization:
    ///   1.  raw-field-based via reflection + PersistAttribute
    ///   2.  classic ISerializable-style interface with explicit save/restore via 
    ///       IPropertySerializer/IPropertyDeserializer similar to old SerializerInfo
    ///   3.  byref+reflection based "Persist(ref T val1, ref T val2)"
    /// </summary>
    /// <remarks>
    /// SerializationInfo and friends feel so in-flux / detested that I am going to re-implement
    /// some of its functionality my own way.  I personally liked SerializationInfo, but utilizing
    /// it at this point is fighting a trend vs rolling my own
    /// </remarks>
    public interface IPersistor
    {
        void Persist(object instance);

        Persistor.ModeEnum Mode { set; }

        IPersistorContext Context { set; }
    }


    public interface IPersistorExperimental
    {
        void Serialize(IPersistorSerializationContext context, object instance);
        object Deserialize(IPersistorDeserializationContext context, object instance = null);
    }


    public interface IPersistorExperimental<TSerializationContext, TDeserializationContext>
    {
        void Serialize(TSerializationContext context, object instance);
        object Deserialize(TDeserializationContext context, object instance = null);
    }


    public interface IPersistorExperimental<T, TSerializationContext, TDeserializationContext>
        where T: class
    {
        void Serialize(TSerializationContext context, T instance);
        T Deserialize(TDeserializationContext context, T instance = null);
    }


    /// <summary>
    /// Experimental
    /// </summary>
    public interface IPersistorSerializationContext
    {

    }


    /// <summary>
    /// Experimental
    /// </summary>
    public interface IPersistorSerializationContext<TContext> : IPersistorSerializationContext
    {
        TContext Context { get; set; }
    }


    /// <summary>
    /// Experimental
    /// </summary>
    public class PersistorSerializationContext<TContext> : IPersistorSerializationContext<TContext>, IDisposable
    {
        public TContext Context { get; set; }

        public void Dispose()
        {
            if (Context is IDisposable)
                ((IDisposable)Context).Dispose();
        }
    }


    /// <summary>
    /// Experimental
    /// </summary>
    public interface IPersistorDeserializationContext
    {

    }


    /// <summary>
    /// Experimental
    /// </summary>
    public interface IPersistorDeserializationContext<TContext> : IPersistorDeserializationContext
    {
        TContext Context { get; set; }
    }


    /// <summary>
    /// Experimental
    /// </summary>
    public class PersistorDeserializationContext<TContext> : IPersistorDeserializationContext<TContext>
    {
        public TContext Context { get; set; }
    }


    public interface IPersistorContext
    {

    }

    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="T">Class of instance being persisted</typeparam>
    /// <typeparam name="TContext">persist-specific context, perhaps a filename or a connection string</typeparam>
    public interface IPersistorContext<T, TContext> : IPersistorContext
    {
        /// <summary>
        /// Can be null, in which case a new T() is expected to work
        /// </summary>
        IFactory<T> InstanceFactory { get; set; }

        Persistor.ModeEnum Mode { get; set; }

        T Instance { get; set; }

        TContext Context { get; set; }
    }


    public interface IResourcePersisterContext<T> : IPersistorContext<T, object>
    {

    }


    public interface IPersistorFactory : IFactory<Type, IPersistor>
    {
        void Register(Type t, IPersistor persistor);
    }
}
