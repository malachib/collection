using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class JsonPersistor : Persistor, IPersistor
    {
        readonly Func<JsonReader> readerFactory;
        readonly Func<JsonWriter> writerFactory;
        /// <summary>
        /// FIX: bad name, refers to method#3 described on IPersistor interface
        /// </summary>
        readonly Persistor method3;

        public JsonPersistor(Func<JsonReader> readerFactory, Func<JsonWriter> writerFactory, Persistor method3 = null)
        {
            this.readerFactory = readerFactory;
            this.writerFactory = writerFactory;
            this.method3 = method3;
        }


        public void Persist(object instance)
        {
            //var typeInfo = instance.GetType().GetTypeInfo();
            //System.Runtime.Serialization.SerializationInfo;
            //System.Runtime.Versioning.TargetFrameworkAttribute
            //SerializableAttribute
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic);
            var persistFields = from f in fields
                                let attr = f.GetCustomAttribute<PersistAttribute>()
                                where attr != null
                                select f;

            if (Mode == ModeEnum.Serialize)
            {
                if (method3 != null)
                {
                    // effectively map fields to method parameters
                    var persistMethod = method3.GetType().GetTypeInfo().GetMethod("Persist");
                    var parameters = persistMethod.GetParameters();
                    var parameterValues = new object[parameters.Length];
                    var index = 0;

                    foreach(var p in parameters)
                    {
                        // match each parameter to a field
                        var field = fields.FirstOrDefault(x => x.Name == p.Name);

                        // if no such field exists, then we abort method 3 processing
                        if (field == null) throw new InvalidOperationException("Persist method specifies invalid parameters: " + p.Name);

                        var value = field.GetValue(instance);

                        parameterValues[index++] = value;
                    }

                    method3.Mode = ModeEnum.Serialize;
                    persistMethod.Invoke(method3, parameterValues);
                }
                else
                {
                    if (method3 != null)
                    {
                        // effectively map fields to method parameters
                        var persistMethod = method3.GetType().GetTypeInfo().GetMethod("Persist");
                        var parameters = persistMethod.GetParameters();
                        var parameterValues = new object[parameters.Length];

                        method3.Mode = ModeEnum.Deserialize;
                        persistMethod.Invoke(method3, parameterValues);


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
                    else
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
            }
            else
            {
                if (method3 != null)
                {
                }
                else
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
