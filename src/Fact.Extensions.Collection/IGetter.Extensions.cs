using System;
using System.Collections.Generic;
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
