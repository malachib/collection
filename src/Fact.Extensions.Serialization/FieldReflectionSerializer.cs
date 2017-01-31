using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
#if NETSTANDARD1_6
    /// <summary>
    /// Serializes object by reflecting over PersistAttribute-marked fields (not properties, and not public)
    /// Utilizes IPropertySerializer & IPropertyDeserializer as its transport
    /// </summary>
    public class FieldReflectionSerializer :
        ISerializationManager<IPropertyDeserializer, IPropertySerializer>,
        IInPlaceDeserializer<IPropertyDeserializer>
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

        public void Serialize(IPropertySerializer serializer, object instance, Type type)
        {
            serializer.StartNode("test", null);
            foreach (var field in GetFields(instance))
            {
                var value = field.GetValue(instance);
                serializer[field.Name, field.FieldType] = value;
            }
            serializer.EndNode();
        }


        public object Deserialize(IPropertyDeserializer deserializer, Type type)
        {
            var instance = Activator.CreateInstance(type);
            Deserialize(deserializer, instance, type);
            return instance;
        }


        public void Deserialize(IPropertyDeserializer deserializer, object instance, Type type)
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
#else
    public class FieldReflectionSerializer :
        ISerializer<IPropertySerializer>,
        IDeserializer<IPropertyDeserializer>,
        IInPlaceDeserializer<IPropertyDeserializer>
    {
        public FieldReflectionSerializer()
        {
            throw new Exception("Shim class only for conditional-compile bug resolution. DO NOT USE");
        }
        object IDeserializer<IPropertyDeserializer>.Deserialize(IPropertyDeserializer input, Type type)
        {
            throw new NotImplementedException();
        }

        void IInPlaceDeserializer<IPropertyDeserializer>.Deserialize(IPropertyDeserializer input, object instance, Type type)
        {
            throw new NotImplementedException();
        }

        void ISerializer<IPropertySerializer>.Serialize(IPropertySerializer output, object inputValue, Type type)
        {
            throw new NotImplementedException();
        }
    }
#endif
}
