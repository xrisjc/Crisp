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

        public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(
            this IEnumerable<(TKey, TValue)> items)
        {
            var dict = new Dictionary<TKey, TValue>();
            foreach (var (key, value) in items)
            {
                dict[key] = value;
            }
            return dict;
        }
    }
}
