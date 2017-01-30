﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class Method3DelegatePersistor : Persistor, IPersistor
    {
        readonly Delegate method3;

        public Method3DelegatePersistor(Delegate method3)
        {
            this.method3 = method3;
        }

        public void Persist(object instance)
        {
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var persistMethod = method3.GetMethodInfo();
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
                persistMethod.Invoke(method3, parameterValues);
            }
            else
            {
                // effectively map fields to method parameters
                var parameterValues = new object[parameters.Length];

                parameterValues[0] = Mode;
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
        }
    }


    /// <summary>
    /// Utilizing a combination of field-reflection on the instance and method reflection on this Persistor
    /// class, we match up field names to parameter names and pass by ref so as to handle both
    /// serialization and deserialization
    /// </summary>
    public class RefPersistor : Persistor, IPersistor
    {
        /// <summary>
        /// FIX: bad name, refers to method#3 described on IPersistor interface
        /// </summary>
        readonly Persistor refMethod;

        public RefPersistor(Persistor refMethod)
        {
            this.refMethod = refMethod;
        }

        public void Persist(object instance)
        {
            var fields = instance.GetType().GetTypeInfo().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            Persist(instance, fields);
        }

        internal void Persist(object instance, IEnumerable<FieldInfo> fields)
        {
            if(Mode == ModeEnum.Serialize)
            {
                // effectively map fields to method parameters
                var persistMethod = refMethod.GetType().GetTypeInfo().GetMethod("Persist");
                var parameters = persistMethod.GetParameters();
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

                refMethod.Mode = ModeEnum.Serialize;
                persistMethod.Invoke(refMethod, parameterValues);
            }
            else
            {
                // effectively map fields to method parameters
                var persistMethod = refMethod.GetType().GetTypeInfo().GetMethod("Persist");
                var parameters = persistMethod.GetParameters();
                var parameterValues = new object[parameters.Length];

                refMethod.Mode = ModeEnum.Deserialize;
                persistMethod.Invoke(refMethod, parameterValues);


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

    public abstract class PropertySerializerPersistor : Persistor, IPersistor
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
    }


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
            foreach (var field in GetFields(instance))
            {
                var value = field.GetValue(instance);
                serializer[field.Name, field.FieldType] = value;
            }
        }

        protected override void Deserialize(IPropertyDeserializer deserializer, object instance)
        {
            foreach (var field in GetFields(instance))
            {
                var value = deserializer.Get(field.Name, field.FieldType);
                field.SetValue(instance, value);
            }
        }
    }

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
