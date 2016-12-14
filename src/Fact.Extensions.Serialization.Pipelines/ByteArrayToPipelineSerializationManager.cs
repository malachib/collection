using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Pipelines
{
    public class ByteArrayToPipelineSerializationManagerAsync : ISerializationManagerAsync<ByteArray>
    {
        ISerializationManagerAsync<IPipelineReader, IPipelineWriter> serializer;

        public async Task<object> DeserializeAsync(ByteArray input, Type type)
        {
            return await serializer.DeserializeAsync(input, type);
        }

        public async Task SerializeAsync(ByteArray output, object inputValue, Type type = null)
        {
            output.Value = await serializer.SerializeToByteArrayAsync(inputValue, type);
        }
    }
}
