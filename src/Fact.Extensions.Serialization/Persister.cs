using System;
using System.Collections.Generic;
using System.Reflection;
#if NETSTANDARD1_6
using System.Linq;
#endif
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public class Persistor
    {
        public ModeEnum Mode { get; set; }
        public enum ModeEnum
        {
            /// <summary>
            /// Move from memory representation to outside data destination
            /// </summary>
            Serialize,
            /// <summary>
            /// Move from outside data source to internal memory representation
            /// </summary>
            Deserialize
        }
    }


#if NETSTANDARD1_6
#endif

    public class PersistAttribute : Attribute
    {

    }
}
