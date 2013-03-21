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

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
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

    }
}