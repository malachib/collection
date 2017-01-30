using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public interface ISerializable
    {
        void Serialize(ISetter output, object context);
        void Deserialize(IGetter input, object context);
    }
}
