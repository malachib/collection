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
            public override TestNode RootNode => throw new NotImplementedException();

            protected override TestNode CreateNode(TestNode parent, SyncKey key) =>
                new TestNode(key);

            protected override IEnumerable<SyncKey> Split(SyncKey key)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void NodeAttributeTest()
        {

        }
    }
}
