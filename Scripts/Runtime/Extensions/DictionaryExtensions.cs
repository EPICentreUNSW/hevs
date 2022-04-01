using System;
using System.Collections.Generic;
using System.Text;

namespace HEVS.Extensions {
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class DictionaryExtensions {
        /// <summary>
        /// Return the value for key if key is in the dictionary, else defaultValue. If defaultValue is not given, it defaults to <c>default(TValue)</c>, so that this method never raises a <see cref="KeyNotFoundException"/>.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default) {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }
}
