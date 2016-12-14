using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public static class ISerializationManager_Extensions
    {
        public static async Task<byte[]> SerializeToByteArrayAsync(this ISerializerAsync<IPipelineWriter> serializationManager, object input, Type type = null)
        {
            // See below comment in DeserializeAsync regarding kludginess of this
            var pipeline = new PipelineFactory().Create();
            var writerTask = serializationManager.SerializeAsync(pipeline, input);
            // Happens inside SerializeAsync, but do we always really want to totally end pipeline communication
            // from INSIDE a utility function?
            //pipeline.CompleteWriter(); 
            var readableBuffer = await pipeline.ReadToEndAsync();
            var returnValue = readableBuffer.ToArray();
            return returnValue;
        }


        public static async Task<object> DeserializeAsync(this IDeserializerAsync<IPipelineReader> serializationManager, byte[] inputValue, Type type)
        {
            var reader = inputValue.AsPipelineReader();

            // Not waiting on awaiter from AsReader because it should be 100% consumed by the read embedded in 
            // DeserializeAsync
            return await serializationManager.DeserializeAsync(reader, type);
        }
    }




    public static class ByteArray_Extensions
    {
        public static UnownedBufferReader AsPipelineReader(this byte[] value, out Task awaiter)
        {
            var reader = new UnownedBufferReader();
            awaiter = reader.WriteAsync(value, CancellationToken.None);
            return reader;
        }


        public static UnownedBufferReader AsPipelineReader(this byte[] value)
        {
            var reader = new UnownedBufferReader();
            reader.WriteAsync(value, CancellationToken.None);
            return reader;
        }
    }
}
