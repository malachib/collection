using Fact.Extensions.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public interface ISerializerFactoryContainer<TIn, TOut>
    {
        IFactory<Type, IDeserializer<TIn>> DeserializerFactory { get; }
        IFactory<Type, ISerializer<TOut>> SerializerFactory { get; }
    }


    /// <summary>
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class AggregateSerializerFactoryContainer<TIn, TOut> : ISerializerFactoryContainer<TIn, TOut>
    {
        readonly AggregateFactory<Type, ISerializer<TOut>> serializerFactory =
            new AggregateFactory<Type, ISerializer<TOut>>();

        readonly AggregateFactory<Type, IDeserializer<TIn>> deserializerFactory =
            new AggregateFactory<Type, IDeserializer<TIn>>();

        public IFactory<Type, IDeserializer<TIn>> DeserializerFactory => deserializerFactory;
        public IFactory<Type, ISerializer<TOut>> SerializerFactory => serializerFactory;

        public void Add(ISerializerFactory<TIn, TOut> sf)
        {
            serializerFactory.Add(sf);
            deserializerFactory.Add(sf);
        }
    }


    public static class AggregateSerializerFactoryContainer_Extensions
    {
        public static void AddFieldReflection(this AggregateSerializerFactoryContainer<IPropertyDeserializer, IPropertySerializer> a)
        {
            a.Add(new FieldReflectionSerializerFactory());
        }


        public static void AddSerializable<TIn, TOut>(this AggregateSerializerFactoryContainer<TIn, TOut> a)
        {
            a.Add(new SerializableSerializerFactory<TIn, TOut>());
        }
    }

}
