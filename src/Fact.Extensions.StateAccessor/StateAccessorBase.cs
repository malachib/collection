using Fact.Extensions.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public abstract class StateAccessorBase :
#if FEATURE_DYNAMIC
         DynamicObject,
#endif
        IStateAccessorBase,
        IDirtyMarker
    {
        readonly protected IParameterProvider paramProvider;

        public IParameterProvider ParameterProvider => paramProvider;

        protected StateAccessorBase(IParameterProvider paramProvider) : this()
        {
            this.paramProvider = paramProvider;
        }

        public abstract object this[IParameterInfo p] { get; set; }

        public object this[string name]
        {
            get
            {
                var pinfo = paramProvider.GetParameterByName(name);
                if (pinfo == null)
                    throw new KeyNotFoundException("Unknown parameter: " + name);
                return this[pinfo];
            }
            set
            {
                var pinfo = paramProvider.GetParameterByName(name);
                if (pinfo == null)
                    throw new KeyNotFoundException("Unknown parameter: " + name);
                if (value != null && !pinfo.ParameterType.IsAssignableFrom(value.GetType()))
                    throw new KeyNotFoundException("Incorrect type.  Expecting: " + pinfo.ParameterType + " but got: " + value.GetType());

#if FEATURE_PARAMINFO_EVENTS
                pinfo.DoUpdating(this);
#endif

                DoParameterUpdating(pinfo);

                this[pinfo] = value;

#if FEATURE_PARAMINFO_EVENTS
                pinfo.DoUpdated(this, value);
#endif

                DoParameterUpdated(pinfo, value);
            }
        }


        protected StateAccessorBase()
        {
            ParameterUpdated += (p, o) =>
            {
                dirtyParameters.Add(p);

                // TODO: incomplete feature, need to track reverting back to 0 as well
                if (dirtyParameters.Count == 1) DirtyStateChanged?.Invoke(this);
            };
#if ENABLE_COMPOSITES
			ParameterUpdated += CompositeHelperUpdated;
            ParameterUpdating += CompositeHelperUpdating;
#endif
        }


#if ENABLE_COMPOSITES
		public void CompositeHelperUpdating(IParameterInfo p, object value)
        {
            // FIX: This may break things
            // OSS flavor composites are an add - on
            var v = value as Composite.ICompositeBase;

            if (v != null)
            {
                v.Notify -= v_Notify;
            }
        }
#endif

#if ENABLE_WALKER
        void v_Notify(object composite, Walker.IPropInfo prop, object oldValue, object newValue)
        {
        }
#endif

#if ENABLE_COMPOSITES
		public void CompositeHelperUpdated(IParameterInfo p, object value)
        {
            // FIX: This may break things
            // OSS flavor composites are an add - on
            var v = value as Composite.ICompositeBase;

            if (v != null)
            {
                v.Notify += v_Notify;
            }
        }
#endif

        protected object createEmptyInput(IParameterInfo current)
        {
            var type = current.ParameterType;
            var typeInfo = type.GetTypeInfo();

            if (type == typeof(string))
            {
                // experimenting
                //result[i] = "";
            }
            // FIX: Sometimes we don't actually want our List<>'s created, etc. and would prefer
            // them to stay NULL (symantic differences being important).  
            // So far though it's been harmless and even somewhat helpful
            else if (!(typeInfo.IsInterface || typeInfo.IsAbstract || type.IsArray))
            {
                return Activator.CreateInstance(current.ParameterType);
            }

            return null;
        }

        /// <summary>
        /// Fired when a parameter is reassigned
        /// </summary>
        public event Action<IParameterInfo, object> ParameterUpdated;

        /// <summary>
        /// Fired just before a parameter is reassigned
        /// </summary>
        public event Action<IParameterInfo, object> ParameterUpdating;

        protected void DoParameterUpdating(IParameterInfo p)
        {
            if (ParameterUpdating != null)
                ParameterUpdating(p, this[p]);
        }

        protected void DoParameterUpdated(IParameterInfo p, object value)
        {
            if (ParameterUpdated != null)
                ParameterUpdated(p, value);
        }

#if FEATURE_DYNAMIC
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var prm = paramProvider.GetParameterByName(binder.Name.ToLower());

            if (prm != null)
            {
                result = this[prm];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var prm = paramProvider.GetParameterByName(binder.Name.ToLower());

            if (prm != null)
            {
                DoParameterUpdating(prm);
                this[prm] = value;
                DoParameterUpdated(prm, value);
                return true;
            }
            else
                return false;
        }

#endif

        public event Action<IDirtyMarker> DirtyStateChanged;

        protected HashSet<IParameterInfo> dirtyParameters = new HashSet<IParameterInfo>();

        /// <summary>
        /// Detects whether parameters have been updated
        /// </summary>
        /// <remarks>
        /// TODO: Use DirtyHashset here, and beef up DirtyHashset to fire dirty state change notifications
        /// TODO: Make this able to revert back to non-dirty, looks like this code doesn't offer that provision except
        /// on a downline reset
        /// </remarks>
        public bool IsDirty
        {
            get { return dirtyParameters.Count > 0; }
            set { dirtyParameters.Clear(); }
        }
    }


