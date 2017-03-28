using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Configuration
{
    /// <summary>
    /// For use in 2 major scenarios:
    /// 
    /// a) when an OR entity strongly-typed name is one thing but the underlying sql column name is another, use alias to denote actual SQL column name
    /// b) Maps a configuration element's property to a slightly differently named underlying app.config setting
    /// </summary>
    public class AliasAttribute : Attribute//, INamed
    {
        public AliasAttribute(string name) { Name = name; }

        public string Name
        {
            get;
            set;
        }
    }
}
