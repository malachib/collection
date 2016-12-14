using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Pipelines
{
    /// <summary>
    /// EXPERIMENTAL
    /// Pipeline serialization manager for JsonSerializer
    /// Probably shouldn't use this, instead should make a TextReader/TextWriter wrapper for pipeline serialization
    /// </summary>
    public class JsonSerializationManagerAsync : 
        ISerializationManagerAsync<IPipelineReader, IPipelineWriter>,
        ISerializationManager_TextEncoding
    {
        //readonly JsonSerializerSettings settings;
        readonly JsonSerializer serializer = new JsonSerializer();

        public Encoding Encoding => Encoding.UTF8;

        public async Task<object> DeserializeAsync(IPipelineReader input, Type type)
        {
            // TODO: Works, but seems ReadAsync may be creating more of an intermediate buffer than we actually need
            var readResult = await input.ReadAsync();
            var stream = new ReadableBufferStream(readResult.Buffer);
            var reader = new StreamReader(stream, Encoding);
            return serializer.Deserialize(reader, type);

#if UNUSED
            //new ReadableBufferReader(readableBufferAwaitable.)
            // FIX: Super kludgey and massive overhead, right now I'm just learning how to use this thing
            var buffer = await input.ReadToEndAsync();
            var jsonText = this.Encoding.GetString(buffer.ToArray());
            return serializer.Deserialize(new StringReader(jsonText), type);
#endif
        }


        public async Task SerializeAsync(IPipelineWriter output, object inputValue, Type type = null)
        {
            var writeBuffer = output.Alloc();
            var stream = new WriteableBufferStream(writeBuffer);
            var writer = new StreamWriter(stream, Encoding);
            serializer.Serialize(writer, inputValue, type);
            await writer.FlushAsync();
            //await writer.FlushAsync();
            //await writeBuffer.FlushAsync();
            output.Complete();
        }
    }
}
