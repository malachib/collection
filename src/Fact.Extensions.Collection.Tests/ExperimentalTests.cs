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
        internal class TestNode : TaxonomyBase.NodeBase<TestNode>, 
            IChild<TestNode>
        {
            readonly TestNode parent;

            internal TestNode(TestNode parent, string name) : base(name)
            {
                this.parent = parent;
            }

            public TestNode Parent => parent;
        }

        internal class TestTaxonomy : TaxonomyBase<TestNode>
        {
            readonly TestNode rootNode = new TestNode(null, "root");

            protected override TestNode CreateNode(TestNode parent, string name)
            {
                return new TestNode(parent, name);
            }

            public override TestNode RootNode => rootNode;
        }

        static string[] expectedNames = new[] { "root", "child1", "grandchild1", "child2" };

        TestTaxonomy setup()
        {
            var taxonomy = new TestTaxonomy();
            var rootNode = taxonomy.RootNode;
            var child1 = new TestNode(rootNode, "child1");
            rootNode.AddChild(child1);
            child1.AddChild(new TestNode(child1, "grandchild1"));
            rootNode.AddChild(new TestNode(rootNode, "child2"));

            return taxonomy;
        }

        [TestMethod]
        public void VisitorTest()
        {
            TestTaxonomy taxonomy = setup();
            int counter = 0;

            taxonomy.Visit(n =>
            {
                Console.WriteLine($"Visiting: {n.Name}");

                Assert.AreEqual(expectedNames[counter++], n.Name);
            });
        }


        [TestMethod]
        public void FullNameTest()
        {
            TestTaxonomy taxonomy = setup();

            var rootNode = taxonomy.RootNode;
            var grandchild = rootNode.GetChild("child1").GetChild("grandchild1");

            var grandChildName = grandchild.GetFullName();

            Assert.AreEqual("root/child1/grandchild1", grandChildName);

            grandchild = rootNode.GetChild("child1").GetChild("grandchild2");

            Assert.IsNull(grandchild);
        }


        [TestMethod]
        public void CreateTaxonomyNodeTest()
        {
            TestTaxonomy taxonomy = setup();

            var node = taxonomy["child3"];

            Assert.AreEqual("root/child3", node.GetFullName());
        }
    }
}
