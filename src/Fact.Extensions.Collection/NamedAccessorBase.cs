using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
#if DYNAMICBASE
    using System.Dynamic;

    //using Castle.Windsor;
    //using Castle.MicroKernel.Registration;

    public abstract class NamedAccessorBase<TValue> : DynamicObject
    {
#if UNUSED
        // meta handler to wrap underlying try get methods.  A little overkill,
        // but lack of MI demands something like this
        public interface ITryGetHandler<TKey, _TValue>
        {
            bool TryGet(object source, TKey key, out _TValue result);
        }

        public interface ITryGetHandler<TKey> : ITryGetHandler<TKey, TValue> { }

        internal abstract class TryGetHandlerBase<TKey, TInterface> : ITryGetHandler<TKey>
            where TInterface: class
        {
            public bool TryGet(object source, TKey key, out TValue result)
            {
                var s = source as TInterface;

                if (s == null)
                {
                    result = default(TValue);
                    return false;
                }

                return TryGet(s, key, out result);
            }

            protected abstract bool TryGet(TInterface source, TKey key, out TValue result);
        }

        internal class NativeTryGetHandler<TKey> : 
            TryGetHandlerBase<TKey, IAccessorWithTryGet<TKey, TValue>>
        {
            protected override bool TryGet(IAccessorWithTryGet<TKey, TValue> source, TKey key, out TValue result)
            {
                return source.TryGetValue(key, out result);
            }
        }


        internal class MetaTryGetHandler<TKey> : 
            TryGetHandlerBase<TKey, IAccessorWithMeta<TKey, TValue>>
        {
            protected override bool TryGet(IAccessorWithMeta<TKey, TValue> source, TKey key, out TValue result)
            {
                return source.TryGetValue(key, out result);
            }
        }

        static readonly IWindsorContainer container = new WindsorContainer();
#endif

        public delegate bool TryGetDelegate(string key, out TValue result);

        static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        static readonly TypeReturnDispatcher<object, TryGetDelegate> dispatcher =
            new TypeReturnDispatcher<object, TryGetDelegate>();

        protected readonly TypeReturnDispatcher<object, TryGetDelegate> tryGetDispatcher;

        static NamedAccessorBase()
        {
#if UNUSED
            // TODO: merge this with "pluggable" concept
            // TODO: create a version of this that is delegate based, which handles the forward casting as part
            //       of the framework
            container.Register(Component.For<ITryGetHandler<string>>().Instance(new NativeTryGetHandler<string>()));
            container.Register(Component.For<ITryGetHandler<string>>().Instance(new MetaTryGetHandler<string>()));
#endif

            dispatcher.Add<IAccessorWithMeta<string, TValue>>((accessor) => accessor.TryGetValue);
            dispatcher.Add<IAccessorWithTryGet<string, TValue>>((accessor) => accessor.TryGetValue);
        }


        protected NamedAccessorBase()
        {
            tryGetDispatcher = dispatcher;
        }

        // FIX: Icky, but lack of MI sorta railroads us into forward casting.
        // TODO: Look into making it IoC based.  Overkill?
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var key = binder.Name;
#if UNUSED
            var candidates = container.ResolveAll<ITryGetHandler<string>>();
#endif

            var tryGetDelegate = dispatcher.Dispatch(this);

            if (tryGetDelegate != null)
            {
                TValue _result;
                var isPresent = tryGetDelegate(key, out _result);
                result = _result;
                return isPresent;
            }

            try
            {
                var thisAccessor = this as IAccessor<string, TValue>;
                result = thisAccessor[binder.Name];
                return true;
            }
            catch (Exception e)
            {
                TryGetMemberExceptionHandler(key, e);
                result = default(TValue);
                return false;
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var keyProvider = this as IAccessor_Keys<string>;

            if (keyProvider != null)
                return keyProvider.Keys;
            else
                return base.GetDynamicMemberNames();
        }

        protected virtual void TryGetMemberExceptionHandler(string key, Exception e)
        {
            logger.D("Cannot retreive value for key " + key + " returning default value instead", e);
        }
    }
#endif
}
