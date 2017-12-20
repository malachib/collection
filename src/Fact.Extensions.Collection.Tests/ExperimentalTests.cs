using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using Fact.Extensions.Experimental;

// TODO: Make a Fact.Extensions.Experimental.Tests project discrete from Fact.Extensions.Collection.Tests
namespace Fact.Extensions.Collection.Tests
{
    [TestClass]
    public class ExperimentalTests
    {
        internal class TestNode : TaxonomyBase.NodeBase<TestNode>
        {
            internal TestNode(string name) : base(name) { }
        }

        internal class TestTaxonomy : TaxonomyBase<TestNode>
        {
            readonly TestNode rootNode = new TestNode("root");

            public override TestNode RootNode => rootNode;
        }

        [TestMethod]
        public void VisitorTest()
        {
            var taxonomy = new TestTaxonomy();
            var expectedNames = new[] { "root", "child1", "grandchild1", "child2" };
            int counter = 0;

            var child1 = new TestNode("child1");
            taxonomy.RootNode.AddChild(child1);
            child1.AddChild(new TestNode("grandchild1"));
            taxonomy.RootNode.AddChild(new TestNode("child2"));

            taxonomy.Visit(n =>
            {
                Console.WriteLine($"Visiting: {n.Name}");

                Assert.AreEqual(expectedNames[counter++], n.Name);
            });
        }
    }
}
