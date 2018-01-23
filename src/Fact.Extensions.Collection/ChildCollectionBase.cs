﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Reference implementation of IChildCollection
    /// </summary>
    /// <remarks>
    /// All the IChildProvider stuff is very Dictionary-like, but the paradigm of it being 
    /// children making it look and feel slightly different - even if the functionality is nearly 
    /// identical.  Utilize the ITaxonomy stuff to treat more dictionary like (using IAccessor)
    /// </remarks>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class KeyedChildCollectionBase<TKey, TNode> : IChildCollection<TNode>
    {
        protected abstract TKey GetKey(TNode node);

        SparseDictionary<TKey, TNode> children;

        public IEnumerable<TNode> Children => children.Values;

        /// <summary>
        /// Fired when a child is added to the node child collection.  First parameter is sender
        /// (this node), second paramter is child being added
        /// </summary>
        public event Action<object, TNode> ChildAdded;

        /// <summary>
        /// Fired when a child is removed from this child collection.  First parameter is sender
        /// (this node), second parameter is child being removed
        /// </summary>
        public event Action<object, TNode> ChildRemoved;

        /// <summary>
        /// Returns child if found, otherwise null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TNode GetChild(TKey key)
        {
            children.TryGetValue(key, out TNode value);
            return value;
        }

        public void AddChild(TNode node)
        {
            children.Add(GetKey(node), node);
            ChildAdded?.Invoke(this, node);
        }


        public void RemoveChild(TNode node)
        {
            children.Remove(GetKey(node));
            ChildRemoved?.Invoke(this, node);
        }
    }


    /// <summary>
    /// Reference implementation of INamedChildCollection
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public class NamedChildCollection<TNode> :
            KeyedChildCollectionBase<string, TNode>,
            INamed,
            INamedChildCollection<TNode>
            where TNode : INamed
    {
        protected override string GetKey(TNode node) => node.Name;

        public string Name { get; private set; }

        public NamedChildCollection(string name) => Name = name;
    }
}