using System;
using Xunit;
using FluentAssertions;
using System.ComponentModel;
using System.Linq;

namespace Fact.Extensions.Experimental.xUnit
{
    public class SyncDataProviderTests
    {
        [Fact]
        public void Test1()
        {
            var node = new SyncDataProviderNode("root", null);
            var sdp = new SyncDataProvider(node);
            var entity = new TestEntity1();

            node.AddChild(new SyncDataProviderNode("test1", () => entity));
        }


        [Fact]
        public void TrackerUpdatedTest()
        {
            var t = new SyncDataProviderTracker();
            var node1 = new SyncDataProviderNode("root", null);
            var node2 = new SyncDataProviderNode("test1", null);

            node1.AddChild(node2);

            t.Update(node1, "updated1.1", "initial");
            t.Update(node1, "updated1.2", "updated1.1");
            t.Update(node1, "updated1.3", "updated1.2");

            var dl = t.DiffList.ToArray();

            dl.Should().HaveCount(1);
            dl[0].Command.Should().Be(SyncDataProviderTracker.Diffs.Updated);
        }


        [Fact]
        public void TrackerAddedTest()
        {
            var t = new SyncDataProviderTracker();
            var node1 = new SyncDataProviderNode("root", null);
            var node2 = new SyncDataProviderNode("test1", null);

            node1.AddChild(node2);

            t.Update(node1, "updated1.1", null);
            t.Update(node1, "updated1.2", "updated1.1");
            t.Update(node1, "updated1.3", "updated1.2");

            var dl = t.DiffList.ToArray();

            dl.Should().HaveCount(1);
            dl[0].Command.Should().Be(SyncDataProviderTracker.Diffs.Added);

            t.Clear();

            // Test extensions, which we may elect to not use in the future
            t.Update(node1, "updated1.1");
            t.Update(node1, "updated1.2");
            t.Update(node1, "updated1.3");

            dl = t.DiffList.ToArray();

            dl.Should().HaveCount(1);
            dl[0].Command.Should().Be(SyncDataProviderTracker.Diffs.Added);
        }


        [Fact]
        public void TrackerRemovedTest()
        {
            var t = new SyncDataProviderTracker();
            var node1 = new SyncDataProviderNode("root", null);
            var node2 = new SyncDataProviderNode("test1", null);

            node1.AddChild(node2);

            t.Update(node1, "updated1.1", "baseline");
            t.Update(node1, "updated1.2", "updated1.1");
            t.Update(node1, null, "updated1.2");

            var dl = t.DiffList.ToArray();

            dl.Should().HaveCount(1);
            dl[0].Command.Should().Be(SyncDataProviderTracker.Diffs.Removed);

            t.Update(node1, "updated1.1", "baseline");
            t.Update(node1, "updated1.2");
            t.Update(node1, null);

            dl = t.DiffList.ToArray();

            dl.Should().HaveCount(1);
            dl[0].Command.Should().Be(SyncDataProviderTracker.Diffs.Removed);

        }


        [Fact]
        public void TrackerUnchangedTest()
        {
            var t = new SyncDataProviderTracker();
            var node1 = new SyncDataProviderNode("root", null);
            var node2 = new SyncDataProviderNode("test1", null);

            node1.AddChild(node2);

            t.Update(node1, "updated1.1", null);
            t.Update(node1, "updated1.2", "updated1.1");
            t.Update(node1, "updated1.3", "updated1.2");
            t.Update(node1, null, "updated1.3");

            var dl = t.DiffList.ToArray();

            dl.Should().BeEmpty();

            t.Clear();

            t.Update(node1, "updated1.1");
            t.Update(node1, "updated1.2");
            t.Update(node1, "updated1.3");
            t.Update(node1, null);

            dl = t.DiffList.ToArray();

            dl.Should().BeEmpty();
        }


        [Fact]
        public void TrackerMixedTest()
        {
            var t = new SyncDataProviderTracker();
            var node1 = new SyncDataProviderNode("root", null);
            var node2 = new SyncDataProviderNode("test1", null);

            node1.AddChild(node2);

            t.Update(node1, "updated1.1", null);
            t.Update(node2, "updated2.1", "baseline2.0");

            var dl = t.DiffList.ToArray();

            dl.Should().HaveCount(2);

            t.Update(node1, null, "updated1.1");
            t.Update(node2, "updated2.2", "updated2.1");

            dl = t.DiffList.ToArray();

            dl.Should().HaveCount(1);
        }
    }

    public class TestEntity1 : 
        INotifyPropertyChanged,
        INotifyPropertyChanging
    {
        public string Value1 { get; set; }
        public int Value2 { get; set; }

        public TestEntity1 Nested { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
    }
}
