using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ordercloud.integrations.library
{
    public static class MemberInfoExtensions
    {
        public static T GetAttribute<T>(this MemberInfo member, bool includeInherited = true) where T : Attribute
        {
            var result = member.GetAttributes<T>(includeInherited);
            return result.FirstOrDefault();
        }

        public static IEnumerable<T> GetAttributes<T>(this MemberInfo member, bool includeInherited = true) where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), includeInherited).Select(a => a as T);
        }

        public static bool HasAttribute<T>(this MemberInfo member, bool includeInherited = true) where T : Attribute
        {
            var result = member.GetAttributes<T>(includeInherited);
            return result.Any();
        }
    }
}
