using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class JsonReflectionPersistor : ReflectionPersistor
    {
        public JsonReflectionPersistor(Func<JsonReader> readerFactory, Func<JsonWriter> writerFactory)
            : base(() => new JsonPropertySerializer(writerFactory()),
                  () => new JsonPropertyDeserializer(readerFactory()))
        {
        }
    }
}
