using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /*
    /// <summary>
    /// Abstract class which knows how to persist "serializable" object of the classic is-a behavior
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TIn"></typeparam>
    /// <remarks>TODO: decouple this from Deserializer portion</remarks>
    public class SerializableSerializer<TOut, TIn> : ISerializationManager<TIn, TOut>
    {

    }*/


    public class SerializableDeserializer<TIn> : IDeserializer<TIn>
    {
        public object Deserialize(TIn input, Type type)
        {
            // Have to #if out this area instead of entire class because of VS2015 compilation glitch
//#if NETSTANDARD1_6
            //var constructor = type.GetTypeInfo().GetConstructor(new[] { typeof(TIn) });
            // assert that the incoming type implements IDeserializer<TOut>
            //Debug.Assert(type.GetTypeInfo().IsAssignableFrom()
            //object serializable = constructor.Invoke(new[] { input });

            // this can't find the protected constructor with this
            object serializable = Activator.CreateInstance(type, input);
            return serializable;
//#else
//            throw new InvalidOperationException();
//#endif
        }
    }

    public class SerializableSerializer<TOut> : ISerializer<TOut>
    { 
        public void Serialize(TOut output, object inputValue, Type type = null)
        {
            var serializable = (ISerializable<TOut>)inputValue;

            serializable.Serialize(output);
        }
    }


    public class DelegateDeserializer<TIn> : IDeserializer<TIn>
    {
        readonly Func<TIn, Type, object> deserialize;

        public DelegateDeserializer(Func<TIn, Type, object> deserialize)
        {
            this.deserialize = deserialize;
        }

        public object Deserialize(TIn input, Type type)
        {
            return deserialize(input, type);
        }
    }


    public class DelegateSerializer<TOut> : ISerializer<TOut>
    {
        readonly Action<TOut, object, Type> serialize;

        public DelegateSerializer(Action<TOut, object, Type> serialize)
        {
            this.serialize = serialize;
        }

        public void Serialize(TOut output, object inputValue, Type type = null)
        {
            serialize(output, inputValue, type);
        }
    }
}
