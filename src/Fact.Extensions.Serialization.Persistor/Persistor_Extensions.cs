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
    public static class Persistor_Extensions
    {
        public static void Serialize(this IPersistor persistor, object instance)
        {
            persistor.Mode = Persistor.ModeEnum.Serialize;
            persistor.Persist(instance);
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
    }



    /// <summary>
    /// Shim for existing persistor instances to register in a DI container
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Persistor<T> : Persistor, IPersistor
    {
        readonly IPersistor persistor;

        public Persistor(IPersistor persistor)
        {
            this.persistor = persistor;
        }

        public void Persist(object instance)
        {
            persistor.Mode = Mode;
            persistor.Persist(instance);
        }
    }


    public interface IPersistorFactory : IFactory<Type, IPersistor>
    {
        void Register(Type t, IPersistor persistor);
    }


    /// <summary>
    /// Shim for existing persistor instances to register in a DI container
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Technique shamelessly lifted from ILoggerFactory
    /// </remarks>
    public class PersistorShim<T> : IPersistor
    {
        public readonly IPersistor Persistor;

        public PersistorShim(IPersistorFactory persistorFactory)
        {
            Persistor = persistorFactory.Create(typeof(T));
        }

        public Persistor.ModeEnum Mode
        {
            set { Persistor.Mode = value; }
        }

        public void Persist(object instance)
        {
            Persistor.Persist(instance);
        }
    }


    public static class IServiceCollection_Extensions
    {
        public static void AddPersistor<T>(this IServiceCollection serviceCollection, IPersistor persistor)
        {
            var pShim = new Persistor<T>(persistor);
            serviceCollection.AddSingleton(pShim);
        }


        public static void AddMethod3Persistor<T>(this IServiceCollection serviceCollection, Persistor persistor)
        {
            var p = new RefPersistor(persistor);
            serviceCollection.AddPersistor<T>(p);
        }


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
            where TPersistor: Persistor, new()
        {
            return factory.AddRefPersistor(typeof(T), new TPersistor());
        }


        public static IServiceCollection AddPersistorFactory(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IPersistorFactory>(new PersistorFactory());
            serviceCollection.AddSingleton(typeof(PersistorShim<>));
            return serviceCollection;
        }


        public static void AddPersistorSerializableJson(this IServiceCollection serviceCollection, 
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
                // FIX: this is probably flawed in that callers will probably need to pass in a Type and not
                // a generic T
                var p = sp.GetService<Persistor<T>>();
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
