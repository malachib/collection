#if NETSTANDARD1_6
#define FEATURE_ENABLE_PIPELINES
#endif

using System;
using System.Collections.Generic;
using System.IO;
#if FEATURE_ENABLE_PIPELINES
using System.IO.Pipelines;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Newtonsoft.Json;

namespace Fact.Extensions.Serialization.Newtonsoft
{
    public class JsonSerializationManager : ISerializationManager, 
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


#if FEATURE_ENABLE_PIPELINES
    public class JsonSerializationManagerAsync : ISerializationManagerAsync,
        ISerializationManager_TextEncoding
    {
        //readonly JsonSerializerSettings settings;
        readonly JsonSerializer serializer = new JsonSerializer();

        public Encoding Encoding => Encoding.UTF8;

        public async Task<object> DeserializeAsync(IPipelineReader input, Type type)
        {
            // FIX: Super kludgey and massive overhead, right now I'm just learning how to use this thing
            var buffer = await input.ReadToEndAsync();
            var jsonText = this.Encoding.GetString(buffer.ToArray());
            return serializer.Deserialize(new StringReader(jsonText), type);
        }

        public async Task SerializeAsync(IPipelineWriter output, object inputValue, Type type = null)
        {
            // FIX: Super kludgey and massive overhead, right now I'm just learning how to use this thing
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, inputValue, type);
                var writeBuffer = output.Alloc();
                var bytes = this.Encoding.GetBytes(writer.ToString());
                writeBuffer.Write(bytes);
                await output.Writing;
            }
        }
    }
#endif
}
