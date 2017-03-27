using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public interface ISerializer<TOut>
    {
        void Serialize(TOut output, object inputValue, Type type = null);
    }


    /// <summary>
    /// Generally, a serializer pushes something over an out channel.  Sometimes though,
    /// the serializer generates the output channel itself.  This is the case with byte array
    /// output channels
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    public interface IAllocatingSerializer<TOut>
    {
        TOut Serialize(object input, Type type = null);
    }


    /// <summary>
    /// Generally, a serializer pushes something over an out channel.  Sometimes though,
    /// the serializer generates the output channel itself.  This is the case with byte array
    /// output channels
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    public interface IAllocatingSerializerAsync<TOut>
    {
        Task<TOut> SerializeAsync(object input, Type type = null);
    }


    public interface IDeserializer<TIn>
    {
        object Deserialize(TIn input, Type type);
    }


    /// <summary>
    /// Classic "is-a" style serializer, designed to be put ON the class
    /// itself being serialized
    /// </summary>
    /// <typeparam name="TSerializer"></typeparam>
    /// <remarks>
    /// We gently avoid this style, perhaps even when using a very generic serializer (such as IPropertySerializer) so
    /// as to avoid implementation-specific (read: serialize destination specific) serialization code
    /// </remarks>
    public interface ISerializable<TSerializer>
    {
        void Serialize(TSerializer serializer);
    }


    /// <summary>
    /// Classic "is-a" deserializer, designed to be put ON the class being deserialized
    /// </summary>
    /// <typeparam name="TDeserializer"></typeparam>
    public interface IInPlaceDeserializable<TDeserializer>
    {
        void Deserialize(TDeserializer deserializer);
    }


    /// <summary>
    /// Classic "is-a" deserializer, designed to be put ON the class being deserialized
    /// 
    /// This interface means that the class shall implement a classic SerializationInfo style
    /// deserialize on the constructor, using the signature:
    /// 
    /// Constructor(TDeserializer deserializer)
    /// </summary>
    public interface IDeserializable<TDeserializer> { }


    /// <summary>
    /// Sister of IAllocatingSerializer, this interface could also be known as INonAllocatingDeserializer
    /// and relegates instance creation to an outside source
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public interface IInPlaceDeserializer<TIn>
    {
        void Deserialize(TIn input, object instance, Type type = null);
    }

    public interface ISerializerAsync<TOut>
    {
        Task SerializeAsync(TOut output, object inputValue, Type type = null);
    }


    public interface IDeserializerAsync<TIn>
    {
        Task<object> DeserializeAsync(TIn input, Type type);
    }

    public interface ISerializationManager<TTransportIn, TTransportOut> : 
        ISerializer<TTransportOut>,
        IDeserializer<TTransportIn>
    {
    }


    public interface IByteArraySerializationManager : 
        IAllocatingSerializer<byte[]>,
        IDeserializer<byte[]>
    {

    }


    public interface ISerializationManagerAsync<TTransportIn, TTransportOut> :
        ISerializerAsync<TTransportOut>,
        IDeserializerAsync<TTransportIn>
    {
    }


    public interface IByteArraySerializationManagerAsync : 
        IAllocatingSerializerAsync<byte[]>,
        IDeserializerAsync<byte[]>
    {

    }


    public interface ISerializationManagerAsync<TTransport> :
        ISerializationManagerAsync<TTransport, TTransport>
    { }


    public interface ISerializationManager<TTransport> : 
        ISerializationManager<TTransport, TTransport>
    {
    }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TTransport"></typeparam>
    public interface ISerializationContext<TTransport>
    {
        TTransport Transport { get; set; }
    }

    /// <summary>
    /// Use for ISerializationManagers which have a constant encoding
    /// </summary>
    public interface ISerializationManager_TextEncoding
    {
        /// <summary>
        /// Indicates which text encoding this serialization manager is using
        /// </summary>
        System.Text.Encoding Encoding { get; }
    }
}
