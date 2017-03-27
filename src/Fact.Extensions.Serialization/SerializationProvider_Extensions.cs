using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public static class SerializationProvider_Extensions
    {
        public static void Serialize<T, TOut>(this ISerializerProvider sc, TOut context, T instance)
        {
            var s = sc.GetSerializer<TOut>(typeof(T));
            s.Serialize(context, instance);
        }


        public static T Deserialize<T, TIn>(this IDeserializerProvider sc, TIn context)
        {
            var ds = sc.GetDeserializer<TIn>(typeof(T));
            return (T)ds.Deserialize(context, typeof(T));
        }


        public static void Register<TIn, TOut>(this ISerializationRegistrar sc, ISerializerFactoryContainer<TIn, TOut> persistor)
        {
            sc.Register(persistor.SerializerFactory);
            sc.Register(persistor.DeserializerFactory);
        }


        public static AggregateSerializerFactoryContainer<TIn, TOut> RegisterAggregate<TIn, TOut>(this ISerializationRegistrar sp)
        {
            var p = new AggregateSerializerFactoryContainer<TIn, TOut>();
            sp.Register(p);
            return p;
        }


        public static AggregateSerializerFactoryContainer<IPropertyDeserializer, IPropertySerializer> RegisterPropertySerializerAggregate(this ISerializationRegistrar sp)
        {
            return sp.RegisterAggregate<IPropertyDeserializer, IPropertySerializer>();
        }


        /// <summary>
        /// Configures Type, Serializable and Reflection serializers for this provider
        /// specifically using the IPropertySerializer / IPropertyDeserializer transport
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="configureTypeSerializerFactory"></param>
        public static void UsePropertySerializer(this ISerializationRegistrar sp,
            SerializationProviderPropertySerializerConfig config = null)
        {
            var p = sp.RegisterPropertySerializerAggregate();

            p.Configure(config?.ConfigureTypeSerializerFactory);
        }


        /// <summary>
        /// Adds stock standard:
        /// 
        /// TypeSerializerFactory
        /// SerializableSerializerFactory
        /// FieldReflectionSerializerFactory
        /// 
        /// In that order
        /// </summary>
        /// <param name="p"></param>
        /// <param name="configureTypeSerializerFactory"></param>
        public static void Configure(this AggregateSerializerFactoryContainer<IPropertyDeserializer, IPropertySerializer> p,
            Action<TypeSerializerFactory<IPropertyDeserializer, IPropertySerializer>> configureTypeSerializerFactory)
        {
            var tsf = new TypeSerializerFactory<IPropertyDeserializer, IPropertySerializer>();

            configureTypeSerializerFactory?.Invoke(tsf);

            p.Add(tsf);
            p.AddSerializable();
            p.AddFieldReflection();
        }


        /// <summary>
        /// EXPERIMENTAL
        /// retrieve a strongly-typed-wrapped serialization provider based on the specified serialization provider
        /// </summary>
        /// <param name="sp"></param>
        public static WrapperSerializationProvider<IPropertyDeserializer, IPropertySerializer>
            GetPropertySerializationProvider(this ISerializationProvider sp)
        {
            var _sp = (SerializationProvider)sp;
            return new WrapperSerializationProvider<IPropertyDeserializer, IPropertySerializer>(_sp);
        }
    }
}
