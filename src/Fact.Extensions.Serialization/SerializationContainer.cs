﻿using Fact.Extensions.Collection;
using Fact.Extensions.Factories;
using Fact.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

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


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// TODO: Either reimplemnt this as IFactory or place in calls to HasSerializer & HasDeserializer
    /// so that aggregation can be done more cleanly
    /// </remarks>
    public interface ISerializationContainer
    {
        ISerializer<TOut> GetSerializer<TOut>(Type type);
        IDeserializer<TIn> GetDeserializer<TIn>(Type type);
    }


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class IPersistor<TIn, TOut>
    {
        IFactory<Type, IDeserializable<TIn>> DeserializerFactory { get; }
        IFactory<Type, ISerializer<TOut>> SerializerFactory { get; }
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


    /// <summary>
    /// EXPERIMENTAL
    /// </summary>
    public class SerializationContainer3 : ISerializationContainer
    {
        IServiceContainer container = new LightweightContainer();

        public ISerializer<TOut> GetSerializer<TOut>(Type type)
        {
            // Find a factory for this type of serializer
            var factory = container.Resolve<IFactory<Type, ISerializer<TOut>>>();
            return factory.Create(type);
        }

        public IDeserializer<TIn> GetDeserializer<TIn>(Type type)
        {
            // Find a factory for this type of deserializer
            var factory = container.Resolve<IFactory<Type, IDeserializer<TIn>>>();
            return factory.Create(type);
        }

        public void Register<TOut>(IFactory<Type, ISerializer<TOut>> factory)
        {
            container.Register(factory);
        }


        public void Register<TIn>(IFactory<Type, IDeserializer<TIn>> factory)
        {
            container.Register(factory);
        }


        public void Register<TIn, TOut>(SerializerFactory<TIn, TOut> factory)
        {
            container.Register<IFactory<Type, ISerializer<TOut>>>(factory);
            container.Register<IFactory<Type, IDeserializer<TIn>>>(factory);
        }
    }


    public class _SerializationContainer : ISerializationContainer
    {
        public readonly SerializationContainer2 registeredContainer = new SerializationContainer2();

        public IDeserializer<TIn> GetDeserializer<TIn>(Type type)
        {
            IDeserializer<TIn> deserializer;

            // First find any specifically registered deserializers
            if (registeredContainer.container.TryResolve(type.Name, out deserializer))
                return deserializer;
            // If none exist, see if this type implements IDeserializable and use our
            // standard approach for that
            //else if (type.GetTypeInfo().IsAssignableFrom(typeof(IDeserializable<TIn>).GetTypeInfo()))
            else if (typeof(IDeserializable<TIn>).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                return new SerializableDeserializer<TIn>();
            }
            else if (typeof(IDeserializer<TIn>) == typeof(IDeserializer<IPropertyDeserializer>))
            {
                // falling back to field-reflction if nothing else works
                // FIX: Very kludgey
                return (IDeserializer<TIn>)new FieldReflectionSerializer();
            }
            else
                throw new InvalidOperationException();
        }

        public ISerializer<TOut> GetSerializer<TOut>(Type type)
        {
            ISerializer<TOut> serializer;

            // First find any specifically registered deserializers
            if (registeredContainer.container.TryResolve(type.Name, out serializer))
                return serializer;
            // If none exist, see if this type implements IDeserializable and use our
            // standard approach for that
            //else if (type.GetTypeInfo().IsAssignableFrom(typeof(ISerializable<TOut>).GetTypeInfo()))
            else if (typeof(ISerializable<TOut>).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                return new SerializableSerializer<TOut>();
            }
            else if (typeof(ISerializer<TOut>) == typeof(ISerializer<IPropertySerializer>))
            {
                // falling back to field-reflction if nothing else works
                // FIX: Very kludgey
                return (ISerializer<TOut>)new FieldReflectionSerializer(x => "TEST");
            }
            else
                throw new InvalidOperationException();
        }
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
