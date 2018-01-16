using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Fact.Extensions.Configuration;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Lightweight IoC.  All singletons. Not meant to replace Castle or even TinyIoC,
    /// just used in situations where I know the footprint of those two
    /// are too large.  Revisit that notion about TinyIoC, because it really is pretty small
    /// </summary>
    /// <remarks>
    /// TODO: Implement as a Factory 
    /// </remarks>
    public class LightweightContainer : //: Fact.Apprentice.Collection.IFactory<Type, object>
        IServiceContainer
    {
        internal struct Item
        {
            internal string key;
            internal object value;
        }

        Dictionary<Type, LinkedList<Item>> registered;


        public IEnumerable<Type> RegisteredTypes
        {
            get { return registered == null ? Enumerable.Empty<Type>() : registered.Keys; }
        }



        /// <summary>
        /// Attempts to resolve the most recently registered item
        /// </summary>
        public bool TryResolve(Type type, string key, out object value)
        {
            if (registered == null)
            {
                value = null;
                return false;
            }

            LinkedList<Item> lookup;

            if (!registered.TryGetValue(type, out lookup))
            {
                value = null;
                return false;
            }

            if (key == null)
            {
                value = lookup.Last.Value.value;
                return true;
            }

            foreach (var item in lookup.Reverse())
            {
                if (item.key == key)
                {
                    value = item.value;
                    return true;
                }
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Registers a particular singleton instance of a type with provided key
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="key"></param>
        public void Register(Type type, object value, string key = null)
        {
            if (registered == null)
                registered = new Dictionary<Type, LinkedList<Item>>();

            var item = new Item() { key = key, value = value };
            LinkedList<Item> lookup;

            if (!registered.TryGetValue(type, out lookup))
                registered[type] = lookup = new LinkedList<Item>();
            else
            {
                if (lookup.Any(x => x.key == key))
                    throw new IndexOutOfRangeException("An item with that key has already been added: key = " + key);
            }

            lookup.AddLast(item);
        }


        /// <summary>
        /// See if a particular type is registered
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsRegistered(Type type)
        {
            if (registered == null) return false;

            return registered.ContainsKey(type);
        }

        /* 
        bool Apprentice.Collection.IFactory<string, object>.CanCreate(string id)
        {
        }

        object Apprentice.Collection.IFactory<string, object>.Create(string id)
        {
            return registered[id].First().value;
        } */
    }
}
