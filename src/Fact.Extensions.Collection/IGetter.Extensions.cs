using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public static class IGetter_Extensions
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="getter"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue Get<TValue>(this IGetter<string, object> getter, string key)
        {
            return (TValue) getter.Get(key, typeof(TValue));
        }

        public static TValue Get<TValue>(this IGetter<object, object> getter, object key)
        {
            return (TValue)getter.Get(key, typeof(TValue));
        }


        public static async Task<TValue> GetAsync<TValue>(this IGetterAsync<string, object> getter, string key)
        {
            return (TValue)await getter.GetAsync(key, typeof(TValue));
        }


        /*
        public static async Task<TValue> GetOrCreateAsync<TValue>(this IGetterAsync<string, object> getter, string key)
        {

        } */


        public static bool TryGet<TValue>(this ITryGetter getter, string key, out TValue value)
        {
            object output;
            if(getter.TryGet(key, typeof(TValue), out output))
            {
                value = (TValue)output;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }


        public static bool TryGet<TGetter>(this TGetter getter, string key, Type type, out object value)
            where TGetter: IContainsKey<string>, IGetter
        {
            if(getter.ContainsKey(key))
            {
                value = getter.Get(key, type);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// EXPERIMENTAL, does not have the type safety I'd like AND
        /// potentially weakens the type safety of other TryGet calls since they
        /// potentially can overload automatically to this call
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="getter"></param>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetExp<TKey>(this IGetter<TKey, object> getter, TKey key, Type type, out object value)
        {
            // What would be interesting is a factory to resolve interfaces to query
            // and act , but it might ONLY be interesting, maybe not actually useful
            // or practical
            var getterContainsKey = getter as IContainsKey<TKey>;
            var getterKeys = getter as IKeys<TKey>;

            if(getterContainsKey != null)
            {
                if(getterContainsKey.ContainsKey(key))
                {
                    value = getter.Get(key, type);
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
            else if(getterKeys != null)
            {
                if(getterKeys.Keys.Contains(key))
                {
                    value = getter.Get(key, type);
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
            else
                throw new InvalidCastException();
        }

#if NETSTANDARD1_6
        /// <summary>
        /// Listen for changes from a particular property, and when they appear, call a notifier
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="propertyNotifier"></param>
        /// <param name="propertyName"></param>
        /// <param name="notify"></param>
        public static void AddChangeHandler<T, TValue>(this T propertyNotifier, string propertyName, Action<TValue> notify)
            where T :
                IGetter<string, object>,
                System.ComponentModel.INotifyPropertyChanged
        {
            propertyNotifier.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName.Equals(propertyName))
                {
                    var value = propertyNotifier.Get<TValue>(e.PropertyName);
                    notify(value);
                }
            };
        }

        /// <summary>
        /// Listen for changes from a particular property, and when they appear, call a notifier
        /// After registering the listener, query the property.  If it does not match expectedInitialState
        /// fire off notify immediately
        /// </summary>
        /// <param name="propertyNotifier">Property notifier.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="notify">Notify.</param>
        /// <param name="expectedInitialState">Expected initial state.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        /// <typeparam name="TValue">The 2nd type parameter.</typeparam>
        public static void AddChangeHandler<T, TValue>(this T propertyNotifier,
                                                       string propertyName,
                                                       Action<TValue> notify,
                                                       TValue expectedInitialState)
            where T :
                IGetter<string, object>,
                System.ComponentModel.INotifyPropertyChanged
        {
            AddChangeHandler(propertyNotifier, propertyName, notify);
            var actualInitialState = propertyNotifier.Get<TValue>(propertyName);
            if (!actualInitialState.Equals(expectedInitialState))
                notify(actualInitialState);
        }
#endif

        /// <summary>
        /// Get value from this provider, and if it is not yet present return instead a default
        /// </summary>
        /// <typeparam name="TBag"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="bag"></param>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue Get<TBag, TValue>(this TBag bag, string propertyName, TValue defaultValue)
            where TBag : IGetter<string, object>, IContainsKey<string>
        {
            if (bag.ContainsKey(propertyName))
                return bag.Get<TValue>(propertyName);
            else
                return defaultValue;
        }
    }
}
