using Fact.Extensions.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Serialization
{
    public interface INodeSerializer
    {
        /// <summary>
        /// Creates a node/container and moves into it for all properties subsequently 
        /// to be set under
        /// </summary>
        void StartNode(object key, object[] attributes);
        /// <summary>
        /// Finishes a node/container and moves out of it, effectively moving the taxonomy
        /// up one level
        /// </summary>
        void EndNode();
    }

    public interface IPropertySerializer : ISetter, INodeSerializer
    {
    }


    public interface INodeDeserializer
    {
        /// <summary>
        /// Expects a node/container at this level and retrieves any key/attributes if
        /// pertinent
        /// </summary>
        /// <param name="key"></param>
        /// <param name="attributes"></param>
        /// <remarks>Exception is thrown in DEBUG mode if start node not found as expected</remarks>
        void StartNode(out object key, out object[] attributes);
        /// <summary>
        /// Expects a node ending here
        /// </summary>
        void EndNode();
    }


    public interface IPropertyDeserializer : IGetter, INodeDeserializer
    {
    }
}
