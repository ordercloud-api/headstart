using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ordercloud.integrations.library
{
    public static class ExpressionExtensions
    {
        public static PropertyInfo GetPropertyInfo<T, TProp>(this Expression<Func<T, TProp>> property)
        {
            // http://stackoverflow.com/a/672212/62600

            var member = (property.Body.NodeType == ExpressionType.Convert)
                ? ((UnaryExpression) property.Body).Operand as MemberExpression
                : property.Body as MemberExpression;

            if (member == null)
                throw new Exception($"Can't get property info for {property} because it's not a property.");

            var pi = member.Member as PropertyInfo;
            if (pi == null)
                throw new Exception($"Can't get property info for {property} because it's not a property.");

            return pi;
        }
    }
}
