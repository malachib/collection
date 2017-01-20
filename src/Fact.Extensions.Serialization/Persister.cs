using System;
using System.Collections.Generic;
//using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class Persister : IPersister
    {
        public ModeEnum Mode { get; set; }
        public enum ModeEnum
        {
            /// <summary>
            /// Move from memory representation to outside data destination
            /// </summary>
            Serialize,
            /// <summary>
            /// Move from outside data source to internal memory representation
            /// </summary>
            Deserialize
        }

        public void Persist(object instance)
        {
            throw new NotImplementedException();
        }
    }


    public class Persister<TTransport> : Persister
    {
        readonly TTransport transport;
        readonly ISerializer<TTransport> serializer;
        readonly IDeserializer<TTransport> deserializer;

        public Persister(TTransport transport, ISerializer<TTransport> serializer, IDeserializer<TTransport> deserializer)
        {
            this.transport = transport;
            this.serializer = serializer;
            this.deserializer = deserializer;
        }

        public Persister(object instance)
        {
            //System.Runtime.Serialization.SerializationInfo;
                //System.Runtime.Versioning.TargetFrameworkAttribute
            //SerializableAttribute
        }
    }
}
