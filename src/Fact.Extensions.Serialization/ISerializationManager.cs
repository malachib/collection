#if NETSTANDARD1_6
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
    public interface ISerializationManager<TTransportIn, TTransportOut>
    {
        void Serialize(TTransportOut output, object inputValue, Type type = null);
        object Deserialize(TTransportIn input, Type type);
    }


    public interface ISerializationManagerAsync<TTransportIn, TTransportOut>
    {
        Task SerializeAsync(TTransportOut output, object inputValue, Type type = null);
        Task<object> DeserializeAsync(TTransportIn input, Type type);
    }


    public interface ISerializationManager : ISerializationManager<Stream, Stream>
    {
    }

#if FEATURE_ENABLE_PIPELINES
    public interface ISerializationManagerAsync : 
        ISerializationManagerAsync<IPipelineReader, IPipelineWriter>
    {
    }
#else

    public interface ISerializationManagerAsync : 
        ISerializationManagerAsync<Stream, Stream>
    {
    }

#endif


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
