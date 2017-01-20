using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class Persistor<TTransport> : Persistor
    {
        readonly TTransport transport;
        readonly ISerializer<TTransport> serializer;
        readonly IDeserializer<TTransport> deserializer;

        public Persistor(TTransport transport, ISerializer<TTransport> serializer, IDeserializer<TTransport> deserializer)
        {
            this.transport = transport;
            this.serializer = serializer;
            this.deserializer = deserializer;
        }


        public void Persist(object instance)
        {
            //System.Runtime.Serialization.SerializationInfo;
            //System.Runtime.Versioning.TargetFrameworkAttribute
            //SerializableAttribute
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic);
            var persistFields = from f in fields
                                let attr = f.GetCustomAttribute<PersistAttribute>()
                                where attr != null
                                select f;

            if(Mode == ModeEnum.Serialize)
            {
                IPropertySerializer ps = null;

                foreach(var field in persistFields)
                {
                    var value = field.GetValue(instance);
                    ps[field.Name, field.FieldType] = value;
                }
            }
            else
            {
                IPropertyDeserializer pds = null;

                foreach(var field in persistFields)
                {
                    var value = pds.Get(field.Name, field.FieldType);
                    field.SetValue(instance, value);
                }
            }
        }
    }
}
