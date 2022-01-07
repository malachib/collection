using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Experimental
{
    using Collection;
    using System.ComponentModel;
    using System.Linq;

    public class SyncDataProviderTracker
    {
        public int Version { get; private set; }

        public enum Diffs
        {
            Added,
            Removed,
            Updated
        }

        public class Diff
        {
            public SyncDataProviderNode Node { get; }
            public Diffs Command { get; }
            public object Value { get; }
            public object Baseline { get; }

            public Diff(SyncDataProviderNode node, Diffs command, object value, object baseline)
            {
                Node = node;
                Command = command;
                Value = value;
                Baseline = baseline;
            }
        }

        LinkedList<Diff> diffs = new LinkedList<Diff>();

        public IEnumerable<Diff> DiffList => diffs;

        public void Clear() => diffs.Clear();

        void _Update(SyncDataProviderNode node, object value, object baseline)
        {
            if(baseline != value)
            {
                Diff d;

                if(value == null)
                {
                    d = new Diff(node, Diffs.Removed, value, baseline);
                }
                else if(baseline == null)
                {
                    d = new Diff(node, Diffs.Added, value, baseline);
                }
                else
                {
                    d = new Diff(node, Diffs.Updated, value, baseline);
                }

                diffs.AddLast(d);
            }
        }

        public void Update(SyncDataProviderNode node, object value, object baseline)
        {
            /*
            var d = diffs.First;

            {
                d = d.Next;
            }
            while (d.Value.Node != node && d.Next != null) ; */

            for(var d = diffs.First; d != null; d = d.Next)
            {
                if(d.Value.Node == node)
                {
                    diffs.Remove(d);
                    // Carry forward old baseline, thus retaining it
                    _Update(node, value, d.Value.Baseline);
                    return;
                }
            }

            _Update(node, value, baseline);
        }
    }

    public class SyncDataProviderNode : NamedChildCollection<SyncDataProviderNode>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Need use cases for:
        /// 
        /// 1.  This particular object was replaced
        /// 2.  A child was replaced
        /// 3.  Some descendent of the child was replaced
        /// </remarks>
        public event Action<SyncDataProviderNode> Updated;

        readonly Func<object> getter;

        object value;

        void Update()
        {
            value = getter?.Invoke();

            // FIX: Need to do a -= as well
            // DEBT: Likely this can be consolidated with our State object
            if(value is INotifyPropertyChanged nfc)
            {
                nfc.PropertyChanged += Nfc_PropertyChanged;
            }
        }

        private void Nfc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SyncDataProviderNode child = GetChild(e.PropertyName);
        }

        public object Value => value;

        public SyncDataProviderNode(string name, Func<object> getter) : base(name)
        {
            this.getter = getter;
        }
    }

    public class SyncDataProvider : TaxonomyBase<SyncDataProviderNode>
    {
        public override SyncDataProviderNode RootNode { get; }

        public SyncDataProvider(SyncDataProviderNode root) => RootNode = root;

        protected override SyncDataProviderNode CreateNode(SyncDataProviderNode parent, string name)
        {
            // Only used when node isn't found -- so for this case, it really will be an exception
            // DEBT: That could use some cleanup somehow
            throw new NotImplementedException();
        }
    }
}
