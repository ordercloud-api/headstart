using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ordercloud.integrations.library;

namespace ordercloud.integrations.library.Cosmos
{
    public static class ExpressionBuilder
    {
        private static Tuple<Expression, Type> GetExpression<T>(Expression param, string sort)
        {
            var member = param;
            member = sort.Replace("!", "").Split(".").Aggregate(member, Expression.Property); // takes X.X notation and gets to the nested property member

            var propertyType = ((PropertyInfo)member.To<MemberExpression>().Member).PropertyType;
            return Tuple.Create(member, propertyType);
        }

        private static Expression GetExpression<T>(Expression param, ListFilter filter)
        {
            var member = param;
            member = filter.Name.Split(".").Aggregate(member, Expression.Property); // takes X.X notation and gets to the nested property member

            var propertyType = ((PropertyInfo)member.To<MemberExpression>().Member).PropertyType;
            if (propertyType.GetInterface(nameof(IList)) != null) {
                var elementType = propertyType.GetTypeInfo().GenericTypeArguments[0];
                return filter.ORExpressions(f => GetListExpression<T>(elementType, member, f));
            }

            if (propertyType.IsEnum || (propertyType.IsNullable() && Nullable.GetUnderlyingType(propertyType).IsEnum))
                return filter.ORExpressions(f => GetEnumExpression<T>(propertyType, member, f));
            return filter.ORExpressions(f => GetStringExpression<T>(propertyType, member, f));
        }

        private static Expression ORExpressions(this ListFilter filter, Func<ListFilterValue, Expression> func)
		{
            var seed = Expression.Constant(false);
            return filter.Values.Aggregate<ListFilterValue, Expression>(seed, (expression, filter) => Expression.OrElse(expression, func(filter)));
        }

        private static Expression ANDExpressions(this IList<ListFilter> filters, Func<ListFilter, Expression> func)
        {
            var seed = Expression.Constant(true);
            return filters.Aggregate<ListFilter, Expression>(seed, (expression, filter) => Expression.AndAlso(expression, func(filter)));
        }

        public static Expression<Func<T, bool>> GetSearchExpression<T>(IListArgs args)
        {
            if (args?.Search == null || args?.SearchOn == null)
                return null;
            var param = Expression.Parameter(typeof(T), args?.SearchOn);

            var expr = GetExpression<T>(param, new ListFilter()
            {
                Name = args?.SearchOn,
                Values = new List<ListFilterValue>()
                {
                    new ListFilterValue() { Operator = ListFilterOperator.Equal, Term = args?.Search, WildcardPositions = new List<int>(){0,1} }
                }
            });
            return Expression.Lambda<Func<T, bool>>(expr, param);
        }

        public static Expression<Func<T, bool>> GetFilterExpression<T>(IListArgs args)
        {
            if (args?.Filters == null || args?.Filters.Count == 0)
                return null;
            var filters = args?.Filters;

            var param = Expression.Parameter(typeof(T), typeof(T).Name);

            Expression exp = filters.ANDExpressions(filter => GetExpression<T>(param, filter));

            return Expression.Lambda<Func<T, bool>>(exp, param);
        }

        public static Tuple<Expression, Type> GetSortByExpression<TSource>(IListArgs args)
        {
            if (args?.SortBy.Count == 0)
                return null;
            var sort = args?.SortBy;
            var param = Expression.Parameter(typeof(TSource), typeof(TSource).Name);
            Tuple<Expression, Type> exp;
            switch (sort?.Count)
            {
                case 1:
                    exp = GetExpression<TSource>(param, sort[0]);
                    break;
                default:
                    throw new NotImplementedException("Multiple sort is not supported");

            }
            return Tuple.Create(Expression.Lambda(exp.Item1, param) as Expression, exp.Item2);
        }

		private static Expression GetEnumExpression<T>(Type propertyType, Expression member, ListFilterValue filter)
		{
			// TODO: this can't handle multiple values for single filter. ex: Action: Get|Ignore|Update
			if (filter == null)
				return Expression.Empty();

			ConstantExpression right = null;
			if (propertyType.IsEnum)
				right = Expression.Constant((int)Enum.Parse(propertyType, filter?.Term).To(propertyType));
			else if (Nullable.GetUnderlyingType(propertyType).IsEnum)
				right = Expression.Constant((int)Enum.Parse(propertyType.GenericTypeArguments[0], filter?.Term).To(propertyType));

			//var right = Expression.Constant((int)Enum.Parse(propertyType, filter.Values.FirstOrDefault()?.Term).To(propertyType));
			var left = Expression.Convert(member, typeof(int));

			switch (filter.Operator)
			{
				// doesn't yet support the | OR operator
				case ListFilterOperator.Equal:
					return Expression.Equal(left, right);
				case ListFilterOperator.NotEqual:
					return Expression.NotEqual(left, right);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

        // May not handle objects inside of arrays correctly. Only arrays of simple types like strings and numbers
		public static Expression GetListExpression<T>(Type elementType, Expression member, ListFilterValue filter)
		{
			if (filter == null)
				return Expression.Empty();

            var converter = TypeDescriptor.GetConverter(elementType);
			var elementValue = converter.ConvertFromInvariantString(filter?.Term);
            var constant = Expression.Constant(elementValue?.ToString().To(elementType));
            var method = (typeof(Enumerable))
                .GetMethods()
                .Where(m => m.Name == "Any" && m.GetParameters().Length == 2)
                .FirstOrDefault()
                .MakeGenericMethod(elementType);
			var lambdaParam = Expression.Parameter(elementType, elementType.Name);
            var lambdaBody = GetStringExpression<T>(elementType, lambdaParam, filter);  // If any element of the array matches the filter expression, return that record.

             return Expression.Call(method, member, Expression.Lambda(lambdaBody, lambdaParam));
        }

        public static Expression GetStringExpression<T>(Type propertyType, Expression member, ListFilterValue filter)
        {
            // TODO: this can't handle multiple values for single filter. ex: Action: Get|Ignore|Update
			if (filter == null)
				return Expression.Empty();

            var converter = TypeDescriptor.GetConverter(propertyType);
            // works for strings and probably any non-complex object
            var propertyValue = converter.ConvertFromInvariantString(filter?.Term);
            var constant = Expression.Constant(propertyValue?.ToString().To(propertyType));
            var right = Expression.Convert(constant, propertyType);

            // doesn't yet support the | OR operator
            switch (filter.Operator)
            {
                case ListFilterOperator.Equal:
                    // * operator for start, contains and end
                    if (filter.HasWildcard)
                    {
                        var term = "";
                        if (filter.WildcardPositions.Count == 2)
                            term = "Contains";
                        else if (filter.WildcardPositions[0] == 0)
                            term = "EndsWith";
                        else if (filter.WildcardPositions[0] > 0)
                            term = "StartsWith";
                        var method = typeof(string).GetMethod(term, new[] { propertyType });
                        return Expression.Call(member, method, constant);
                    }
                    return Expression.Equal(member, right);
                case ListFilterOperator.NotEqual:
                    return Expression.NotEqual(member, right);
                case ListFilterOperator.GreaterThan:
                    return Expression.GreaterThan(member, right);
                case ListFilterOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, right);
                case ListFilterOperator.LessThan:
                    return Expression.LessThan(member, right);
                case ListFilterOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, right);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
