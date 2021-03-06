﻿using System;
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
