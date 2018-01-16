using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if UNUSED
namespace Fact.Extensions.Serialization
{
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
        public abstract class PropertySerializerSerializer<TContext> : ISerializer<TContext>
        {
            public abstract void Serialize(TContext serializer, object instance);

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

            void IPersistorExperimental<IPropertySerializer, IPropertyDeserializer>.Serialize(IPropertySerializer context, object instance)
            {
                Serialize(context, instance);
            }

            object IPersistorExperimental<IPropertySerializer, IPropertyDeserializer>.Deserialize(IPropertyDeserializer context, object instance)
            {
                Deserialize(context, instance);
                return instance;
            }
        }
    }
}
#endif