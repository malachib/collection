using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Converts a stream from native unicode to a byte array of the specified encoding
    /// </summary>
    /// <remarks>
    /// Adapted from https://github.com/huseyint/StringStream
    /// </remarks>
    public class ReadonlyStringStream : Stream
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        public ReadonlyStringStream(string data, Encoding encoding)
        {
            Contract.Requires(data != null);
            Contract.Requires(encoding != null);

            this.data = data;
            this.encoding = encoding;
        }

        readonly Encoding encoding;
        readonly string data;

        int position = 0;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override bool CanTimeout => false;
        public override long Length { get { throw new NotImplementedException(); } }

        public override long Position
        {
            get { return position; }
            set { throw new NotImplementedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: position = (int)offset; break;
                case SeekOrigin.Current: position += (int)offset; break;
                case SeekOrigin.End: throw new IndexOutOfRangeException();
            }

            return position;
        }


        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            /*
            var maxByteCount = encoding.GetMaxByteCount(1);

            if (count < maxByteCount)
                throw new InvalidOperationException("Buffer must be at least " + maxByteCount);


            int actualBytesWritten = encoding.GetBytes(data, position, count, buffer, offset);

            position += count;

            return actualBytesWritten;
            */

            var bytesRead = 0;
            var chars = new char[1];

            // Loop until the buffer is full or the string has no more chars
            while (bytesRead < count && position < data.Length)
            {
                // Get the current char to encode
                chars[0] = data[position];

                // Get the required byte count for current char
                var byteCount = encoding.GetByteCount(chars);

                // If adding current char to buffer will exceed its length, do not add it
                if (bytesRead + byteCount > count)
                {
                    return bytesRead;
                }

                // Add the bytes of current char to byte buffer at next index
                encoding.GetBytes(chars, 0, 1, buffer, offset + bytesRead);

                // Increment the string position and total bytes read so far
                position++;
                bytesRead += byteCount;
            }

            return bytesRead;
        }


        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }


    public static class Stream_Extensions
    {
        /// <summary>
        /// Retrieve stream contents as a byte enumerable
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: optimize
        /// bufferSize is hard wired to 1 right now
        /// </remarks>
        public static IEnumerable<byte> Read(this Stream stream, int bufferSize = 1)
        {
            int value;

            while ((value = stream.ReadByte()) != -1) yield return (byte)value;
        }

    }
}
