﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public class LightweightContainer //: Fact.Apprentice.Collection.IFactory<Type, object>
    {
        internal struct Item
        {
            internal string key;
            internal object value;
        }

        Dictionary<Type, LinkedList<Item>> registered;

        /// <summary>
        /// Be aware this is an internal accessor, and as such sometimes registered will be NULL
        /// </summary>
        [Obsolete]
        internal IEnumerable<KeyValuePair<Type, LinkedList<Item>>> Registrations
        {
            get { return registered; }
        }


        public IEnumerable<Type> RegisteredTypes
        {
            get { return registered == null ? Enumerable.Empty<Type>() : registered.Keys; }
        }

        public T Resolve<T>(string key = null)
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type, string key = null)
        {
            if (registered == null)
                throw new KeyNotFoundException("No items registered");

            var lookup = registered[type];
            if (key == null)
                return lookup.First.Value.value;
            else
                return lookup.First(x => x.key == key).value;
        }

        /// <summary>
        /// Attempts to resolve the most recently registered item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool TryResolve<T>(out T value, string key = null)
        {
            object _value;
            if (TryResolve(typeof(T), out _value, key))
            {
                value = (T)_value;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public bool TryResolve(Type type, out object value, string key = null)
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


    public static class LightweightContainer_Extensions
    {
        /// <summary>
        /// Registers a particular singleton instance of a type with provided key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="key"></param>
        public static void Register<T>(this LightweightContainer container, T value, string key = null)
        {
            container.Register(typeof(T), value, key);
        }

        public static bool TryResolve<T>(this LightweightContainer container, string key, out T value)
        {
            object output;
            if(container.TryResolve(typeof(T), out output, key))
            {
                value = (T)output;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
    }
}