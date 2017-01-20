using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class PersistorBase<TTransport> : Persistor
    {
        readonly TTransport transport;
        readonly ISerializer<TTransport> serializer;
        readonly IDeserializer<TTransport> deserializer;

        public PersistorBase(TTransport transport, ISerializer<TTransport> serializer, IDeserializer<TTransport> deserializer)
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


    /// <summary>
    /// Handles ISerializable cases
    /// </summary>
    /// <remarks>
    /// UNTESTED
    /// </remarks>
    public class PersistorSerializable : PropertySerializerPersistor, IPersistor
    {
        public PersistorSerializable(Func<IPropertySerializer> serializer, Func<IPropertyDeserializer> deserializer)
            : base(serializer, deserializer) { }

        protected override void Serialize(IPropertySerializer serializer, object instance)
        {
            Debug.Assert(instance is ISerializable);

            var s = (ISerializable)instance;
            s.Serialize(serializer, null);
        }

        protected override void Deserialize(IPropertyDeserializer deserializer, object instance)
        {
            Debug.Assert(instance is ISerializable);

            var s = (ISerializable)instance;
            s.Deserialize(deserializer, null);
        }
    }
}
