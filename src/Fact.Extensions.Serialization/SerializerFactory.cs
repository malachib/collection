using Fact.Extensions.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// dual factories in one instance: handlers serializers and deserializers
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public interface ISerializerFactory<TIn, TOut> :
        IFactory<Type, ISerializer<TOut>>,
        IFactory<Type, IDeserializer<TIn>>
    {

    }

    /// <summary>
    /// </summary>
    public abstract class SerializerFactory<TIn, TOut> : ISerializerFactory<TIn, TOut>
    {
        protected abstract IDeserializer<TIn> GetDeserializer(Type id);
        protected abstract ISerializer<TOut> GetSerializer(Type id);

        bool IFactory<Type, IDeserializer<TIn>>.CanCreate(Type id)
        {
            // FIX: a tiny bit sloppy not having a proper CanCreate abstract to call
            return GetDeserializer(id) != null;
        }

        bool IFactory<Type, ISerializer<TOut>>.CanCreate(Type id)
        {
            return GetSerializer(id) != null;
        }

        IDeserializer<TIn> IFactory<Type, IDeserializer<TIn>>.Create(Type id)
        {
            var deserializer = GetDeserializer(id);
            return deserializer;
        }

        ISerializer<TOut> IFactory<Type, ISerializer<TOut>>.Create(Type id)
        {
            var serializer = GetSerializer(id);
            return serializer;
        }
    }
}
