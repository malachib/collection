#if NETSTANDARD1_6
#define FEATURE_ENABLE_PIPELINES
#endif

using System;
using System.Collections.Generic;
using System.IO;
#if FEATURE_ENABLE_PIPELINES
using System.IO.Pipelines;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Newtonsoft.Json;

namespace Fact.Extensions.Serialization.Newtonsoft
{
    public class JsonSerializationManager : ISerializationManager<Stream>, 
        ISerializationManager_TextEncoding
    {
        //readonly JsonSerializerSettings settings;
        readonly JsonSerializer serializer = new JsonSerializer();

        public Encoding Encoding => Encoding.UTF8;

        public object Deserialize(Stream input, Type type)
        {
            return serializer.Deserialize(new StreamReader(input), type);
        }

        public void Serialize(Stream output, object inputValue, Type type = null)
        {
            using (var writer = new StreamWriter(output))
            {
                serializer.Serialize(writer, inputValue, type);
            }
        }
    }


#if FEATURE_ENABLE_PIPELINES
    public class ReadableBufferStream : Stream
    {
        readonly ReadableBuffer readableBuffer;

        public ReadableBufferStream(ReadableBuffer readableBuffer)
        {
            this.readableBuffer = readableBuffer;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            readableBuffer.CopyTo(new Span<byte>(buffer, offset, count));
            if (count > readableBuffer.Length)
                return readableBuffer.Length;
            else
                return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// UNTESTED
    /// </summary>
    public class TextSerializationManagerWrapperAsync : 
        ISerializationManagerAsync<IPipelineReader, IPipelineWriter>, 
        ISerializationManager_TextEncoding
    {
        readonly ISerializationManager<Stream, Stream> serializationManager;

        public Encoding Encoding => ((ISerializationManager_TextEncoding)serializationManager).Encoding;

        public async Task<object> DeserializeAsync(IPipelineReader input, Type type)
        {
            var readResult = await input.ReadAsync();
            var stream = new ReadableBufferStream(readResult.Buffer);
            return serializationManager.Deserialize(stream, type);
        }

        public async Task SerializeAsync(IPipelineWriter output, object inputValue, Type type = null)
        {
            var writeBuffer = output.Alloc();
            var stream = new WriteableBufferStream(writeBuffer);
            serializationManager.Serialize(stream, inputValue, type);
            // FIX: following line needs to somehow work for this code to be viable
            //await writer.FlushAsync();
            output.Complete();
        }
    }

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
#endif
        }
