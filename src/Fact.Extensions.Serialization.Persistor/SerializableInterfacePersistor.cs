using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Handles ISerializable cases
    /// </summary>
    /// <remarks>
    /// UNTESTED
    /// Find better name
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

        public override IPersistorContext Context
        {
            set { }
        }
    }
}
