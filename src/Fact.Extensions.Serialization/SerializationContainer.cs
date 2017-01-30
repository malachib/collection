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
}
