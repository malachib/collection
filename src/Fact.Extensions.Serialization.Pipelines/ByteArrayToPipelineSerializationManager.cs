using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Pipelines
{
    public class ByteArrayToPipelineSerializationManagerAsync : 
        IByteArraySerializationManagerAsync
    {
        ISerializationManagerAsync<IPipelineReader, IPipelineWriter> serializer;

        public async Task<object> DeserializeAsync(byte[] input, Type type)
        {
            return await serializer.DeserializeAsync(input, type);
        }

        public async Task<byte[]> SerializeAsync(object inputValue, Type type = null)
        {
            return await serializer.SerializeToByteArrayAsync(inputValue, type);
        }
    }
}
