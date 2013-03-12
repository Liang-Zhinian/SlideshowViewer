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
        
    }
}