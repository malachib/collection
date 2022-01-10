using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;
using Xunit;

namespace Fact.Extensions.Experimental.xUnit
{
    using Collection;

    public class TaxonomyTests
    {
        public class TestNode : 
            KeyedChildCollectionBase<SyncKey, TestNode>,
            IKeyed<SyncKey>
        {
            public SyncKey Key { get; }

            public TestNode(SyncKey key) => Key = key;

            protected override SyncKey GetKey(TestNode node) => node.Key;
        }

        public class TestAttributedTaxonomy : TaxonomyBase<SyncKey, TestNode>
        {
            public override TestNode RootNode { get; } = new TestNode(new SyncKey("root"));

            protected override TestNode CreateNode(TestNode parent, SyncKey key) =>
                new TestNode(key);
        }

        [Fact]
        public void NodeAttributeTest()
        {
            var t = new TestAttributedTaxonomy();

            var k1 = t.RootNode.Key;
            var k2 = new SyncKey("child1");
            var k3 = new SyncKey("grandChild1");

            var child1 = new TestNode(k2);
            var grandChild1 = new TestNode(k3);

            t.RootNode.AddChild(child1);
            child1.AddChild(grandChild1);

            // DEBT: Sometimes we need root node here and sometimes we don't.  Close
            // the descrepency and/or document why it exists
            //TestNode node = t[new[] { k1, k2, k3 }];

            TestNode node = t[new[] { k2, k3 }];

            node.Should().Be(grandChild1);
        }
    }
}
