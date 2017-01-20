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
    ///   1.  raw-field-based via reflection + PersistAttribute
    ///   2.  classic ISerializable-style interface with explicit save/restore via 
    ///       IPropertySerializer/IPropertyDeserializer similar to old SerializerInfo
    ///   3.  reflection based "Persist(ref T val1, ref T val2)"
    /// </summary>
    /// <remarks>
    /// SerializationInfo and friends feel so in-flux / detested that I am going to re-implement
    /// some of its functionality my own way.  I personally liked SerializationInfo, but utilizing
    /// it at this point is fighting a trend vs rolling my own
    /// </remarks>
    public interface IPersistor
    {
        void Persist(object instance);
    }
}
