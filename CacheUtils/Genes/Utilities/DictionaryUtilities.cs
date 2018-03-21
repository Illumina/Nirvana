using System;
using System.Collections.Generic;
using System.IO;

namespace CacheUtils.Genes.Utilities
{
    public static class DictionaryUtilities
    {
        public static Dictionary<TK, T> GetSingleValueDict<T, TK>(this IEnumerable<T> elements, Func<T, TK> idFunc)
        {
            var dict = new Dictionary<TK, T>();
            foreach (var element in elements)
            {
                var key = idFunc(element);
                if (key == null) continue;
                if (dict.ContainsKey(key)) throw new InvalidDataException($"Multiple entries for [{key}] already exist in the dictionary.");
                dict[key] = element;
            }
            return dict;
        }

        public static Dictionary<TK, List<T>> GetMultiValueDict<T, TK>(this IEnumerable<T> elements, Func<T, TK> idFunc)
        {
            var dict = new Dictionary<TK, List<T>>();
            foreach (var element in elements)
            {
                var key = idFunc(element);
                if (key == null) continue;
                if (dict.TryGetValue(key, out var geneList)) geneList.Add(element);
                else dict[key] = new List<T> { element };
            }
            return dict;
        }

        public static Dictionary<TK, TV> GetKeyValueDict<T, TK, TV>(this IEnumerable<T> elements, Func<T, TK> keyFunc, Func<T, TV> valueFunc)
        {
            var dict = new Dictionary<TK, TV>();
            foreach (var element in elements)
            {
                var key   = keyFunc(element);
                var value = valueFunc(element);
                if (key == null || value == null) continue;
                dict[key] = value;
            }
            return dict;
        }

        public static HashSet<TV> GetSet<T,TV>(this IEnumerable<T> elements, Func<T, TV> idFunc)
        {
            var set = new HashSet<TV>();
            foreach (var element in elements)
            {
                var key = idFunc(element);
                set.Add(key);
            }
            return set;
        }

        public static Dictionary<T, int> CreateIndex<T>(this IEnumerable<T> elements)
        {
            var index = new Dictionary<T, int>();
            var currentIndex = 0;
            foreach (var element in elements) index[element] = currentIndex++;
            return index;
        }
    }
}
