using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// TODO: Phase out usage of SerializerFactory in favor of ISerializerFactory
    /// </remarks>
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
}
