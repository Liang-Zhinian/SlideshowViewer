using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SlideshowViewer
{
    public static class Extensions
    {
        public static TKey GetKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TValue value) 
            where TKey : class
            where TValue :class 
        {
            if (value == null)
            {
                var keyValuePair = dict.First(pair => pair.Value == null);
                var key = keyValuePair.Key;
                return key;
            }
            EqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
            return (from pair in dict where @default.Equals(pair.Value, value) select pair.Key).FirstOrDefault();
        }

        public static IEnumerable<string> SplitIntoLines(this string s)
        {
            using (var reader = new StringReader(s))
            {
                var line = reader.ReadLine();
                while (line!=null)
                {
                    yield return line;
                    line = reader.ReadLine();
                }
            }
        }

        public static bool MatchGlob(this string s, string pattern)
        {
            pattern = Regex.Escape(pattern);
            pattern=pattern.Replace(@"\*", "[^/]*");
            pattern=pattern.Replace(@"\?", "[^/]?");
            return new Regex("^"+pattern+"$", RegexOptions.IgnoreCase).IsMatch(s);
        }

        public static bool StartsWith<T>(this List<T> l, List<T> start)
        {
            EqualityComparer<T> @default = EqualityComparer<T>.Default;
            if (l.Count < start.Count)
                return false;
            for (int i = 0; i < start.Count; i++)
            {
                if (!@default.Equals(l[i],start[i]))
                    return false;
            }
            return true;
        }

        public static IEnumerable<T> GetShuffled<T>(this IEnumerable<T> items)
        {
            var ret = new SortedDictionary<double, T>();
            var r = new Random();
            foreach (var item in items)
            {
                while (true)
                {
                    var key = r.NextDouble();
                    try
                    {
                        ret.Add(key, item);
                        break;
                    }
                    catch (ArgumentException e)
                    {
                    }
                }
            }
            return ret.Values;
        }

        public static IEnumerable<T> GetSorted<T>(this IEnumerable<T> items)
        {
            var ret = new SortedSet<T>();
            ret.AddAll(items);
            return ret;
        }

        public static bool IsEmpty<T>(this ICollection<T> items)
        {
            return items.Count == 0;
        }

        public static bool IsEmpty(this string s)
        {
            return s.Length == 0;
        }

        public static List<T> GetRange<T>(this List<T> l, int index)
        {
            return l.GetRange(index, l.Count - index);
        }

        public static void AddAll<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static T Largest<T>(this IEnumerable<T> items, Comparison<T> comparer) where T : class
        {
            T largest=null;
            foreach (var item in items)
            {
                if (largest == null || comparer(largest, item) > 0)
                    largest = item;
            }
            return largest;
        }

        public static IEnumerable<T> MergeSorted<T>(Comparison<T> comparer,params IEnumerable<T>[] input) where T:class 
        {
            var all=new List<IEnumerator<T>>(input.Select(enumerable => enumerable.GetEnumerator()));
            all.RemoveAll(enumerator => !enumerator.MoveNext());
            while (!all.IsEmpty())
            {
                var largest = all.Largest((enumerator, enumerator1) => comparer(enumerator.Current, enumerator1.Current));
                yield return largest.Current;
                if (!largest.MoveNext())
                    all.Remove(largest);
            }
        }

    }
}