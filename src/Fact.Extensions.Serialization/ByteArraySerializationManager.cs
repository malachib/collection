using System;
using System.Collections.Generic;
using System.IO;
#if NETSTANDARD1_6
using System.Linq;
using System.Text;
#endif
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class ByteArray
    {
        public byte[] Value { get; set; }

        public static implicit operator byte[] (ByteArray byteArray)
        {
            return byteArray.Value;
        }
    }

    public class ByteArrayToStreamSerializationManager : 
        ISerializationManager<ByteArray>
    {
        readonly ISerializationManager<Stream> serializationManager;

        public object Deserialize(ByteArray input, Type type)
        {
            return serializationManager.Deserialize(input, type);
        }

        public void Serialize(ByteArray output, object inputValue, Type type = null)
        {
            output.Value = serializationManager.SerializeToByteArray(inputValue, type);
        }
    }


    public class ByteArrayToReaderWriteSerializationManager : ISerializationManager<ByteArray>
    {
        readonly ISerializationManager<TextReader, TextWriter> serializationManager;

        public object Deserialize(ByteArray input, Type type)
        {
            throw new NotImplementedException();
        }

        public void Serialize(ByteArray output, object inputValue, Type type = null)
        {
            throw new NotImplementedException();
        }
    }


#if NETSTANDARD1_6
    public class StreamToReaderWriterSerializationManager : ISerializationManager<Stream>
    {
        readonly ISerializationManager<TextReader, TextWriter> serializer;

        Encoding Encoding
        {
            get
            {
                if (serializer is ISerializationManager_TextEncoding)
                {
                    return ((ISerializationManager_TextEncoding)serializer).Encoding;
                }

                return null;
            }
        }

        public object Deserialize(Stream input, Type type)
        {
            var encoding = Encoding;
            if(encoding != null)
                return serializer.Deserialize(new StreamReader(input, encoding), type);
            else
                return serializer.Deserialize(new StreamReader(input), type);
        }

        public void Serialize(Stream output, object inputValue, Type type = null)
        {
            using (var writer = new StreamWriter(output, Encoding))
            {
                serializer.Serialize(writer, inputValue, type);
            }
        }
    }
#endif
}
