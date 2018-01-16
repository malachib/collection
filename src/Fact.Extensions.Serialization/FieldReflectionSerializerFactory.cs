using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Needs more work
    /// </summary>
    public class FieldReflectionSerializerFactory : SerializerFactory<IPropertyDeserializer, IPropertySerializer>
    {
        /// <summary>
        /// EXPERIMENTAL
        /// </summary>
        readonly Dictionary<object, string> registeredKeys = new Dictionary<object, string>();

        string KeyGetter(object instance)
        {
            if (registeredKeys.ContainsKey(instance))
                return registeredKeys[instance];

            return FieldReflectionSerializer.NONODE;
        }

        protected override IDeserializer<IPropertyDeserializer> GetDeserializer(Type id)
        {
            return new FieldReflectionSerializer(KeyGetter);
        }

        protected override ISerializer<IPropertySerializer> GetSerializer(Type id)
        {
            return new FieldReflectionSerializer(KeyGetter);
        }


        public void Register(object instance, string key)
        {
            registeredKeys.Add(instance, key);
        }
    }
}
