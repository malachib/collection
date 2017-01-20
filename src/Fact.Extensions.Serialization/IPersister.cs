using System;
using System.Collections.Generic;
//using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Persister takes Serialization to the next level
    /// a) It encapsulates/hides the transport used for serialization
    /// b) it provides one of 3 mechanisms for serialization:
    ///   1.  raw-field-based via SerializationAttribute
    ///   2.  classic ISerializable-style interface with explicit save/restore via 
    ///       IPropertySerializer/IPropertyDeserializer similar to old SerializerInfo
    ///   3.  reflection based "Persist(ref T val1, ref T val2)"
    /// </summary>
    public interface IPersister
    {
        void Persist(object instance);
    }
}
