using System;
using System.Collections.Generic;

namespace Crisp
{
    static class Utility
    {
        public static TValue GetValue<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key, TValue defaultValue)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        public static Dictionary<TKey, TVal2> MapDictionary<TKey, TVal1, TVal2>(
            this Dictionary<TKey, TVal1> dictionary,
            Func<TKey, TVal1, TVal2> map)
        {
            var result = new Dictionary<TKey, TVal2>();
            foreach (var key in dictionary.Keys)
            {
                var val1 = dictionary[key];
                var val2 = map(key, val1);
                result.Add(key, val2);
            }
            return result;
        }
    }
}
