using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// TODO: Still need to lift from old serialization-exp branch
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class RefSerializer<TIn, TOut> : ISerializationManager<TIn, TOut>
    {
        public object Deserialize(TIn input, Type type)
        {
            throw new NotImplementedException();
        }

        public void Serialize(TOut output, object inputValue, Type type = null)
        {
            throw new NotImplementedException();
        }
    }
}
