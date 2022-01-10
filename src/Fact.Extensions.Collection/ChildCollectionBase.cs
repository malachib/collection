using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection
{
    public abstract class ChildCollectionBase<TNode> : IChildCollection<TNode>
    {
        /// <summary>
        /// Fired when a child is adding to the node child collection, but before it is added
        /// First parameter is sender (this node), second paramter is child being added
        /// </summary>
        public event Action<object, TNode> ChildAdding;

        /// <summary>
        /// Fired when a child is added to the node child collection.  First parameter is sender
        /// (this node), second paramter is child being added
        /// </summary>
        public event Action<object, TNode> ChildAdded;

        protected abstract void AddChildInternal(TNode node);


        public void AddChild(TNode node)
        {
            ChildAdding?.Invoke(this, node);
            AddChildInternal(node);
            ChildAdded?.Invoke(this, node);
        }

        public abstract IEnumerable<TNode> Children { get; }
    }

    /// <summary>
    /// Reference implementation of IChildCollection
    /// </summary>
    /// <remarks>
    /// All the IChildProvider stuff is very Dictionary-like, but the paradigm of it being 
    /// children making it look and feel slightly different - even if the functionality is nearly 
    /// identical.  Utilize the ITaxonomy stuff to treat more dictionary like (using IAccessor)
    /// 
    /// TODO: Make a async/awaitable and/or MT-safe set of calls for this
    /// </remarks>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class KeyedChildCollectionBase<TKey, TNode> : 
        ChildCollectionBase<TNode>,
        IChildCollection<TKey, TNode>
    {
        /// <summary>
        /// Get the key of a particular node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected abstract TKey GetKey(TNode node);

        SparseDictionary<TKey, TNode> children;

        public override IEnumerable<TNode> Children => children.Values;

        /// <summary>
        /// Fired when a child is about to be removed from collection
        /// First parameter is sender (this node), second paramter is child being added
        /// </summary>
        public event Action<object, TNode> ChildRemoving;

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

        protected override void AddChildInternal(TNode node)
        {
            children.Add(GetKey(node), node);
        }


        public void RemoveChild(TNode node)
        {
            ChildRemoving?.Invoke(this, node);
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
