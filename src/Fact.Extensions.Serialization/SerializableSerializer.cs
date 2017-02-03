using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Abstract class which knows how to persist "serializable" object of the classic is-a behavior
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TIn"></typeparam>
    /// <remarks>TODO: decouple this from Deserializer portion</remarks>
    public abstract class SerializableSerializer<TOut, TIn> : ISerializationManager<TIn, TOut>
    {
        protected abstract TOut GetSerializer();
        protected abstract TIn GetDeserializer();

        public object Deserialize(TIn input, Type type)
        {
            TIn deserializer = GetDeserializer();
            // assert that the incoming type implements IDeserializer<TOut>
            //Debug.Assert(type.GetTypeInfo().IsAssignableFrom()
            object serializable = Activator.CreateInstance(type, deserializer);
            return serializable;
        }

        public void Serialize(TOut output, object inputValue, Type type = null)
        {
            TOut serializer = GetSerializer();
            var serializable = (ISerializable<TOut>)inputValue;

            serializable.Serialize(serializer);
        }
    }
}
