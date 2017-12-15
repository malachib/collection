using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Indicates the implemnting class provides a well-known name
    /// </summary>
    public interface INamed
    {
        string Name { get; }
    }


    public interface IKeyed<TKey>
    {
        TKey Key { get; }
    }
}
