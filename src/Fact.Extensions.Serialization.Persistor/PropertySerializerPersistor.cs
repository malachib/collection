using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Base abstract IPersistor class for scenarios which use IPropertySerializer/IPropertyDeserializer
    /// for serialization
    /// </summary>
    public abstract class PropertySerializerPersistor : Persistor, IPersistor, IPersistorExperimental
    {
        protected readonly Func<IPropertySerializer> serializerFactory;
        protected readonly Func<IPropertyDeserializer> deserializerFactory;

        public PropertySerializerPersistor(Func<IPropertySerializer> serializer, Func<IPropertyDeserializer> deserializer)
        {
            this.serializerFactory = serializer;
            this.deserializerFactory = deserializer;
        }


        protected abstract void Serialize(IPropertySerializer serializer, object instance);
        protected abstract void Deserialize(IPropertyDeserializer deserializer, object instance);

        public void Persist(object instance)
        {
            if (Mode == ModeEnum.Serialize)
            {
                var ps = serializerFactory();
                Serialize(ps, instance);
                if (ps is IDisposable) ((IDisposable)ps).Dispose();
            }
            else
            {
                var pds = deserializerFactory();
                Deserialize(pds, instance);
            }
        }

        void IPersistorExperimental.Serialize(IPersistorSerializationContext context, object instance)
        {
            var c = (IPersistorSerializationContext<IPropertySerializer>)context;
            Serialize(c.Context, instance);
        }

        object IPersistorExperimental.Deserialize(IPersistorDeserializationContext context, object instance)
        {
            var c = (IPersistorDeserializationContext<IPropertyDeserializer>)context;
            Deserialize(c.Context, instance);
            return instance;
        }
    }


}
