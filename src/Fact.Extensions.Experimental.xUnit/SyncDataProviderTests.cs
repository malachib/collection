using System;
using Xunit;
using FluentAssertions;
using System.ComponentModel;

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
    }

    public class TestEntity1 : INotifyPropertyChanged
    {
        public string Value1 { get; set; }
        public int Value2 { get; set; }

        public TestEntity1 Nested { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
