using FluentAssertions;
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

            /// <summary>
            /// Not really used
            /// </summary>
            public TestNode Parent => parent;
        }

        internal class TestKeyedTaxonomy : TaxonomyBase<int, TestNode>
        {
            readonly TestNode rootNode = new TestNode(null, -1);

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
                    key >>= 8;
                    yield return split;
                }    
            }
        }


        [Fact]
        public void CreateKeyedTaxonomyNodeTest()
        {
            var t = new TestKeyedTaxonomy();
            var child = new TestNode(t.RootNode, 0);
            var grandChild = new TestNode(child, 1);

            t.RootNode.AddChild(child);
            child.AddChild(grandChild);

            // For this synthetic scenario, parent is the rightmost

            TestNode _child = t[0x01_00];

            _child.Should().Be(grandChild);
        }
    }
}
