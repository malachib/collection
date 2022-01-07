using System;
using System.Collections.Generic;
using System.Text;

namespace Fact.Extensions.Experimental
{
    using Collection;
    using System.ComponentModel;
    using System.Linq;

    public class SyncDataProviderTracker : SyncDataProviderTracker<object>
    {

    }

    public class SyncDataProviderTracker<TKey>
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
            public TKey Key { get; }
            public Diffs Command { get; }
            public object Value { get; }
            public object Baseline { get; }

            public Diff(TKey key, Diffs command, object value, object baseline)
            {
                Key = key;
                Command = command;
                Value = value;
                Baseline = baseline;
            }
        }

        //LinkedList<Diff> diffs = new LinkedList<Diff>();
        Dictionary<TKey, Diff> diffs = new Dictionary<TKey, Diff>();

        public IEnumerable<Diff> DiffList => diffs.Values;

        public void Clear() => diffs.Clear();

        void _Update(TKey key, object value, object baseline)
        {
            if(baseline != value)
            {
                Diff d;

                if(value == null)
                {
                    d = new Diff(key, Diffs.Removed, value, baseline);
                }
                else if(baseline == null)
                {
                    d = new Diff(key, Diffs.Added, value, baseline);
                }
                else
                {
                    d = new Diff(key, Diffs.Updated, value, baseline);
                }

                //diffs.AddLast(d);
                diffs.Add(key, d);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="baseline"></param>
        /// <remarks>
        /// FIX: Likely is better to have baseline specified first before value
        /// </remarks>
        public void Update(TKey key, object value, object baseline)
        {
            /*
            for(var d = diffs.First; d != null; d = d.Next)
            {
                if(d.Value.Node.Equals(node))
                {
                    diffs.Remove(d);
                    // Carry forward old baseline, thus retaining it
                    _Update(node, value, d.Value.Baseline);
                    return;
                }
            } */
            if(diffs.TryGetValue(key, out Diff d))
            {
                diffs.Remove(key);
                baseline = d.Baseline;
            }

            _Update(key, value, baseline);
        }
    }


    public static class SyncDataProviderTrackerExtensions
    {
        /// <summary>
        /// Updates, comparing baseline to what exists (if anything) in existing tracker
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="sdpt"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <remarks>
        /// Can and should be optimized if this goes into rotation; however, this may be an extreme edge case as
        /// almost always we really want to have the existing value passed in
        /// </remarks>
        public static void Update<TKey>(this SyncDataProviderTracker<TKey> sdpt, TKey key, object value)
        {
            var d = sdpt.DiffList.FirstOrDefault(x => x.Key.Equals(key));

            sdpt.Update(key, value, d?.Baseline);
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
        public event Action<SyncDataProviderNode, object, object> Updated;

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

            var v = child.value;

            Updated?.Invoke(child, v, child.Value);
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
