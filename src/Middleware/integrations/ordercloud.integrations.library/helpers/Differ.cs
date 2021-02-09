using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ordercloud.integrations.library
{
    public static class Differ
    {
        public static JObject Diff(this JObject current, JObject cache)
        {
            if (cache == null) return null;
            if (JToken.DeepEquals(current, cache)) return null;

            var diff = new JObject();

            foreach (var (key, value) in current)
            {
                if (key == "Token" || key == "ClientId" || key == "TermsAccepted" || key == "OwnerID") continue;
                var previousValue = cache.SelectToken(key);

                if (JToken.DeepEquals(value, previousValue)) continue;

                if (value.Type == JTokenType.Object)
                {
                    var obj = ((JObject)value).Diff(cache); // recursion
                    if (obj != null)
                    {
                        diff.Add(key, obj);
                    }
                }
                else
                {
                    diff.Add(key, value);
                }
            }

            return diff.HasValues ? diff : null;
        }

        public static bool HasDeletedXp(this JObject cache, JObject current)
        {
            if (current.SelectToken("xp") == null) return false;
            var cacheNodes = new Dictionary<string, string>().ParseNodes(cache.SelectToken("xp"));
            var currentNodes = new Dictionary<string, string>().ParseNodes(current.SelectToken("xp"));
            var diff = cacheNodes.Where(n => currentNodes.All(x => x.Key != n.Key));
            return diff.Any();
        }

        private static IDictionary<string, string> ParseNodes(this IDictionary<string, string> dict, JToken token, string parent = "")
        {
            if (token == null) return dict;
            if (token.HasValues)
            {
                foreach (var child in token.Children())
                {
                    if (token.Type == JTokenType.Property)
                        parent = parent == "" ? ((JProperty)token).Name : $"{parent}.{((JProperty)token).Name}";
                    dict.ParseNodes(child, parent);
                }
                return dict;
            }

            if (dict.ContainsKey(parent))
                dict[parent] += $"|{token}";
            else
                dict.Add(parent, token.ToString());
            return dict;
        }
    }
}
