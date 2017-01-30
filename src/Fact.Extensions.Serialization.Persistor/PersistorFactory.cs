using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class PersistorFactory : IPersistorFactory
    {
        // FIX: Only use this until proper IoC container is available (including LightweightContainer)
        Dictionary<Type, IPersistor> persistors = new Dictionary<Type, IPersistor>();

        public bool CanCreate(Type id)
        {
            return true;
        }

        public IPersistor Create(Type id)
        {
            return persistors[id];
            //return new Method3Persistor(new TestRecord2Persistor());
        }


        public void Register(Type t, IPersistor singleton)
        {
            persistors.Add(t, singleton);
        }
    }
}
