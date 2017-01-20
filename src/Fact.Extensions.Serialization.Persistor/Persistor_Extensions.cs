﻿using Microsoft.Extensions.DependencyInjection;
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

    public static class IServiceCollection_Extensions
    {
        public static void AddPersistor<T>(this IServiceCollection serviceCollection, IPersistor persistor)
        {
            var pShim = new Persistor<T>(persistor);
            serviceCollection.AddSingleton(pShim);
        }


        public static void AddMethod3Persistor<T>(this IServiceCollection serviceCollection, Persistor persistor)
        {
            var p = new Method3Persistor(persistor);
            serviceCollection.AddPersistor<T>(p);
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
                var p = sp.GetService<Persistor<T>>();
                if(p != null)
                {
                    p.Mode = mode;
                    p.Persist(instance);
                }
                else
                {
                    // this is the default approach, which likely is reflection-based
                    // peering in looking for "Persist" attributes
                    var _p = sp.GetRequiredService<IPersistor>();

                    _p.Mode = mode;
                    _p.Persist(instance);
                }
            }
        }
    }
}
