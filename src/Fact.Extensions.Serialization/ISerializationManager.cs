﻿#if NETSTANDARD1_6
#define FEATURE_ENABLE_PIPELINES
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#if FEATURE_ENABLE_PIPELINES
using System.IO.Pipelines;
#endif

namespace Fact.Extensions.Serialization
{
    public interface ISerializer<TOut>
    {
        void Serialize(TOut output, object inputValue, Type type = null);
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


    public interface ISerializationManagerAsync<TTransportIn, TTransportOut> :
        ISerializerAsync<TTransportOut>,
        IDeserializerAsync<TTransportIn>
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
