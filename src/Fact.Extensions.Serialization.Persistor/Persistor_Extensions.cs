using Fact.Extensions.Factories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public static class IPersistor_Extensions
    {
        public static void Serialize(this IPersistor persistor, object instance)
        {
            persistor.Mode = Persistor.ModeEnum.Serialize;
            persistor.Persist(instance);
        }


        /// <summary>
        /// EXPERIMENTAL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="persistor"></param>
        /// <param name="context"></param>
        public static void Serialize<T, TContext>(this IPersistor persistor, IPersistorContext<T, TContext> context)
        {
            persistor.Mode = context.Mode;
            persistor.Persist(context.Instance);
        }


        public static void Deserialize(this IPersistor persistor, object instance)
        {
            persistor.Mode = Persistor.ModeEnum.Deserialize;
            persistor.Persist(instance);
        }


        public static void Deserialize<T>(this IPersistor persistor)
            where T: new()
        {
            var instance = new T();
            // TODO: use this or a full DI container to inject IPersistor into
            // the object's constructor
            //Activator.CreateInstance()
            persistor.Deserialize(instance);
        }


        /// <summary>
        /// Experimental
        /// </summary>
        /// <param name="psc"></param>
        public static void SetJsonFile(this IPersistorSerializationContext<IPropertySerializer> psc, string fileName)
        {
            StreamWriter file = File.CreateText(fileName);
            psc.SetJsonWriter(file, true);
        }


        /// <summary>
        /// Experimental
        /// </summary>
        /// <param name="psc"></param>
        /// <param name="writer"></param>
        /// <param name="closeOnDispose"></param>
        public static void SetJsonWriter(this IPersistorSerializationContext<IPropertySerializer> psc, TextWriter writer, bool closeOnDispose = false)
        {
            var jsonWriter = new JsonTextWriter(writer);
            psc.Context = new JsonPropertySerializer(jsonWriter, closeOnDispose);
        }



        /// <summary>
        /// Experimental
        /// </summary>
        /// <param name="pdc"></param>
        /// <param name="fileName"></param>
        public static void SetJsonFile(this IPersistorDeserializationContext<IPropertyDeserializer> pdc, string fileName)
        {
            StreamReader file = File.OpenText(fileName);
            pdc.SetJsonReader(file);
        }


        /// <summary>
        /// Experimental
        /// </summary>
        /// <param name="pdc"></param>
        /// <param name="reader"></param>
        public static void SetJsonReader(this IPersistorDeserializationContext<IPropertyDeserializer> pdc, TextReader reader)
        {
            var jsonReader = new JsonTextReader(reader);
            jsonReader.Read();
            pdc.Context = new JsonPropertyDeserializer(jsonReader);
        }


        /// <summary>
        /// Experimental ONLY; even if other experimental stuff remains, this needs to go
        /// </summary>
        /// <param name="p"></param>
        /// <param name="psc"></param>
        /// <param name="instance"></param>
        public static void Serialize<T>(this IPersistor _p, IPersistorSerializationContext psc, T instance)
        {
            var p = (IPersistorExperimental)((PersistorShim<T>)_p).Persistor;
            p.Serialize(psc, instance);
        }


        /// <summary>
        /// Experimental
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <param name="instance"></param>
        /// <param name="fileName"></param>
        public static void SerializeToJsonFile<T>(this IPersistorContainer pc, T instance, string fileName)
        {
            using (var psc = new PersistorSerializationContext<IPropertySerializer>())
            {
                psc.SetJsonFile(fileName);

                pc.Serialize(psc, instance);
            }
        }


        /// <summary>
        /// Experimental
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pc"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T DeserializeFromJsonFile<T>(this IPersistorContainer pc, string fileName)
            where T: new()
        {
            var pdc = new PersistorDeserializationContext<IPropertyDeserializer>();

            pdc.SetJsonFile(fileName);
            return pc.Deserialize<T>(pdc);
        }


        /// <summary>
        /// Experimental
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pc"></param>
        /// <param name="psc"></param>
        /// <param name="instance"></param>
        public static void Serialize<T>(this IPersistorContainer pc, IPersistorSerializationContext psc, T instance)
        {
            var _p = pc.ServiceProvider.GetRequiredService<IPersistor<T>>();
            var p = (IPersistorExperimental)((PersistorShim<T>)_p).Persistor;
            p.Serialize(psc, instance);
        }


        /// <summary>
        /// Experimental
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pc"></param>
        /// <param name="psc"></param>
        /// <param name="instance"></param>
        public static void Serialize<T>(this IPersistorContainer pc, IPropertySerializer ps, T instance)
        {
            var _p = pc.ServiceProvider.GetRequiredService<IPersistor<T>>();
            var p = (IPersistorExperimental<IPropertySerializer, IPropertyDeserializer>)((PersistorShim<T>)_p).Persistor;
            p.Serialize(ps, instance);
        }


        /// <summary>
        /// Experimental
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pc"></param>
        /// <param name="pdc"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this IPersistorContainer pc, IPersistorDeserializationContext pdc)
            where T: new()
        {
            var _p = pc.ServiceProvider.GetRequiredService<IPersistor<T>>();
            var p = (IPersistorExperimental)((PersistorShim<T>)_p).Persistor;
            // FIX: bury factory within either IPersistor itself (probably) or maybe IPersistorContext
            var instance = new T();
            return (T) p.Deserialize(pdc, instance);
        }
    }


    public static class IPersistorFactory_Extensions
    {
        public static IPersistorFactory AddRefPersistor(this IPersistorFactory factory, Type t, Persistor persistor)
        {
            var p = new RefPersistor(persistor);
            factory.Register(t, p);
            return factory;
        }


        /// <summary>
        /// Add a RefPersistor type of persistor and associate it with a class T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TPersistor"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IPersistorFactory AddRefPersistor<T, TPersistor>(this IPersistorFactory factory)
            where TPersistor : Persistor, new()
        {
            return factory.AddRefPersistor(typeof(T), new TPersistor());
        }
    }


    public static class IServiceCollection_Extensions
    {
        public static IServiceCollection AddPersistorFactory(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IPersistorFactory, PersistorFactory>();
            serviceCollection.AddSingleton(typeof(IPersistor<>), typeof(PersistorShim<>));
            serviceCollection.AddSingleton(typeof(IPersistorExperimental<,,>), typeof(PersistorExperimentalShim<,,>));
            return serviceCollection;
        }


        public static IServiceCollection AddJsonPersistorFactory(this IServiceCollection serviceCollection)
        {
            var a = new PersistorFactoryAggregator();
            a.Add(new PersistorFactory());
            a.Add(new SerializableInterfacePersistorFactory(null, null));
            //a.Add(new JsonReflectionPersistorFactory());
            a.Add(new DelegateFactory<Type, IPersistor>(
                id => true, 
                id => new JsonReflectionPersistor(null, null)));

            serviceCollection.AddSingleton<IPersistorFactory>(a);
            serviceCollection.AddSingleton(typeof(IPersistor<>), typeof(PersistorShim<>));
            serviceCollection.AddSingleton(typeof(IPersistorExperimental<,,>), typeof(PersistorExperimentalShim<,,>));
            return serviceCollection;
        }


        public static void AddJsonSerializationInterfacePersistor(this IServiceCollection serviceCollection, 
            Func<TextWriter> writerFactory,
            Func<TextReader> readerFactory)
        {

            var p = new PersistorSerializable(() =>
            {
                var writer = writerFactory();
                return new JsonPropertySerializer(new JsonTextWriter(writer));
            }, () =>
            {
                var reader = readerFactory();
                return new JsonPropertyDeserializer(new JsonTextReader(reader));
            });
            serviceCollection.AddSingleton(p);
        }


        public static IServiceCollection AddJsonReflectionPersistor(this IServiceCollection serviceCollection,
            Func<JsonReader> readerFactory, Func<JsonWriter> writerFactory)
        {
            var p = new ReflectionPersistor(
                () => new JsonPropertySerializer(writerFactory()),
                () => new JsonPropertyDeserializer(readerFactory()));
            return serviceCollection.AddSingleton(p);
        }


        /// <summary>
        /// EXPERIMENTAL
        /// I don't like hanging this as an extension right off IServiceProvider, but it has to live somewhere
        /// so it's here for now
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="instance"></param>
        /// <param name="mode"></param>
        public static void Persist<T>(this IServiceProvider sp, T instance, Persistor.ModeEnum mode)
        {
            if(instance is ISerializable)
            {
                var p = sp.GetRequiredService<PersistorSerializable>();
                p.Mode = mode;
                p.Persist(instance);
            }
            else
            {
                // shim class primarily for "method 3" approach
                var p = sp.GetService<IPersistor<T>>();
                if(p != null)
                {
                    p.Mode = mode;
                    p.Persist(instance);
                }
                else
                {
                    // this is the default approach, which is reflection-based
                    // peering in looking for "Persist" attributes
                    var _p = sp.GetRequiredService<ReflectionPersistor>();

                    _p.Mode = mode;
                    _p.Persist(instance);
                }
            }
        }
    }
}
