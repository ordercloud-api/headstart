using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Headstart.Common.Extensions
{
    public static class ParameterInfoExtensions
    {
        public static bool HasAttribute<T>(this ParameterInfo param, bool includeInherited = true) where T : Attribute
        {
            return param.GetAttributes<T>(includeInherited).Any();
        }

        public static T GetAttribute<T>(this ParameterInfo param, bool includeInherited = true) where T : Attribute
        {
            return param.GetAttributes<T>(includeInherited).FirstOrDefault();
        }

        public static IEnumerable<T> GetAttributes<T>(this ParameterInfo param, bool includeInherited = true) where T : Attribute
        {
            var result = param.GetCustomAttributes(typeof(T), includeInherited);
            return result.Select(a => a as T);
        }
    }
}
