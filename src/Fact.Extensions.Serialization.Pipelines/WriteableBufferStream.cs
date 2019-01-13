using System;
using System.IO;
using System.IO.Pipelines;

namespace Fact.Extensions.Serialization.Pipelines
{
    public class WriteableBufferStream : Stream
    {
        readonly PipeWriter pipeWriter;

        public WriteableBufferStream(PipeWriter pipeWriter)
        {
            this.pipeWriter = pipeWriter;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            pipeWriter.FlushAsync().AsTask().Wait();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
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
            //var span = new Span<byte>(buffer, offset, count);
            var rom = new ReadOnlyMemory<byte>(buffer, offset, count);
            pipeWriter.WriteAsync(rom).AsTask().Wait();
        }
    }
}
