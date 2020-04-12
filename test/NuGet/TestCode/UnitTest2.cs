#if TEST_DIAGNOSTIC
#define TEST_ENABLED
#endif

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace TestCode
{
#if TEST_ENABLED
    using Fact.Extensions.Collection;
    using FluentAssertions;
#endif

    public class UnitTest2
    {
#if TEST_ENABLED
        public class Node : INamed
        {
            public string Name { get; set; }
        }
#endif

        [Fact]
        public void Create_NamedChildCollection()
        {
#if TEST_ENABLED
            var childCollection = new NamedChildCollection<Node>("root");

            childCollection.AddChild(new Node() { Name = "Node 1" });

            var children = childCollection.Children;

            children.Should().ContainSingle();
#endif
        }
    }
}
