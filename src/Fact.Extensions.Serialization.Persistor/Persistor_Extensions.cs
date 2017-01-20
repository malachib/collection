using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
    }
}
