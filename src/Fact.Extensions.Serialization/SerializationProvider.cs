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
    public interface ISerializationProvider
    {
        ISerializer<TOut> GetSerializer<TOut>(Type type);
        IDeserializer<TIn> GetDeserializer<TIn>(Type type);
    }


    /// <summary>
    /// Home where all serializers and deserializers are registered
    /// </summary>
    public class SerializationProvider : ISerializationProvider
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


        public void Register<TIn, TOut>(SerializerFactory<TIn, TOut> factory)
        {
            container.Register<IFactory<Type, ISerializer<TOut>>>(factory);
            container.Register<IFactory<Type, IDeserializer<TIn>>>(factory);
        }
    }
}
