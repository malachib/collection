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
    public interface ISerializationManager
    {
        void Serialize(Stream output, object inputValue, Type type = null);
        object Deserialize(Stream input, Type type);
    }

#if FEATURE_ENABLE_PIPELINES
    public interface ISerializationManagerAsync
    {
        Task SerializeAsync(IPipelineWriter output, object inputValue, Type type = null);
        Task<object> DeserializeAsync(IPipelineReader input, Type type);
    }
#else

    public interface ISerializationManagerAsync
    {
        Task SerializeAsync(Stream output, object inputValue, Type type = null);
        Task<object> DeserializeAsync(Stream input, Type type);
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
