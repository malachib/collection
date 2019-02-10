using System;
using Fact.Extensions.Collection;

namespace Fact.Extensions.Experimental
{
    public class AbstractFileProvider<TAccessor> where TAccessor: INamedAccessor<object>
    {
        TAccessor files;

        public AbstractFileProvider()
        {
        }
    }
}
