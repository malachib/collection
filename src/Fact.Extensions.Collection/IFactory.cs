using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Temporarily keeping these here until we merge back into master branch, then will 
/// move it out to its own project
/// </summary>
namespace Fact.Extensions.Factories
{
    public interface IFactory<T>
    {
        T Create();
    }


    public interface IFactory<TInput, TOutput>
    {
        bool CanCreate(TInput id);

        /// <summary>
        /// Creates an item provided the given input.  Throws an exception if input
        /// is invalid for creation
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns created TOutput - should never return null</returns>
        TOutput Create(TInput id);
    }


    /// <summary>
    /// Meta provides an optimization cache area, since often the CanCreate does a lookup operation
    /// of some kind which then the create may have to repeat
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IFactoryWithMeta<TInput, TOutput> : IFactory<TInput, TOutput>
    {
        bool CanCreate(TInput id, out object meta);

        TOutput Create(TInput id, object meta);
    }
}
