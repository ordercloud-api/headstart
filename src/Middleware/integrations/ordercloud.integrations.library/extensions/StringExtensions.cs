using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ordercloud.integrations.library
{
    public static class StringExtensions
    {
        public static DateTime UnixToDateTimeUTC(this string unix)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddSeconds(int.Parse(unix));
        }

        public static string JoinString<T>(this IEnumerable<T> items, string separator)
        {
            return string.Join(separator, items);
        }

        public static string JoinString<T>(this IEnumerable<T> items, string separator, Func<T, object> transform)
        {
            return string.Join(separator, items.Select(transform));
        }

        public static string ToSafeId(this string obj, params string[] replacements)
        {
            var edited = replacements.Aggregate(obj, (current, r) => current.Replace(r, ""));
            return Regex.Replace(edited, @"[^a-zA-Z0-9-_]+", "_").ToLower();
        }

        public static string TrimStart(this string s, params string[] remove)
        {
            foreach (var r in remove)
                while (s.StartsWith(r))
                    s = s.Substring(r.Length);
            return s;
        }

        public static string TrimEnd(this string s, params string[] remove)
        {
            foreach (var r in remove)
                while (s.EndsWith(r))
                    s = s.Substring(0, s.Length - r.Length);
            return s;
        }
    }
}
