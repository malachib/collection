using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Newtonsoft.Json;

namespace Fact.Extensions.Serialization.Newtonsoft
{
    public class JsonSerializationManagerNative : 
        ISerializationManager<TextReader, TextWriter>,
        ISerializationManager_TextEncoding
    {
        //readonly JsonSerializerSettings settings;
        readonly JsonSerializer serializer;

        public Encoding Encoding => Encoding.UTF8;

        public JsonSerializationManagerNative(JsonSerializer serializer = null)
        {
            this.serializer = serializer ?? JsonSerializer.CreateDefault();
        }

        public object Deserialize(TextReader input, Type type)
        {
            return serializer.Deserialize(input, type);
        }

        public void Serialize(TextWriter output, object inputValue, Type type = null)
        {
            serializer.Serialize(output, inputValue, type);
        }
    }


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
}
