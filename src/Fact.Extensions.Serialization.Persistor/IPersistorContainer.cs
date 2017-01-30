using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    /// <summary>
    /// Experimental
    /// </summary>
    public interface IPersistorContainer
    {
        IServiceProvider ServiceProvider { get; }
    }


    public class PersistorContainer : IPersistorContainer
    {
        readonly IServiceProvider sp;

        public IServiceProvider ServiceProvider => sp;

        public PersistorContainer(IServiceProvider sp)
        {
            this.sp = sp;
        }
    }
}
