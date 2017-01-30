using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            //if(persistors.ContainsKey(id))
                
                return persistors[id];
            /* saving this code for the aggregated flavors
            else
            {
                if(id.GetTypeInfo().IsAssignableFrom(typeof(ISerializable)))
                {
                    return 
                }
            }*/
        }


        public void Register(Type t, IPersistor singleton)
        {
            persistors.Add(t, singleton);
        }
    }


    public class SerializableInterfacePersistorFactory : IPersistorFactory
    {
        readonly Func<IPropertySerializer> psFactory;
        readonly Func<IPropertyDeserializer> pdsFactory;

        public SerializableInterfacePersistorFactory(Func<IPropertySerializer> psFactory, Func<IPropertyDeserializer> pdsFactory)
        {
            this.psFactory = psFactory;
            this.pdsFactory = pdsFactory;
        }

        public bool CanCreate(Type id)
        {
            return id.GetTypeInfo().IsAssignableFrom(typeof(ISerializable));
        }

        public IPersistor Create(Type id)
        {
            return new PersistorSerializable(psFactory, pdsFactory);
        }

        public void Register(Type t, IPersistor persistor)
        {
            throw new NotImplementedException();
        }
    }
}
