using System;
using Xunit;
using FluentAssertions;
using System.ComponentModel;
using System.Linq;
using Fact.Extensions.Collection;

namespace Fact.Extensions.Experimental.xUnit
{
    public class SyncDataProviderTests
    {
        [Fact]
        public void Test1()
        {
            var root = new SyncDataProviderNode("root", null);
            var sdp = new SyncDataProvider(root);
            var entity = new TestEntity1();
            var child = new SyncDataProviderNode("test1", () => entity);
            var counter = 0;

            root.AddChild(child);

            entity.PropertyChanging += (sender, e) =>
            {
                ++counter;

                switch(e.PropertyName)
                {
                    case nameof(TestEntity1.Value1):
                        entity.Value1.Should().Be(null);
                        break;

                    case nameof(TestEntity1.Value2):
                        entity.Value2.Should().Be(0);
                        break;
                }
            };

            entity.PropertyChanged += (sender, e) =>
            {
                ++counter;

                switch (e.PropertyName)
                {
                    case nameof(TestEntity1.Value1):
                        entity.Value1.Should().Be("hi2u");
                        break;

                    case nameof(TestEntity1.Value2):
                        entity.Value2.Should().Be(5);
                        break;
                }
            };

            entity.Value1 = "hi2u";
            entity.Value2 = 5;

            counter.Should().Be(4);
        }

        [Fact]
        public void Test2()
        {
            var root = new SyncDataProviderNode("root", null);
            var sdp = new SyncDataProvider(root);
            var entity = new TestEntity1();

            var entityNode = root.AddEntity("test1", entity);

            int counter = 0;

            // DEBT: Might want periods in this context at some point rather than slashes
            // DEBT: NOT specifying root node name a little confusing
            //var node = sdp["root/test1/Value1"];
            var value2Node = sdp["test1/Value2"];

            entityNode.Updated += (n, oldValue, newValue) =>
            {

            };

            value2Node.Updated += (n, oldValue, newValue) =>
            {
                ++counter;

                oldValue.Should().Be(0);
                newValue.Should().Be(7);
            };

            entity.Value2 = 7;

            counter.Should().Be(1);
        }


        [Fact]
        public void TrackerUpdatedTest()
        {
            var t = new SyncDataProviderTracker();
            var node1 = "root";

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
            var node1 = "root";

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
            var node1 = "root";

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
            var node1 = "root";

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
            var node1 = "root";
            var node2 = "test1";

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


    public class ChangeEventHelperBase :
        INotifyPropertyChanged,
        INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        protected void Changed(object sender, string propertyName) =>
            PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));

        protected void Changing(object sender, string propertyName) =>
            PropertyChanging(sender, new PropertyChangingEventArgs(propertyName));
    }

    public class ChangeEventHelper : ChangeEventHelperBase
    {
        public new void Changed(object sender, string propertyName) =>
            base.Changed(sender, propertyName);

        public new void Changing(object sender, string propertyName) =>
            base.Changing(sender, propertyName);

        public void Init<T>(ref State<T> s, object sender, string propertyName)
        {
            s.Changed += delegate { Changed(sender, propertyName); };
            s.Changing += delegate { Changing(sender, propertyName); };
        }

        public void Init(ref PropertyChangingEventHandler changing, ref PropertyChangedEventHandler changed)
        {
            //PropertyChanged += (sender, e) => changed?.Invoke(sender, e);
            //PropertyChanging += (sender, e) => changing?.Invoke(sender, e);
            PropertyChanged += ChangeEventHelper_PropertyChanged;
        }

        private void ChangeEventHelper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }
    }

    public class TestEntity1 : 
        INotifyPropertyChanged,
        INotifyPropertyChanging
    {
        ChangeEventHelper ceh = new ChangeEventHelper();

        State<string> value1;
        State<int> value2;

        public string Value1
        {
            get => value1.Value;
            set => value1.Value = value;
        }

        public int Value2
        {
            get => value2;
            set => value2.Value = value;
        }

        public TestEntity1 Nested { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public TestEntity1()
        {
            //ceh.Init(PropertyChanging, PropertyChanged);

            ceh.PropertyChanged += (sender, e) => PropertyChanged?.Invoke(sender, e);
            ceh.PropertyChanging += (sender, e) => PropertyChanging?.Invoke(sender, e);

            value1.Changed += delegate { ceh.Changed(this, nameof(Value1)); };
            value1.Changing += delegate { ceh.Changing(this, nameof(Value1)); };

            ceh.Init(ref value2, this, nameof(Value2));
        }
    }
}
