using Fact.Extensions.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public interface ISerializerFactory<TIn, TOut> :
        IFactory<Type, ISerializer<TOut>>,
        IFactory<Type, IDeserializer<TIn>>
    {

    }

    /// <summary>
    /// EXPERIMENTAL
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



    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    public class SerializableSerializerFactory<TIn, TOut> : SerializerFactory<TIn, TOut>
    {
        protected override IDeserializer<TIn> GetDeserializer(Type id)
        {
            if (typeof(IDeserializable<TIn>).GetTypeInfo().IsAssignableFrom(id.GetTypeInfo()))
                return new SerializableDeserializer<TIn>();
            else
                return null;
        }

        protected override ISerializer<TOut> GetSerializer(Type id)
        {
            if (typeof(ISerializable<TOut>).GetTypeInfo().IsAssignableFrom(id.GetTypeInfo()))
                return new SerializableSerializer<TOut>();
            else
                return null;
        }
    }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    public class FieldReflectionSerializerFactory : SerializerFactory<IPropertyDeserializer, IPropertySerializer>
    {
        protected override IDeserializer<IPropertyDeserializer> GetDeserializer(Type id)
        {
            return new FieldReflectionSerializer(o => "TEST");
        }

        protected override ISerializer<IPropertySerializer> GetSerializer(Type id)
        {
            return new FieldReflectionSerializer(o => "TEST");
        }
    }
}
