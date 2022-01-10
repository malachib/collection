using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;
using Xunit;

namespace Fact.Extensions.Experimental.xUnit
{
    using Collection;
    using System.Linq;

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
            var k4 = new SyncKey("grandChild1", ("name", "Fred"));
            var k5 = new SyncKey("grandChild1", ("name", "Bob"));
            var k6 = new SyncKey("grandChild1", ("_name", "N/A"));

            var child1 = new TestNode(k2);
            var grandChild1 = new TestNode(k3);
            var grandChild2 = new TestNode(k4);
            var grandChild3 = new TestNode(k5);
            var grandChild4 = new TestNode(k6);

            t.RootNode.AddChild(child1);

            child1.AddChild(grandChild1);
            child1.AddChild(grandChild2);
            child1.AddChild(grandChild3);
            child1.AddChild(grandChild4);

            // DEBT: Sometimes we need root node here and sometimes we don't.  Close
            // the descrepency and/or document why it exists
            //TestNode node = t[new[] { k1, k2, k3 }];

            TestNode node = t[k2, k3];

            node.Should().Be(grandChild1);

            var grandChildrenWithName = child1.Children.WithAttributeNames("name").ToArray();

            grandChildrenWithName.Length.Should().Be(2);

            node = t[k2, k6];

            node.Should().Be(grandChild4);
        }
    }
}
