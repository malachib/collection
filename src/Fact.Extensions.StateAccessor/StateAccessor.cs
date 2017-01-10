using System;
using System.Linq;
using System.Reflection;
//using System.Xml.Serialization;
//using System.Runtime.Serialization;

using Fact.Extensions.State;

namespace Fact.Extensions.Collection
{
    /// <summary>
    /// Similar to a session state object, but with more type constraints
    /// </summary>
    public class StateAccessor : StateAccessorBase,
        IStateAccessor
        //, IXmlSerializable, ISerializable
        , IResettable
    {
        /// <summary>
        /// Bundling mechanism to contain IStateAccessor and a particular IParameterInfo
        /// together
        /// </summary>
        public struct Accessor
        {
            public readonly IStateAccessor sa;

            /// <summary>
            /// Access particular value the underlying sa[parameter] points to
            /// </summary>
            public object value
            {
                get { return sa[parameter]; }
                set { sa[parameter] = value; }
            }
            public readonly IParameterInfo parameter;

            internal Accessor(IStateAccessor sa, IParameterInfo parameter)
            {
                this.sa = sa;
                this.parameter = parameter;
            }
        }

        public void Reset()
        {
            if (Resetting != null)
                Resetting(this);

            state = null;
            dirtyParameters.Clear();
        }

        public event Action<IResettable> Resetting;

        public override object this[IParameterInfo p]
        {
            get
            {
                stateHelper();
                return state[paramProvider.IndexOf(p)];
            }
            set
            {
                stateHelper();

#if FEATURE_PARAMINFO_EVENTS
                p.DoUpdating(this);
#endif

                DoParameterUpdating(p);

                state[paramProvider.IndexOf(p)] = value;

#if FEATURE_PARAMINFO_EVENTS
                p.DoUpdated(this, value);
#endif

                DoParameterUpdated(p, value);
            }
        }

        /// <summary>
        /// APR-147: temporary.  Client should actually allocate object and submit to workflow,
        /// not internally create.  This kludge is invisible to external processes
        /// </summary>
        /// <returns></returns>
        object[] createState()
        {
            var result = new object[paramProvider.Count];

            for (int i = 0; i < result.Length; i++)
            {
                var current = paramProvider[i];
                result[i] = createEmptyInput(current);
            }
            return result;
        }

        object[] state;

        public object[] State
        {
            get { return stateHelper(); }
        }

        /// <summary>
        /// Initialize empty state row
        /// </summary>
        /// <returns></returns>
        protected object[] stateHelper()
        {
            return state ?? (state = createState());
        }

        /// <summary>
        /// Needed for XML serialization.  Do not call directly
        /// </summary>
        public StateAccessor() { }

        public StateAccessor(IParameterProvider paramProvider, object[] state)
            : this(paramProvider)
        {
            this.state = state;
        }

        public StateAccessor(IParameterProvider paramProvider) : base(paramProvider)
        {
        }

        public StateAccessor(StateAccessor sa)
            : this(sa.paramProvider)
        {
            state = sa.state;
        }

        public object this[int index]
        {
            get
            {
                stateHelper();
                return state[index];
            }
            set
            {
                stateHelper();

                var pinfo = paramProvider[index];
#if DEBUG

                if (value != null && !pinfo.ParameterType.IsAssignableFrom(value.GetType()))
                    throw new IndexOutOfRangeException("Incorrect type.  Expecting: " + pinfo.ParameterType + " but got: " + value.GetType());
#endif
                DoParameterUpdating(pinfo);

                state[index] = value;

                DoParameterUpdated(pinfo, value);
            }
        }


#if LEGACY_XML
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();

            paramProvider = (IParameterProvider)reader.ReadElementValue("paramProvider");

            /*
            var extraTypes = from n in paramProvider.InputParameters
                             let t = n.ParameterType
                             where !t.IsInterface
                             select t;*/

            state = reader.ReadItems<object>("stateArray").ToArray();
            //state = reader.ReadItems<object>("stateArray", extraTypes.ToArray()).ToArray();
        }


        /// <summary>
        /// Experimental, so that WriteXml can reach "forward" in MODULAR mode
        /// and do extra Workflow-processing (or other processing) if necessary
        /// </summary>
        public interface IWriteXmlProcessor
        {
            /// <summary>
            /// Coerce IParameterProvider, if necessary
            /// </summary>
            /// <param name="pp"></param>
            /// <returns></returns>
            IParameterProvider Evaluate(IParameterProvider pp);
        }


#if MONODROID
        public static readonly TinyIoC.TinyIoCContainer container = new TinyIoC.TinyIoCContainer();
#else
        public static readonly Castle.Windsor.IWindsorContainer container = new Castle.Windsor.WindsorContainer();
#endif

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            var pp = paramProvider;
            // FIX: Kludge, sometimes we use StepBase as a container for IParameterProvider
            // but we don't want attempt to serialize the whole StepBase in this case, just
            // the provider.  So extract it for this operation
            // FIX: Kludge causing issues for MODULAR mode which at this level doesn't have access 
            // to Workflow code base - so Workflow serialization is broken now for Modular mode
            // FIX: Now making unused, because IoC-based mechanism is almost fully activated BUT
            // it is not yet tested.  And as of this writing, it is NOT active for MODULAR
            // (but no less functional than the previous kludgey modular code)
#if !MODULAR
#if UNUSED
            if (pp is Workflow.StepBase)
            {
                var _pp = new Workflow.ParameterProvider();

                foreach (var p in pp.InputParameters)
                    _pp.Add(p.Name, p.ParameterType);

                pp = _pp;
            }
#endif
#endif

            var writeXmlProcessors = container.ResolveAll<IWriteXmlProcessor>();

            foreach (var processor in writeXmlProcessors)
            {
                pp = processor.Evaluate(pp);
            }

            writer.WriteElementValue("paramProvider", pp);
            /*
            var extraTypes = from n in paramProvider.InputParameters
                             let t = n.ParameterType
                             where !t.IsInterface
                             select t;*/

            if (state != null)
            {
                var extraTypes = from n in state
                                 where n != null
                                 select n.GetType();

                writer.WriteItems(state, "stateArray", extraTypes.ToArray());
            }
            else
                writer.WriteItems(state, "stateArray");
        }
#endif


#if LEGACY_SERIALIZATION
        public StateAccessor(SerializationInfo info, StreamingContext context)
        {
            state = info.GetValue<object[]>("stateArray");
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var extraTypes = from n in paramProvider.InputParameters
                             let t = n.ParameterType
                             where !t.IsInterface
                             select t;

            info.AddValue("stateArray", state);
        }
#endif
    }

    public static class IStateAccessor_Extensions2
    {
        /// <summary>
        /// Retrieve an Accessor which is a conveninent holder mechanism for IStateAccessor and IParameterInfo pairing
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static StateAccessor.Accessor GetAccessor(this IStateAccessor sa, int parameter)
        {
            var p = sa.ParameterProvider[parameter];
            var a = new StateAccessor.Accessor(sa, p);
            return a;
        }


        public static StateAccessor.Accessor GetAccessor(this IStateAccessor sa, IParameterInfo p)
        {
            var a = new StateAccessor.Accessor(sa, p);
            return a;
        }
    }
}
