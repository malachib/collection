using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public interface ISerializationManager
    {
        void Serialize(Stream output, object inputValue, Type type = null);
        object Deserialize(Stream input, Type type);
    }


    public interface ISerializationManagerAsync
    {
        Task SerializeAsync(Stream output, object inputValue, Type type = null);
        Task<object> DeserializeAsync(Stream input, Type type);
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
