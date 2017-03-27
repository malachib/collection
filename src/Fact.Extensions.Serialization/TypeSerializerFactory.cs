using Fact.Extensions.Collection;
using Fact.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fact.Extensions.Factories;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Experimental
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class TypeSerializerFactory<TIn, TOut> :
        ISerializerFactory<TIn, TOut>
    {
        readonly IServiceContainer container;

        public TypeSerializerFactory(IServiceContainer container)
        {
            this.container = container;
        }


        public TypeSerializerFactory() : this(new LightweightContainer())
        {

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

        bool IFactory<Type, ISerializer<TOut>>.CanCreate(Type id)
        {
            return container.CanResolve<ISerializer<TOut>>(id.Name);
        }

        ISerializer<TOut> IFactory<Type, ISerializer<TOut>>.Create(Type id)
        {
            return container.Resolve<ISerializer<TOut>>(id.Name);
        }

        bool IFactory<Type, IDeserializer<TIn>>.CanCreate(Type id)
        {
            return container.CanResolve<IDeserializer<TIn>>(id.Name);
        }

        IDeserializer<TIn> IFactory<Type, IDeserializer<TIn>>.Create(Type id)
        {
            return container.Resolve<IDeserializer<TIn>>(id.Name);
        }
    }


    public static class TypeSerializerFactory_Extensions
    {
        public static void RegisterFieldReflection<T>(this TypeSerializerFactory<IPropertyDeserializer, IPropertySerializer> tsf, 
            Func<T, string> keyFinder)
        {

        }
    }
}
