using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class RefDelegatePersistor : Persistor, IPersistor
    {
        readonly Delegate refDelegate;

        public RefDelegatePersistor(Delegate refDelegate)
        {
            this.refDelegate = refDelegate;
        }

        public void Persist(object instance)
        {
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var persistMethod = refDelegate.GetMethodInfo();
            var parameters = persistMethod.GetParameters();
            if (parameters[0].ParameterType != typeof(ModeEnum))
                throw new InvalidCastException("First parameter must be mode enum specifier");

            if (Mode == ModeEnum.Serialize)
            {
                var parameterValues = new object[parameters.Length];
                var index = 0;

                foreach (var p in parameters)
                {
                    // match each parameter to a field
                    var field = fields.FirstOrDefault(x => x.Name == p.Name);

                    // if no such field exists, then we abort method 3 processing
                    if (field == null) throw new InvalidOperationException("Persist method specifies invalid parameters: " + p.Name);

                    var value = field.GetValue(instance);

                    parameterValues[index++] = value;
                }

                parameterValues[0] = Mode;
                persistMethod.Invoke(refDelegate, parameterValues);
            }
            else
            {
                // effectively map fields to method parameters
                var parameterValues = new object[parameters.Length];

                parameterValues[0] = Mode;
                persistMethod.Invoke(refDelegate, parameterValues);

                var index = 0;

                foreach (var p in parameters)
                {
                    // match each parameter to a field
                    var field = fields.FirstOrDefault(x => x.Name == p.Name);

                    // if no such field exists, then we abort method 3 processing
                    if (field == null) throw new InvalidOperationException("Persist method specifies invalid parameters: " + p.Name);

                    var value = parameterValues[index++];

                    field.SetValue(instance, value);
                }
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// FIX: Cleanup (perhaps use propertyserializerpersistor) and remove method3persistor code
    /// </remarks>
    public class JsonReflectionPersistor : Persistor, IPersistor
    {
        readonly Func<JsonReader> readerFactory;
        readonly Func<JsonWriter> writerFactory;
        /// <summary>
        /// FIX: bad name, refers to method#3 described on IPersistor interface
        /// </summary>
        readonly RefPersistor method3persistor;

        public JsonReflectionPersistor(Func<JsonReader> readerFactory, Func<JsonWriter> writerFactory, Persistor method3 = null)
        {
            this.readerFactory = readerFactory;
            this.writerFactory = writerFactory;
            if(method3 != null) method3persistor = new RefPersistor(method3);
        }


        public void Persist(object instance)
        {
            if(method3persistor != null)
            {
                method3persistor.Mode = Mode;
                method3persistor.Persist(instance);
                return;
            }
            //var typeInfo = instance.GetType().GetTypeInfo();
            //System.Runtime.Serialization.SerializationInfo;
            //System.Runtime.Versioning.TargetFrameworkAttribute
            //SerializableAttribute
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var persistFields = from f in fields
                                let attr = f.GetCustomAttribute<PersistAttribute>()
                                where attr != null
                                select f;

            if (Mode == ModeEnum.Serialize)
            {
                {
                    var writer = writerFactory();
                    using (var ps = new JsonPropertySerializer(writer))
                    {
                        foreach (var field in persistFields)
                        {
                            var value = field.GetValue(instance);
                            ps[field.Name, field.FieldType] = value;
                        }
                    }
                }
            }
            else
            {
                {
                    var reader = readerFactory();
                    var pds = new JsonPropertyDeserializer(reader);
                    {

                        foreach (var field in persistFields)
                        {
                            var value = pds.Get(field.Name, field.FieldType);
                            field.SetValue(instance, value);
                        }
                    }
                }
            }
        }
    }
}
