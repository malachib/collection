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
        readonly ReadResult readableBuffer;

        public ReadableBufferStream(ReadResult readableBuffer)
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
            var rom = new Memory<byte>(buffer, offset, count);
            // FIX: we'll need to interact with more than just 'First'
            readableBuffer.Buffer.First.CopyTo(buffer);
            if (count > readableBuffer.Buffer.Length)
                return (int)readableBuffer.Buffer.Length;
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
        ISerializationManagerAsync<PipeReader, PipeWriter>,
        ISerializationManager_TextEncoding
    {
        readonly ISerializationManager<Stream, Stream> serializationManager;

        public Encoding Encoding => ((ISerializationManager_TextEncoding)serializationManager).Encoding;

        public async Task<object> DeserializeAsync(PipeReader input, Type type)
        {
            var readResult = await input.ReadAsync();
            var stream = new ReadableBufferStream(readResult);
            return serializationManager.Deserialize(stream, type);
        }

        public async Task SerializeAsync(PipeWriter output, object inputValue, Type type = null)
        {
            /* totally different now, and old code probably didnt even work right
            var writeBuffer = output.Alloc();
            var stream = new WriteableBufferStream(writeBuffer);
            serializationManager.Serialize(stream, inputValue, type);
            // FIX: following line needs to somehow work for this code to be viable
            //await writer.FlushAsync();
            output.Complete();
            */           
        }
    }
}
