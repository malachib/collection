using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Serializes object by reflecting over PersistAttribute-marked fields (not properties, and not public)
    /// Utilizes IPropertySerializer & IPropertyDeserializer as its transport
    /// </summary>
#if NETSTANDARD1_6_OR_GREATER || NET46_OR_GREATER
    public class FieldReflectionSerializer :
        ISerializationManager<IPropertyDeserializer, IPropertySerializer>,
        IInPlaceDeserializer<IPropertyDeserializer>
    {
        /// <summary>
        /// EXPERIMENTAL
        /// magic value to denote on return from keyFinder that actually no node should be rendered
        /// must match ref EXACTLY this is not a string compare, but a ref address compare
        /// </summary>
        public static readonly string NONODE = "nonode";

        /// <summary>
        /// EXPERIMENTAL
        /// delegate to discover what the Node key name should be for a particular instance
        /// Also serves as a flag indicating that a node should be created on serialize (unless NONODE is used)
        /// </summary>
        readonly Func<object, string> keyGetter;
        /// <summary>
        /// EXPERIMENTAL
        /// delegate to use retrieved node key name and (potentially) announce/assign it to an external interested party
        /// Also serves as a flag indicating that a node exists at all on deserialize
        /// </summary>
        readonly Action<object, string> keySetter;

        public FieldReflectionSerializer(Func<object, string> keyFinder = null)
        {
            this.keyGetter = keyFinder;
        }

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
            string key = null;

            // FIX: Need to get proper key (or nothing)
            // keyFinder is experimental at this time
            if (keyGetter != null)
            {
                key = keyGetter(instance);
                if(key != NONODE)
                    serializer.StartNode(key, null);
            }

            foreach (var field in GetFields(instance))
            {
                var value = field.GetValue(instance);
                serializer[field.Name, field.FieldType] = value;
            }

            if(keyGetter != null && key != NONODE) serializer.EndNode();
        }


        public object Deserialize(IPropertyDeserializer deserializer, Type type)
        {
            // FIX: We'll probably want a configurable factory/container for this
            var instance = Activator.CreateInstance(type);

            Deserialize(deserializer, instance, type);
            return instance;
        }


        public void Deserialize(IPropertyDeserializer deserializer, object instance, Type type)
        {
            bool nonode = keyGetter == null ? true : (keyGetter(instance) == NONODE);

            if (!nonode)
            {
                string key;
                object[] attributes;
                deserializer.StartNode(out key, out attributes);
                keySetter?.Invoke(instance, key);
            }

            foreach (var field in GetFields(instance))
            {
                var value = deserializer.Get(field.Name, field.FieldType);
                field.SetValue(instance, value);
            }

            if (!nonode)
                deserializer.EndNode();
        }
    }
#else
    public class FieldReflectionSerializer :
        ISerializer<IPropertySerializer>,
        IDeserializer<IPropertyDeserializer>,
        IInPlaceDeserializer<IPropertyDeserializer>
    {
        public static readonly string NONODE = "nonode";

        public FieldReflectionSerializer()
        {
            throw new Exception("Shim class only for conditional-compile bug resolution. DO NOT USE");
        }

            public FieldReflectionSerializer(Func<object, string> keyFinder = null)
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
