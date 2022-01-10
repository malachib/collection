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

        /// <summary>
        /// Stock standard tree traversal
        /// </summary>
        /// <param name="startNode">top of tree to search from.  MUST be convertible to type T directly</param>
        /// <param name="splitKeys">broken out path/key components</param>
        /// <param name="nodeFactory"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindChildByPath<T, TKey>(this IChildProvider<T> startNode, IEnumerable<TKey> splitKeys,
            Func<T, TKey, T> nodeFactory, Func<T, TKey, bool> keyPredicate)
        {
            IChildProvider<T> currentNode = startNode;

            // The ChildProvider must also be a type of T for this to work
            T node = (T)currentNode;

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

                currentNode = node as IChildProvider<T>;
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
