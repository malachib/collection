using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
#if NETSTANDARD1_3 || NETSTANDARD1_6
    /// <summary>
    /// Persist object by reflecting over PersistAttribute-marked fields (not properties, and not public)
    /// </summary>
    public class FieldReflectionSerializer
    {
        static IEnumerable<FieldInfo> GetFields(object instance)
        {
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var persistFields = from f in fields
                                let attr = f.GetCustomAttribute<PersistAttribute>()
                                where attr != null
                                select f;
            return persistFields;
        }

        public void Serialize(IPropertySerializer serializer, object instance)
        {
            serializer.StartNode("test", null);
            foreach (var field in GetFields(instance))
            {
                var value = field.GetValue(instance);
                serializer[field.Name, field.FieldType] = value;
            }
            serializer.EndNode();
        }


        public void Deserialize(IPropertyDeserializer deserializer, object instance)
        {
            string key;
            object[] attributes;
            deserializer.StartNode(out key, out attributes);

            foreach (var field in GetFields(instance))
            {
                var value = deserializer.Get(field.Name, field.FieldType);
                field.SetValue(instance, value);
            }

            deserializer.EndNode();
        }
    }
#endif
}
