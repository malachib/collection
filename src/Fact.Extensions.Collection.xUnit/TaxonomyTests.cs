using System;
using System.Collections.Generic;
using Xunit;

namespace Fact.Extensions.Collection.xUnit
{
    public class TaxonomyTests
    {
        internal class TestNode : KeyedChildCollectionBase<int, TestNode>,
            IChild<TestNode>,
            IKeyed<int>
        {
            readonly TestNode parent;
            public int Key { get; }

            protected override int GetKey(TestNode node) => node.Key;

            internal TestNode(TestNode parent, int key)
            {
                this.parent = parent;
                Key = key;
            }

            public TestNode Parent => parent;
        }

        internal class TestKeyedTaxonomy : TaxonomyBase<int, TestNode>
        {
            readonly TestNode rootNode = new TestNode(null, 0);

            protected override TestNode CreateNode(TestNode parent, int key)
            {
                return new TestNode(parent, key);
            }

            public override TestNode RootNode => rootNode;

            protected override IEnumerable<int> Split(int key)
            {
                while(key > 0)
                {
                    int split = key & 0xFF;
                    yield return split;
                }    
            }
        }


        [Fact]
        public void CreateKeyedTaxonomyNodeTest()
        {
            var t = new TestKeyedTaxonomy();
            var child = new TestNode(null, 0x1);

            t.RootNode.AddChild(child);

            // FIX: This gives us an out of memory error.  The key splitting is kinda wrong, but even if so
            // I don't think we should be looping back and creating new nodes over and over
            //TestNode _child = t[0x00_01];
        }
    }
}
