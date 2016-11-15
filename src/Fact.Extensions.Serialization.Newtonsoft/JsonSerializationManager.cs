using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using global::Newtonsoft.Json;

namespace Fact.Extensions.Serialization.Newtonsoft
{
    public class JsonSerializationManager : ISerializationManager
    {
        //readonly JsonSerializerSettings settings;
        readonly JsonSerializer serializer = new JsonSerializer();

        public object Deserialize(Stream input, Type type)
        {
            return serializer.Deserialize(new StreamReader(input), type);
        }

        public void Serialize(Stream output, object inputValue, Type type = null)
        {
            serializer.Serialize(new StreamWriter(output), inputValue, type);
        }
    }
}
