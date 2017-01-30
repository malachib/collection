using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Persist object by reflecting over PersistAttribute-marked fields (not properties, and not public)
    /// </summary>
    public class ReflectionPersistor : PropertySerializerPersistor
    {
        public ReflectionPersistor(Func<IPropertySerializer> serializer, Func<IPropertyDeserializer> deserializer)
            : base(serializer, deserializer) { }



        static IEnumerable<FieldInfo> GetFields(object instance)
        {
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var persistFields = from f in fields
                                let attr = f.GetCustomAttribute<PersistAttribute>()
                                where attr != null
                                select f;
            return persistFields;
        }

        protected override void Serialize(IPropertySerializer serializer, object instance)
        {
            serializer.StartNode("test", null);
            foreach (var field in GetFields(instance))
            {
                var value = field.GetValue(instance);
                serializer[field.Name, field.FieldType] = value;
            }
            serializer.EndNode();
        }

        protected override void Deserialize(IPropertyDeserializer deserializer, object instance)
        {
            object key;
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
}
