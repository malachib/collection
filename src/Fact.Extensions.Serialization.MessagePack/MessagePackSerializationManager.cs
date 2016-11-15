using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.MessagePack
{
    public class MessagePackSerializationManager : ISerializationManager
    {
        public object Deserialize(Stream input, Type type)
        {
            var serializer = SerializationContext.Default.GetSerializer(type);
            return serializer.Unpack(input);
        }

        public void Serialize(Stream output, object inputValue, Type type = null)
        {
            if (type == null) type = inputValue.GetType();

            var serializer = SerializationContext.Default.GetSerializer(type);
            serializer.Pack(output, inputValue);
        }
    }
}
