using System;
using System.Collections.Generic;

namespace Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static void CheckAndAdd(this Dictionary<string, string> dictionary, string key, string value)
        {
            if (key != value)
            {
                dictionary.Add(key, value);
            }
        }

        public static T GetOrDefault<T>(this IDictionary<string, T> dictionary, string key)
        {
            return GetOrDefault(dictionary, key, () => default(T));
        }

        public static string GetOrEmpty(this IDictionary<string, string> dictionary, string key)
        {
            return GetOrDefault(dictionary, key, string.Empty);
        }

        public static T GetOrDefault<T>(this IDictionary<string, T> dictionary, string key, T defaultValue)
        {
            return GetOrDefault(dictionary, key, () => defaultValue);
        }

        public static T GetOrDefault<T>(this IDictionary<string, T> dictionary, string key, Func<T> defaultFactory)
        {
            if (defaultFactory == null)
                throw new ArgumentNullException("defaultFactory");

            if (dictionary == null) return defaultFactory();

            T result;
            return dictionary.TryGetValue(key, out result) ? result : defaultFactory();
        }

        public static List<T> ToList<T>(this IDictionary<string, string> dic, string key, Func<string, T> factory)
        {
            string text;
            var list = new List<T>();
            if (dic.TryGetValue(key, out text))
            {
                text.Split(',').ForEach(s =>
                {
                    list.Add(factory(s.Trim()));
                });
            }
            return list;
        }

        public static TValue GetOrAdd<TKey,TValue>(this IDictionary<TKey, TValue> d,TKey key, Func<TKey,TValue> factory)
        {
            if (!d.TryGetValue(key, out TValue value))
            {
                value = factory(key);
                d[key] = value;
            }

            return value;
        }

        public static IList<TValue> GetOrAdd<TKey, TValue>(this IDictionary<TKey, IList<TValue>> d, TKey key)
        {
            return d.GetOrAdd(key, k => new List<TValue>());
        }

        public static void Increment<T>(this IDictionary<T, int> dic, T key)
        {
            if (dic.TryGetValue(key, out int value))
            {
                value++;
            }
            else
            {
                value = 1;
            }

            dic[key] = value;
        }

        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> destination, IDictionary<TKey, TValue> addition, Func<TValue, TValue, TValue> mergeFun)
        {
            foreach (var entry in addition)
            {
                TValue v;
                if (!destination.TryGetValue(entry.Key, out v))
                {
                    v = default(TValue);
                }

                v = mergeFun.Invoke(entry.Value, v);

                destination[entry.Key] = v;
            }
        }
    }
}
