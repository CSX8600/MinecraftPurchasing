using System;
using System.Collections.Generic;

namespace ClussPro.Base.Extensions
{
    public static class DictionaryExt
    {
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return defaultValue;
        }

        public static TValue GetOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> ctor)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, ctor());
            }

            return dictionary[key];
        }
    }
}
