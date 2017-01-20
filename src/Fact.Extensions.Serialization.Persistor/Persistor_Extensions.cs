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
}
