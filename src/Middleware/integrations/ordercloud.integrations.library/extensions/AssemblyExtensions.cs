using System;
using System.Linq;
using System.Reflection;

namespace ordercloud.integrations.library
{
    public static class AssemblyExtensions
    {
        public static Type GetTypeByAttribute<T>(this Assembly assembly, Func<T, bool> op) where T : Attribute
        {
            var commands = assembly.GetTypes().Where(t => t.HasAttribute<T>(false));
            var type = commands.FirstOrDefault(t => t.GetCustomAttributes<T>().Any(op));
            return type;
        }
    }
}
