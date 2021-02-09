using System;
using System.Collections;
using System.Linq;

namespace ordercloud.integrations.library
{
    public static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return type != null && type.WithoutGenericArgs() == typeof(Nullable<>);
        }

        public static Type WithoutGenericArgs(this Type type)
        {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }

        public static bool HasAttribute<T>(this Type type, bool includeInherited = true) where T : Attribute
        {
            var result = type.GetAttributes<T>(includeInherited);
            return result.Any();
        }

        public static bool IsCollection(this Type type)
        {
            return GetCollectionItemType(type) != null;
        }

        /// <summary>
        /// Gets the item type of a collection type, i.e. the T in List&lt;T&gt; or T[], or null if it's not a collection.
        /// </summary>
        public static Type GetCollectionItemType(this Type type)
        {
            if (type.IsArray)
                return type.GetElementType();
            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
                return type.GetGenericArguments()[0];
            return null;
        }
    }
}
