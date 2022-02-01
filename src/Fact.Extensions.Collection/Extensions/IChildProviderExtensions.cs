using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fact.Extensions.Collection
{
    public static class IChildProviderExtensions
    {
        /// <param name="startNode">top of tree to search from.  MUST be convertible to type T directly</param>
        /// <param name="splitKeys">broken out path/key components</param>
        public static T FindChildByPath<T>(this IChildProvider<T> startNode, IEnumerable<string> splitPaths,
            Func<T, string, T> nodeFactory = null)
            where T: INamed
        {
            return startNode.FindChildByPath(splitPaths, nodeFactory, (node, key) => node.Name.Equals(key));
        }

        public static T FindChildByPath<T, TKey>(T node, IEnumerable<TKey> splitKeys,
            Func<T, TKey, T> nodeFactory, Func<T, TKey, bool> keyPredicate,
            Func<T, IChildProvider<T>> getChildProverFromNode)
        {
            var _startNode = getChildProverFromNode(node);

            return _startNode.FindChildByPath(splitKeys, nodeFactory, keyPredicate, node, getChildProverFromNode);
        }

        /// <summary>
        /// Stock standard tree traversal, adapted to a sequence of keys representing path
        /// </summary>
        /// <param name="startNode">top of tree to search from.  MUST be convertible to type T directly</param>
        /// <param name="splitKeys">broken out path/key components</param>
        /// <param name="nodeFactory"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public static T FindChildByPath<T, TKey>(this IChildProvider<T> startNode, IEnumerable<TKey> splitKeys,
            Func<T, TKey, T> nodeFactory, Func<T, TKey, bool> keyPredicate, 
            T node = default(T), Func<T, IChildProvider<T>> getChildProviderFromNode = null)
        {
            IChildProvider<T> currentNode = startNode;

            if(node == null)
                // The ChildProvider must also be a type of T for this to work
                node = (T)currentNode;

            foreach (var key in splitKeys)
            {
                // We may encounter some nodes which are not child provider nodes
                if (currentNode == null) continue;

                if (currentNode is IChildProvider<TKey, T> currentGetChildNode)
                {
                    node = currentGetChildNode.GetChild(key);
                }
                else
                {
                    node = currentNode.Children.SingleOrDefault(x => keyPredicate(x, key));
                }

                if (node == null)
                {
                    // If no way to create a new node, then we basically abort (node not found)
                    if (nodeFactory == null) return default(T);

                    // If we do have a node factory, attempt to auto add *IF* currentNode is writable
                    if (currentNode is IChildCollection<T> currentWritableNode)
                    {
                        // TODO: have a configuration flag to determine auto add
                        // FIX: typecast to (T) fragile
                        node = nodeFactory((T)currentNode, key);
                        currentWritableNode.AddChild(node);
                    }
                    else
                        return default(T);
                }

                currentNode = getChildProviderFromNode == null ? (node as IChildProvider<T>) : getChildProviderFromNode(node);
            }

            return node;
        }


        /*
        public static KeyValuePair<TKey, TNode> FindChildByPath3<TKey, TNode>(this KeyValuePair<TKey, TNode> parent, 
            IEnumerable<TKey> splitKeys,
            Func<TNode, TKey, KeyValuePair<TKey, TNode>> nodeFactory = null)
        {
            Func<KeyValuePair<TKey, TNode>, TKey, bool> keyPredicate = (KeyValuePair<TKey, TNode> n, TKey key) => n.Key.Equals(key);
            Func<TNode, IChildProvider<KeyValuePair<TKey, TNode>>> getChildProviderFromNode = n => n as IChildProvider<KeyValuePair<TKey, TNode>>;
            Func<KeyValuePair<TKey, TNode>, TNode> getNodeFromChild = kvp => kvp.Value;

            return FindChildByPath2<KeyValuePair<TKey, TNode>, TKey, TNode>(parent.Value, 
                splitKeys, 
                keyPredicate,
                getChildProviderFromNode,
                getNodeFromChild);
        }
        */

        /// <summary>
        /// EXPERIMENTAL
        /// </summary>
        /// <param name="keyPredicate">Compares key of evaluating node against </param>
        /// <param name="nodeFactory">Create a new node with first parameter being parent, 2nd being key of new node</param>
        /// <typeparam name="TChild">Top level node/child which reflects key in it</typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TNode">Low level node/child which may or may not have key in it</typeparam>
        /// <returns></returns>
        public static TNode FindChildByPath2<TChild, TKey, TNode>(TNode node,
            IEnumerable<TKey> splitKeys,
            Func<TChild, TKey, bool> keyPredicate,
            Func<TNode, IChildProvider<TChild>> getChildProviderFromNode,
            Func<TChild, TNode> getNodeFromChild,
            Func<TNode, TKey, TChild> nodeFactory = null)
        {
            // dig through the key navigation
            
            foreach (var key in splitKeys)
            {
                TNode parentNode = node;

                // convert to IChildProvider, if necessary and possible
                IChildProvider<TChild> currentChildProvider = getChildProviderFromNode(node);

                // We may encounter some nodes which are not child provider nodes.  In this case,
                // skip to next one
                if (currentChildProvider == null) continue;
                // otherwise, see if we are the key-aware child node provider
                else if (currentChildProvider is IChildProvider<TKey, TChild> currentGetChildNode)
                {
                    // grab child node
                    TChild child = currentGetChildNode.GetChild(key);
                    node = getNodeFromChild(child);
                }
                else
                {
                    // otherwise, use key predicate to determine if we fit
                    TChild child = currentChildProvider.Children.SingleOrDefault(x => keyPredicate(x, key));
                    node = getNodeFromChild(child);
                }

                // if no version of child acquisition matched, then see if we can create an empty node
                if (node == null)
                {
                    // If we can, do so
                    if(nodeFactory != null)
                    {
                        // evalute if we can add a brand new empty node to something
                        if(currentChildProvider is IChildCollection<TChild> currentChildCollection)
                        {
                            // if so, create brand new node
                            TChild child = nodeFactory(parentNode, key);

                            // add it
                            currentChildCollection.AddChild(child);
                        }
                        else
                            // currentChildProvider can't support children, so no auto add gonna happen here - we're done
                            return default(TNode);
                    }
                    else
                        // no child factory, so can't auto add anything - we're done
                        return default(TNode);
                }
            }

            return node;
        }


        /// <summary>
        /// Implements visitor pattern over the specified node/child provider
        /// Specifically will dig through entire chain, activating visitor callback for each node
        /// Also carries a simplistic context which is created new for each node so that
        /// sibling nodes may share information
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="node"></param>
        /// <param name="visitor"></param>
        /// <param name="context"></param>
        /// <param name="level"></param>
        public static void Visit<TNode, TContext>(this TNode node, Action<TNode, TContext> visitor, TContext context, int level = 0)
            where TNode : IChildProvider<TNode>
            where TContext : new()
        {
            visitor(node, context);

            foreach (TNode childNode in node.Children)
            {
                context = new TContext();

                Visit(childNode, visitor, context, level + 1);
            }
        }


        /// <summary>
        /// Implements visitor pattern over the specified node/child provider
        /// Specifically will dig through entire chain, activating visitor callback for each node
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="visitor"></param>
        /// <param name="level"></param>
        public static void Visit<TNode>(this TNode node, Action<TNode> visitor, int level = 0)
            where TNode : IChildProvider<TNode>
        {
            visitor(node);

            foreach (TNode childNode in node.Children)
            {
                Visit(childNode, visitor, level + 1);
            }
        }
    }
}
