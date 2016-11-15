using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public interface ISerializationManager
    {
        void Serialize(System.IO.Stream output, object inputValue, Type type = null);
        object Deserialize(System.IO.Stream input, Type type);
    }


    public interface ISerializationManagerAsync
    {
        Task SerializeAsync(System.IO.Stream output, object inputValue, Type type = null);
        Task<object> DeserializeAsync(System.IO.Stream input, Type type);
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
