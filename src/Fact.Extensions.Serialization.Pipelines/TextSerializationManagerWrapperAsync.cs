using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Pipelines
{
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
}