#if FEATURE_STATEACCESSOR_INTERCEPTOR
    /// <summary>
    /// Wraps a C# interface onto a StateAccessor
    /// </summary>
    public class StateAccessorInterceptor : Fact.Extensions.Collection.PropertyInterceptorBase
    {
        readonly IIndexer<IParameterInfo, object> accessor;

        public StateAccessorInterceptor(IIndexer<IParameterInfo, object> accessor)
        {
            this.accessor = accessor;
        }

        IParameterInfo GetParameterInfo(PropertyInfo prop)
        {
            if (accessor is IParameterProvider)
                return ((IParameterProvider)accessor).GetParameterByName(prop.Name);
            else
                return new Collection.ParameterInfo(prop.Name, prop.PropertyType, -1);
        }

        protected override object Get(IInvocation invocation, PropertyInfo prop)
        {
            return accessor[GetParameterInfo(prop)];
        }

        protected override void Set(IInvocation invocation, PropertyInfo prop, object value)
        {
            accessor[GetParameterInfo(prop)] = value;
        }
    }
#endif

#if READY_STATEACCESSOR_DIFF
    /// <summary>
    /// Specialized StateAccessor relative class used to synchronize two
    /// different StateAccessors together
    /// </summary>
    public class StateAccessorDiff : IStateAccessor
        //, IXmlSerializable, IResettable
    {
        public class Param
        {
            [XmlAttribute("pos")]
            public int Position { get; set; }
            /// <summary>
            /// If the parameter at this position is a class and not a primitive type
            /// it is allowed to update just properties within it, instead of the
            /// whole class
            /// </summary>
            [XmlAttribute("prop")]
            public string Property { get; set; }
            [XmlText]
            public object Value { get; set; }
        }

        public List<Param> Data = new List<Param>();
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Might be better to let gaurunteed delivery mechanisms handle this
        /// </remarks>
        public int SequenceID;

        public void CopyFrom(IStateAccessor source)
        {
            var count = source.InputParameters;

            // FIX: unproven code
            root = source;

            for (int i = 0; i < source.InputParameters.Length; i++)
            {
                var p = new Param();

                p.Position = i;
                p.Value = source[i];

                Data.Add(p);
            }
        }

        public void CopyTo(IStateAccessor dest)
        {
            foreach (var d in Data)
            {
                if (d.Property == null)
                    dest[d.Position] = d.Value;
                else
                {
                    var param = dest.InputParameters[d.Position];
                    var prop = param.ParameterType.GetProperty(d.Property);
                    prop.SetValue(dest[d.Position], d.Value, null);
                }
            }
        }

        public bool IsDirty
        {
            get
            {
                return Data.Count > 0;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event Action<IParameterInfo, object> ParameterUpdated;

        public object this[string name]
        {
            get
            {
                return this[parameterProvider.GetParameterByName(name)];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public object this[int index]
        {
            get
            {
                return this[parameterProvider.InputParameters[index]];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Can be null, represents state that the StateAccessorDiff is a diff from
        /// </summary>
        IStateAccessor root;

        public object this[IParameterInfo p]
        {
            get
            {
                object value = null;
                var data = Data.Where(x => x.Position == p.Position).ToArray();
                var rootParam = data.FirstOrDefault(x => x.Property == null);

                if (rootParam == null)
                {
                    if (root != null)
                        value = root[p];
                }
                else
                    value = rootParam.Value;

                foreach (var d in data.Where(x => x.Property != null))
                {
                    // if rootParam.Value exists, then there should be no properties
#if DEBUG
                    if (rootParam != null)
                        throw new InvalidOperationException("A root parameter present indicates properties should be ignored/replaced and no longer exist in the collection");
#endif

                    var prop = p.ParameterType.GetProperty(d.Property);

                }

                return value;
            }
            set
            {
                if (ParameterUpdated != null)
                    ParameterUpdated(p, value);

                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Can be NULL
        /// </summary>
        IParameterProvider parameterProvider;

        public IParameterProvider ParameterProvider { set { parameterProvider = value; } }

        public IParameterInfo[] InputParameters
        {
            get { return parameterProvider.InputParameters; }
        }

        public IParameterInfo GetParameterByName(string name)
        {
            return parameterProvider.GetParameterByName(name);
        }

        public event Action<IResettable> Resetting;

        public void Reset()
        {
            if (Resetting != null)
                Resetting(this);

            throw new NotImplementedException();
        }

#if FEATURE_OLD_XML
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();
            Data = new List<Param>(reader.ReadItems<Param>("paramValues"));
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteItems(Data, "paramValues");
        }
#endif
    }
#endif

#if LEGACY_PARAMETERINFO
    //[Serializable]
    // doing manual serialization because auto-serializing ParameterType doesn't seem to be working out
    // (wants an XmlInclude or SoapInclude even though KnownTypes & serializer are both initialized
    // with the desired type)
    //
    public class ParameterInfo : 
        //Pluggable, INamed, 
        IParameterInfo 
        //IXmlSerializable
    {
        public string Name { get; protected set; }
        public Type ParameterType { get; protected set; }
        public int Position { get; internal set; } // internal because WF initializer rewrites these when combining from Step & StepValidator

        public ParameterInfo(System.Reflection.ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name;
            ParameterType = parameterInfo.ParameterType;
            Position = parameterInfo.Position;
        }

        public ParameterInfo() { }

        public ParameterInfo(string name, Type parameterType, int position)
        {
            Name = name;
            ParameterType = parameterType;
            Position = position;
        }

        public override bool Equals(object obj)
        {
            var compareTo = (ParameterInfo)obj;

            return Name.Equals(compareTo.Name) &&
                ParameterType == compareTo.ParameterType &&
                Position == compareTo.Position;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^
                ParameterType.GetHashCode() ^
                Position;
        }

        /// <summary>
        /// As is obvious one potentially can get updates from many different SA's
        /// for this one parameter.  Be aware
        /// </summary>
        public event Action<IStateAccessorBase, IParameterInfo> Updating;
        /// <summary>
        /// As is obvious one potentially can get updates from many different SA's
        /// for this one parameter.  Be aware
        /// </summary>
        public event Action<IStateAccessorBase, IParameterInfo, object> Updated;

        public void DoUpdating(IStateAccessorBase sa)
        {
            if (Updating != null)
                Updating(sa, this);
        }


        public void DoUpdated(IStateAccessorBase sa, object newValue)
        {
            if (Updated != null)
                Updated(sa, this, newValue);
        }
    }
#endif
}
