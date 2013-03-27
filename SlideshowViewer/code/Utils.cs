using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SlideshowViewer
{
    public static class Utils
    {
        public static TKey GetKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TValue value)
            where TKey : class
            where TValue : class
        {
            if (value == null)
            {
                KeyValuePair<TKey, TValue> keyValuePair = dict.First(pair => pair.Value == null);
                TKey key = keyValuePair.Key;
                return key;
            }
            EqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
            return (from pair in dict where @default.Equals(pair.Value, value) select pair.Key).FirstOrDefault();
        }

        public static IEnumerable<string> SplitIntoLines(this string s)
        {
            using (var reader = new StringReader(s))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    yield return line;
                    line = reader.ReadLine();
                }
            }
        }

        public static bool MatchGlob(this string s, string pattern)
        {
            pattern = Regex.Escape(pattern);
            pattern = pattern.Replace(@"\*", "[^/]*");
            pattern = pattern.Replace(@"\?", "[^/]?");
            return new Regex("^" + pattern + "$", RegexOptions.IgnoreCase).IsMatch(s);
        }

        public static bool StartsWith<T>(this List<T> l, List<T> start)
        {
            EqualityComparer<T> @default = EqualityComparer<T>.Default;
            if (l.Count < start.Count)
                return false;
            for (int i = 0; i < start.Count; i++)
            {
                if (!@default.Equals(l[i], start[i]))
                    return false;
            }
            return true;
        }

        public static IEnumerable<T> GetShuffled<T>(this IEnumerable<T> items)
        {
            var ret = new SortedDictionary<double, T>();
            var r = new Random();
            foreach (T item in items)
            {
                while (true)
                {
                    double key = r.NextDouble();
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
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        public static T Largest<T>(this IEnumerable<T> items, Comparison<T> comparer) where T : class
        {
            T largest = null;
            foreach (T item in items)
            {
                if (largest == null || comparer(largest, item) > 0)
                    largest = item;
            }
            return largest;
        }

        public static IEnumerable<T> MergeSorted<T>(Comparison<T> comparer, params IEnumerable<T>[] input)
            where T : class
        {
            var all = new List<IEnumerator<T>>(input.Select(enumerable => enumerable.GetEnumerator()));
            all.RemoveAll(enumerator => !enumerator.MoveNext());
            while (!all.IsEmpty())
            {
                IEnumerator<T> largest =
                    all.Largest((enumerator, enumerator1) => comparer(enumerator.Current, enumerator1.Current));
                yield return largest.Current;
                if (!largest.MoveNext())
                    all.Remove(largest);
            }
        }

        public static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            var b = new byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }

        public static T[] Array<T>(params T[] input)
        {
            return input;
        }

        public static string FirstWord(this string s)
        {
            return s.Split(Array(" "), StringSplitOptions.None)[0];
        }

        public static byte[] Concat(params byte[][] input)
        {
            var stream = new MemoryStream();
            foreach (var b in input)
            {
                stream.Write(b, 0, b.Length);
            }
            return stream.ToArray();
        }
    }
}