using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// For inspecting parameters appearing:
    /// As workflow step parameters
    /// As workflow step parameters during validation
    /// </summary>
    /// <remarks>
    /// TODO: Find a good namespace for this one, Fact.Workflows isn't it
    /// </remarks>
    public interface IParameterInfo
    {
        string Name { get; }
        Type ParameterType { get; }
        Attribute[] Attributes { get; }
    }


    /// <summary>
    /// Retrieves the raw, ordered input parameters.  It is recommended that IParameterInfo be
    /// repeatable.  That is, if this is called twice, the same references are returned
    /// </summary>
    public interface IParameterProviderCore : IAccessor<int, IParameterInfo>
    {
    }

    /// <summary>
    /// Provider for IStateAccessors providing details of the parameters they must interact
    /// with
    /// </summary>
    public interface IParameterProvider : IParameterProviderCore
    {
        int Count { get; }

        /// <summary>
        /// Retrieve native parameter info given the parameter name.  Case insensitve.
        /// Returns NULL if parameter not available
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// It is recommende that IParameterInfo reference be repeatable.  That is, if this
        /// is called twice with the same name, the same reference is returned
        /// </remarks>
        IParameterInfo GetParameterByName(string name);
    }
}
