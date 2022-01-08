using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

// FIX: We have an issue where property names will eventually collide with sub-object names.
namespace Fact.Extensions.Experimental
{
    using Collection;

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


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// DEBT: This is a sparse provider, meaning that Children enumeration only lists
    /// what you actual tried to acquire already
    /// DEBT: Decouple this into a general purpose reflection node, then extend it into the INotify aware
    /// </remarks>
    public class SyncDataProviderNode : NamedChildCollection<SyncDataProviderNode>
    {
        readonly IServiceProvider services;

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

        void Updating()
        {
            // TODO: May consider adding an Updating event
        }

        /// <summary>
        /// Internal call to rewrite 'value' from getter and fire updated event
        /// </summary>
        void Update()
        {
            object oldValue = value;
            value = getter?.Invoke();
            Updated?.Invoke(this, oldValue, value);
        }


#if NETSTANDARD1_3_OR_GREATER
        void AddEvents()
        {
            // DEBT: Likely this can be consolidated with our State object
            // DEBT: Likely we'd prefer some kind of IServiceProvider factory arrangement here so that
            // other kinds of notifiers can be used
            if (value is INotifyPropertyChanged nfc)
            {
                nfc.PropertyChanged += Nfc_PropertyChanged;
                var npc2 = (INotifyPropertyChanging)value;
                npc2.PropertyChanging += Npc2_PropertyChanging;
            }
        }


        // DEBT: Not called yet
        void RemoveEvents()
        {
            ((INotifyPropertyChanged)value).PropertyChanged -= Nfc_PropertyChanged;
            ((INotifyPropertyChanging)value).PropertyChanging -= Npc2_PropertyChanging;
        }

        private void Npc2_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            SyncDataProviderNode child = GetOrAddChild(e.PropertyName);

            child.Updating();
        }

        private void Nfc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SyncDataProviderNode child = GetChild(e.PropertyName);

            child.Update();
        }
#else
        void AddEvents()
        {
            throw new InvalidOperationException("netstandard1.3 or higher needed for this feature");
        }
#endif

        public object Value => value;


        /// <summary>
        /// Create a new node directly associated with a particular property
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncDataProviderNode CreateChild(string name)
        {
            if (value == null)
                throw new InvalidOperationException("No entity from which to create children");

            var entity = value;

            var entityType = entity.GetType();
            var propertyInfo = entityType.GetTypeInfo().GetDeclaredProperty(name);

            Func<object> getter = () => propertyInfo.GetValue(entity);

            return new SyncDataProviderNode(name, getter);
        }

        SyncDataProviderNode GetOrAddChild(string name)
        {
            var child = GetChild(name);

            if (child == null)
            {
                child = CreateChild(name);
                AddChild(child);
            }

            return child;
        }

        public SyncDataProviderNode(string name, Func<object> getter,
            IServiceProvider services = null) : base(name)
        {
            this.getter = getter;
            if(getter != null)
            {
                value = getter();
                AddEvents();
            }
            this.services = services;
        }
    }


    public static class SyncDataProviderNodeExtensions
    {
        public static SyncDataProviderNode AddEntity(this SyncDataProviderNode node, string name, object entity)
        {
            SyncDataProviderNode child = new SyncDataProviderNode(name, () => entity);
            node.AddChild(child);
            return child;
        }
    }

    public class SyncDataProvider : TaxonomyBase<SyncDataProviderNode>
    {
        public override SyncDataProviderNode RootNode { get; }

        public SyncDataProvider(SyncDataProviderNode root) => RootNode = root;

        protected override SyncDataProviderNode CreateNode(SyncDataProviderNode parent, string name) =>
            parent.CreateChild(name);
    }
}
