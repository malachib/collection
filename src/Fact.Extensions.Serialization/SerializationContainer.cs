using Fact.Extensions.Collection;
using Fact.Extensions.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class SerializationContainer
    {
        public interface ISerializer<T> { }

        public interface IDeserializer<T> { }

        readonly IServiceProvider serviceProvider;

        public SerializationContainer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
    }


    public class SerializationContainer<TContext> : SerializationContainer
    {
        public SerializationContainer(IServiceProvider serviceProvider) :
            base(serviceProvider)
        { }
    }

    public class SerializerFactory<T, TOut> : IFactory<T, ISerializer<TOut>>
    {
        LightweightContainer container = new LightweightContainer();

        public bool CanCreate(T id)
        {
            return false;
        }

        public ISerializer<TOut> Create(T id)
        {
            return null;
        }
    }
}
