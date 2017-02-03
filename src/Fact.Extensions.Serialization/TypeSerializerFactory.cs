using Fact.Extensions.Collection;
using Fact.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization.Newtonsoft
{
    /// <summary>
    /// Experimental
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class TypeSerializerFactory<TIn, TOut> :
        SerializerFactory<TIn, TOut>
    {
        readonly LightweightContainer container = new LightweightContainer();

        protected override IDeserializer<TIn> GetDeserializer(Type id)
        {
            // FIX: implement proper CanResolve code here and in parent
            try
            {
                return container.Resolve<IDeserializer<TIn>>(id.Name);
            }
            catch
            {
                return null;
            }
        }

        protected override ISerializer<TOut> GetSerializer(Type id)
        {
            try
            {
                return container.Resolve<ISerializer<TOut>>(id.Name);
            }
            catch
            {
                return null;
            }
        }

        public void RegisterSerializer(ISerializer<TOut> serializer, Type key)
        {
            container.Register(serializer, key.Name);
        }


        public void RegisterDeserializer(IDeserializer<TIn> deserializer, Type key)
        {
            container.Register(deserializer, key.Name);
        }


        public void Register<T>(T t, Type key)
            where T: IDeserializer<TIn>, ISerializer<TOut>
        {
            RegisterSerializer(t, key);
            RegisterDeserializer(t, key);
        }
    }
}
