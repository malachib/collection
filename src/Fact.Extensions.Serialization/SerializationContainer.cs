using Fact.Extensions.Collection;
using Fact.Extensions.Factories;
using Fact.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class SerializationContainer
    {
        //public interface ISerializer<T> { }

        //public interface IDeserializer<T> { }

        readonly IServiceProvider serviceProvider;

        public SerializationContainer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
    }


    public class SerializationContainer<TIn, TOut> : SerializationContainer
    {
        protected SerializationFactory<ISerializer<TOut>> serializerFactory;
        protected SerializationFactory<IDeserializer<TIn>> deserializerFactory;

        public SerializationContainer(IServiceProvider serviceProvider) :
            base(serviceProvider)
        { }

        public ISerializer<TOut> GetSerializer(Type type) => serializerFactory.Create(type);
        public IDeserializer<TIn> GetDeserializer(Type type) => deserializerFactory.Create(type);

        public void Register<TPersistor>(TPersistor persistor, Type t)
            where TPersistor: ISerializer<TOut>, IDeserializer<TIn>
        {
            // NOTE: We should make one unified container for these registrations, since
            // the TPersistor + key can disambiguate nicely
            serializerFactory.container.Register(typeof(ISerializer<TOut>), persistor, t.Name);
            deserializerFactory.container.Register(typeof(IDeserializer<TIn>), persistor, t.Name);
        }
    }


    public class ExperimentalSerializationContainer : 
        SerializationContainer<IPropertyDeserializer, IPropertySerializer>
    {
        public ExperimentalSerializationContainer(IServiceProvider serviceProvider) :
            base(serviceProvider)
        {
            serializerFactory = new SerializationFactory<ISerializer<IPropertySerializer>>();
            deserializerFactory = new SerializationFactory<IDeserializer<IPropertyDeserializer>>();
        }
    }


    public interface ISerializationContainer
    {
        ISerializer<TOut> GetSerializer<TOut>(Type type);
        IDeserializer<TIn> GetDeserializer<TIn>(Type type);
    }

    public class SerializationContainer2 : ISerializationContainer
    {
        public readonly LightweightContainer container = new LightweightContainer();

        public void Register<TIn, TOut>(ISerializationManager<TIn, TOut> persistor, Type t)
        {
            // NOTE: We should make one unified container for these registrations, since
            // the TPersistor + key can disambiguate nicely
            container.Register(typeof(ISerializer<TOut>), persistor, t.Name);
            container.Register(typeof(IDeserializer<TIn>), persistor, t.Name);
        }

        public ISerializer<TOut> GetSerializer<TOut>(Type type) =>
            container.Resolve<ISerializer<TOut>>(type.Name);

        public IDeserializer<TIn> GetDeserializer<TIn>(Type type) =>
            container.Resolve<IDeserializer<TIn>>(type.Name);
    }



    public static class SerializationContainer2_Extensions
    {
        public static void Serialize<T, TOut>(this ISerializationContainer sc, TOut context, T instance)
        {
            var s = sc.GetSerializer<TOut>(typeof(T));
            s.Serialize(context, instance);
        }


        public static T Deserialize<T, TIn>(this ISerializationContainer sc, TIn context)
        {
            var ds = sc.GetDeserializer<TIn>(typeof(T));
            return (T) ds.Deserialize(context, typeof(T));
        }
    }

    public class SerializationFactory<TOut> : IFactory<Type, TOut>
    {
        internal LightweightContainer container = new LightweightContainer();

        IServiceRegistrar ServiceRegistrar => container;

        public bool CanCreate(Type id)
        {
            object value;
            return container.TryResolve(typeof(TOut), id.Name, out value);
        }

        public TOut Create(Type id)
        {
            return container.Resolve<TOut>(id.Name);
        }
        /*
        public void Register
        {
            container.Register<ISerializer<TOut>>
        }*/
    }
}
