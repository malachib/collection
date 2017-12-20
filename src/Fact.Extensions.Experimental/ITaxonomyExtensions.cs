using Fact.Extensions.Collection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Experimental
{
    /// <summary>
    /// 
    /// </summary>
    public static class ITaxonomyExtensions
    {
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


        public static void Visit<TNode>(this TNode node, Action<TNode> visitor, int level = 0)
            where TNode : IChildProvider<TNode>
        {
            visitor(node);

            foreach (TNode childNode in node.Children)
            {
                Visit(childNode, visitor, level + 1);
            }
        }


        public static void Visit<TNode>(this ITaxonomy<TNode> taxonomy, Action<TNode> visitor)
            where TNode : IChildProvider<TNode>, INamed
        {
            Visit(taxonomy.RootNode, visitor);
        }
    }

    /// <summary>
    /// Consider changing to a better name
    /// </summary>
    public class TaxonomyBase
    {
        public class NodeBase<TNode> :
            INamed,
            INamedChildCollection<TNode>
            where TNode : INamed
        {
            SparseDictionary<string, TNode> children;
            readonly string name;

            public string Name => name;

            public IEnumerable<TNode> Children => children.Values;

            public event Action<object, TNode> ChildAdded;

            public NodeBase(string name)
            {
                this.name = name;
            }

            /// <summary>
            /// TODO: Very likely would prefer a null back if no child, not an exception
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public TNode GetChild(string name)
            {
                children.TryGetValue(name, out TNode value);
                return value;
            }

            public void AddChild(TNode node)
            {
                children.Add(node.Name, node);
                ChildAdded?.Invoke(this, node);
            }
        }
    }


    public abstract class TaxonomyBase<TNode> : TaxonomyBase, ITaxonomy<TNode>
        where TNode :
            INamedChildProvider<TNode>,
            INamed
    {
        public abstract TNode RootNode { get; }

        protected virtual TNode CreateNode(TNode parent, string name) { return default(TNode); }

        public event Action<object, TNode> NodeCreated;

        /// <summary>
        /// Helper since cast didn't automatically happen via FindChildByPath
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private TNode _CreateNode(TNode parent, string name)
        {
            var createdNode = CreateNode(parent, name);

            NodeCreated?.Invoke(this, createdNode);

            return createdNode;
        }

        public TNode this[string path]
        {
            get
            {
                string[] splitPaths = path.Split('/');

                return RootNode.FindChildByPath(splitPaths, _CreateNode);
            }
        }
    }


    public static class IChildProviderExtensions
    {
        /// <summary>
        /// Stock standard tree traversal
        /// </summary>
        /// <param name="startNode">top of tree to search from.  MUST be convertible to type T directly</param>
        /// <param name="splitPaths">broken out path components</param>
        /// <param name="nodeFactory"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FindChildByPath<T>(this IChildProvider<T> startNode, IEnumerable<string> splitPaths,
            Func<T, string, T> nodeFactory = null)
            where T : INamed
        {
            IChildProvider<T> currentNode = startNode;

            // The ChildProvider must also be a type of T for this to work
            T node = (T)currentNode;

            foreach (var name in splitPaths)
            {
                // We may encounter some nodes which are not child provider nodes
                if (currentNode == null) continue;

                if(currentNode is IChildProvider<string, T> currentGetChildNode)
                {
                    node = currentGetChildNode.GetChild(name);
                }
                else
                {
                    node = currentNode.Children.SingleOrDefault(x => x.Name.Equals(name));
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
                        node = nodeFactory((T)currentNode, name);
                        currentWritableNode.AddChild(node);
                    }
                    else
                        return default(T);
                }

                currentNode = node as IChildProvider<T>;
            }

            return node;
        }
    }


    public static class IChildExtensions
    {
        static bool DetectNullNameParent<T>(T node)
            where T : INamed, IChild<T>
        {
            return node.Parent.Name == null;
        }

        /// <summary>
        /// String child nodes together to produce something similar to a FQDN
        /// (fully qualified domain name)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="delimiter"></param>
        /// <param name="experimentalAbortProcessor"></param>
        /// <returns></returns>
        public static string GetFullName<T>(this T node, char delimiter = '/',
            Func<T, bool> experimentalAbortProcessor = null)
            where T : INamed, IChild<T>
        {
            var fullName = node.Name;

            while (node.Parent != null)
            {
                if (experimentalAbortProcessor != null && experimentalAbortProcessor(node)) return fullName;

                node = node.Parent;

                // TODO: Ideally this would be more configurable, but will do
                // we skip path building/delimiter concatination if the node has no name
                if (node.Name == null) continue;

                fullName = node.Name + delimiter + fullName;
            }

            return fullName;
        }
    }
}
