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
        public static async Task<byte[]> SerializeToByteArrayAsync(this ISerializerAsync<PipeWriter> serializationManager, object input, Type type = null)
        {
            // See below comment in DeserializeAsync regarding kludginess of this
            var pipe = new Pipe();
            var writerTask = serializationManager.SerializeAsync(pipe.Writer, input);
            // Happens inside SerializeAsync, but do we always really want to totally end pipeline communication
            // from INSIDE a utility function?
            //pipeline.CompleteWriter(); 
            var result = await pipe.Reader.ReadAsync();
            var buffer = result.Buffer;
            // FIX: This is only converting the FIRST potion, clearly not adequate for long term use
            var returnValue = buffer.First.ToArray();
            return returnValue;
        }


        public static async Task<object> DeserializeAsync(this IDeserializerAsync<PipeReader> serializationManager, byte[] inputValue, Type type)
        {
            var reader = inputValue.AsPipelineReader();

            // Not waiting on awaiter from AsReader because it should be 100% consumed by the read embedded in 
            // DeserializeAsync
            return await serializationManager.DeserializeAsync(reader, type);
        }
    }




    public static class ByteArray_Extensions
    {
        public static PipeReader AsPipelineReader(this byte[] value, out Task awaiter)
        {
            var pipe  = new Pipe();
            var writer = pipe.Writer;
            var _awaiter = writer.WriteAsync(value, CancellationToken.None);
            awaiter = _awaiter.AsTask();
            return pipe.Reader;
        }


        public static PipeReader AsPipelineReader(this byte[] value)
        {
            var pipe = new Pipe();
            var writer = pipe.Writer;
            writer.WriteAsync(value, CancellationToken.None);
            return pipe.Reader;
        }
    }
}
