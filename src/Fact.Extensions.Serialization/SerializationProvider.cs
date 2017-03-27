using Fact.Extensions.Collection;
using Fact.Extensions.Configuration;
using Fact.Extensions.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// </remarks>
    public interface ISerializerProvider
    {
        /// <summary>
        /// EXPERIMENTAL
        /// Returns whether we're able to provide a serializer for the given transport and object type
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        bool HasSerializer<TOut>(Type type);

        /// <summary>
        /// Retrieves a serializer using TOut as a transport context and Type as the object type
        /// to be serialized
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        ISerializer<TOut> GetSerializer<TOut>(Type type);
    }


    public interface IDeserializerProvider
    {
        /// <summary>
        /// EXPERIMENTAL
        /// Is able to provide a deserializer for the given transport and object type
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        bool HasDeserializer<TIn>(Type type);

        IDeserializer<TIn> GetDeserializer<TIn>(Type type);
    }


    public interface ISerializationProvider : ISerializerProvider, IDeserializerProvider { }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    public interface ISerializerProvider<TOut>
    {
        ISerializer<TOut> GetSerializer(Type type);
    }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public interface IDeserializerProvider<TIn>
    {
        IDeserializer<TIn> GetDeserializer(Type type);
    }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public interface ISerializationProvider<TIn, TOut> : ISerializerProvider<TOut>, IDeserializerProvider<TIn> { }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    public interface ISerializerRegistrar<TOut>
    {
        void Register(IFactory<Type, ISerializer<TOut>> factory);
    }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public interface IDeserializerRegistrar<TIn>
    {
        void Register(IFactory<Type, IDeserializer<TIn>> factory);
    }


    public interface ISerializerRegistrar
    {
        void Register<TOut>(IFactory<Type, ISerializer<TOut>> factory);
    }


    public interface IDeserializerRegistrar
    {
        void Register<TIn>(IFactory<Type, IDeserializer<TIn>> factory);
    }


    public interface ISerializationRegistrar : ISerializerRegistrar, IDeserializerRegistrar { }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public interface ISerializationRegistrar<TIn, TOut> : ISerializerRegistrar<TOut>, IDeserializerRegistrar<TIn> { }


    /// <summary>
    /// Home where all serializers and deserializers are registered
    /// </summary>
    /// <remarks>
    /// Specifically, serialization factories are registered here which can differenciate your needs by a combination of:
    /// 1) specified transport
    /// 2) specified type
    /// Cascading styles of serialization per above combination is achieved through AggregateFactory
    /// </remarks>
    public class SerializationProvider :
        ISerializationProvider,
        ISerializationRegistrar
    {
        readonly IServiceContainer container;

        /// <summary>
        /// EXPERIMENTAL only for use by WrappedSerializationProvider
        /// </summary>
        internal IServiceContainer Container => container;

        public SerializationProvider(IServiceContainer container)
        {
            this.container = container;
        }

        public SerializationProvider() : this(new LightweightContainer()) { }


        /// <summary>
        /// EXPERIMENTAL
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasSerializer<TOut>(Type type)
        {
            if(container.CanResolve<TOut>(null))
            {
                var s = container.Resolve<IFactory<Type, ISerializer<TOut>>>();
                return s.CanCreate(type);
            }
            return false;
        }


        public ISerializer<TOut> GetSerializer<TOut>(Type type)
        {
            // Find a factory for this type of serializer
            try
            {
                var factory = container.Resolve<IFactory<Type, ISerializer<TOut>>>();
                return factory.Create(type);
            }
            catch(Exception e)
            {
                throw new KeyNotFoundException("Unable to provide serializer for requested transport: " +
                    typeof(TOut).Name, e);
            }
        }


        /// <summary>
        /// EXPERIMENTAL
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasDeserializer<TIn>(Type type)
        {
            if (container.CanResolve<TIn>(null))
            {
                var d = container.Resolve<IFactory<Type, IDeserializer<TIn>>>();
                return d.CanCreate(type);
            }
            return false;
        }

        public IDeserializer<TIn> GetDeserializer<TIn>(Type type)
        {
            try
            {
                // Find a factory for this type of deserializer
                var factory = container.Resolve<IFactory<Type, IDeserializer<TIn>>>();
                return factory.Create(type);
            }
            catch (Exception e)
            {
                throw new KeyNotFoundException("Unable to provide deserializer for requested transport: " +
                    typeof(TIn).Name, e);
            }
        }

        public void Register<TOut>(IFactory<Type, ISerializer<TOut>> factory)
        {
            container.Register(factory);
        }


        public void Register<TIn>(IFactory<Type, IDeserializer<TIn>> factory)
        {
            container.Register(factory);
        }


        public void Register<TIn, TOut>(ISerializerFactory<TIn, TOut> factory)
        {
            container.Register<IFactory<Type, ISerializer<TOut>>>(factory);
            container.Register<IFactory<Type, IDeserializer<TIn>>>(factory);
        }
    }


    /// <summary>
    /// EXPERIMENTAL
    /// More strongly typed.  Slightly less flexible, but more compile time protection
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class SerializationProvider<TIn, TOut> :
        ISerializationProvider<TIn, TOut>
        //ISerializationRegistrar<TIn, TOut>
    {
        readonly IFactory<Type, ISerializer<TOut>> serializerFactory;
        readonly IFactory<Type, IDeserializer<TIn>> deserializerFactory;

        /// <summary>
        /// Since this flavor of SerializationProvider doesn't aggregate different in/out types,
        /// then we don't have an IoC container inside to track different serializerfactories
        /// </summary>
        /// <param name="serializerFactory"></param>
        /// <param name="deserializerFactory"></param>
        public SerializationProvider(IFactory<Type, ISerializer<TOut>> serializerFactory, IFactory<Type, IDeserializer<TIn>> deserializerFactory)
        {
            this.serializerFactory = serializerFactory;
            this.deserializerFactory = deserializerFactory;
        }

        public ISerializer<TOut> GetSerializer(Type type)
        {
            return serializerFactory.Create(type);
        }

        public IDeserializer<TIn> GetDeserializer(Type type)
        {
            return deserializerFactory.Create(type);
        }
    }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class WrapperSerializationProvider<TIn, TOut> : ISerializationProvider<TIn, TOut>
    {
        readonly SerializationProvider sp;

        public WrapperSerializationProvider(SerializationProvider sp)
        {
            Contract.Assert(sp.Container.CanResolve<IDeserializer<TIn>>(null));
            Contract.Assert(sp.Container.CanResolve<ISerializer<TOut>>(null));
        }

        public IDeserializer<TIn> GetDeserializer(Type type)
        {
            return sp.GetDeserializer<TIn>(type);
        }

        public ISerializer<TOut> GetSerializer(Type type)
        {
            return sp.GetSerializer<TOut>(type);
        }
    }

    public class SerializationProviderPropertySerializerConfig
    {
        public Action<TypeSerializerFactory<IPropertyDeserializer, IPropertySerializer>> ConfigureTypeSerializerFactory;
    }
}
