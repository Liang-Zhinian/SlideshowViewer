using System.Collections.Generic;
using System.Linq;

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
         
    }
}