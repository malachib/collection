using Fact.Extensions.Collection;
using Fact.Extensions.Configuration;
using Fact.Extensions.Factories;
using System;
using System.Collections.Generic;
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
        ISerializer<TOut> GetSerializer<TOut>(Type type);
    }


    public interface IDeserializerProvider
    {
        IDeserializer<TIn> GetDeserializer<TIn>(Type type);
    }


    public interface ISerializationProvider : ISerializerProvider, IDeserializerProvider { }


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

        public SerializationProvider(IServiceContainer container)
        {
            this.container = container;
        }

        public SerializationProvider() : this(new LightweightContainer()) { }

        public ISerializer<TOut> GetSerializer<TOut>(Type type)
        {
            // Find a factory for this type of serializer
            var factory = container.Resolve<IFactory<Type, ISerializer<TOut>>>();
            return factory.Create(type);
        }

        public IDeserializer<TIn> GetDeserializer<TIn>(Type type)
        {
            // Find a factory for this type of deserializer
            var factory = container.Resolve<IFactory<Type, IDeserializer<TIn>>>();
            return factory.Create(type);
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


    public class SerializationProviderPropertySerializerConfig
    {
        public Action<TypeSerializerFactory<IPropertyDeserializer, IPropertySerializer>> ConfigureTypeSerializerFactory;
    }
}
