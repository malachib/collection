using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Fact.Extensions.Serialization.Newtonsoft
{
    public class BsonSerializationManager : ISerializationManager
    {
        readonly JsonSerializer serializer = new JsonSerializer();

        public object Deserialize(Stream input, Type type)
        {
            return serializer.Deserialize(new BsonReader(input), type);
        }

        public void Serialize(Stream output, object inputValue, Type type = null)
        {
            using (var writer = new BsonWriter(output))
            {
                serializer.Serialize(writer, inputValue, type);
            }
        }
    }
}
