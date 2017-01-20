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
    public class PersistorSerializable : Persistor, IPersistor
    {
        readonly Func<IPropertySerializer> serializerFactory;
        readonly Func<IPropertyDeserializer> deserializerFactory;

        public PersistorSerializable(Func<IPropertySerializer> serializer, Func<IPropertyDeserializer> deserializer)
        {
            this.serializerFactory = serializer;
            this.deserializerFactory = deserializer;
        }

        public void Persist(object instance)
        {
            Debug.Assert(instance is ISerializable);

            var s = (ISerializable)instance;

            if(Mode == ModeEnum.Serialize)
            {
                var serializer = serializerFactory();
                s.Serialize(serializer, null);
                if(serializer is IDisposable)
                {
                    ((IDisposable)serializer).Dispose();
                }
            }
            else
            {
                var deserializer = deserializerFactory();
                s.Deserialize(deserializer, null);
            }
        }
    }
}
