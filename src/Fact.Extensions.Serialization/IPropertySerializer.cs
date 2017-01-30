using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public interface IPropertySerializer : ISetter
    {
    }


    public interface IPropertyDeserializer : IGetter
    {
    }
}
