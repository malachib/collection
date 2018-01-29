using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// 
    /// </summary>
    public static class ITaxonomyExtensions
    {
        public static void Visit<TNode>(this ITaxonomy<TNode> taxonomy, Action<TNode> visitor)
            where TNode : IChildProvider<TNode>, INamed
        {
            taxonomy.RootNode.Visit(visitor);
        }
    }
}
