using System.Linq;
using ordercloud.integrations.library.Cosmos;
using ordercloud.integrations.library;

namespace ordercloud.integrations.library
{
    public static class QueryableExtensions
    {
        public static IOrderedQueryable<T> Sort<T>(this IQueryable<T> source, IListArgs args) where T : class
        {
            var expr = ExpressionBuilder.GetSortByExpression<T>(args);
            if (expr == null) return source as IOrderedQueryable<T>;
            var direction = args.SortBy[0].Contains("!") ? "OrderByDescending" : "OrderBy";
            var genericMethod = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == direction && m.GetParameters().Count() == 2)
                .MakeGenericMethod(typeof(T), expr.Item2);
            var query = (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { source, expr.Item1 });
            return query;
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> source, IListArgs args) where T : class
        {
            var expr = ExpressionBuilder.GetFilterExpression<T>(args);
            if (expr == null) return source;
            var genericMethod = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == "Where" && m.IsGenericMethodDefinition && m.GetParameters().ToList().Count == 2)
                .MakeGenericMethod(typeof(T));
            var query = (IQueryable<T>)genericMethod.Invoke(genericMethod, new object[] { source, expr });
            return query;
        }

        public static IQueryable<T> Search<T>(this IQueryable<T> source, IListArgs args) where T : class
        {
            var expr = ExpressionBuilder.GetSearchExpression<T>(args);
            if (expr == null) return source;
            var genericMethod = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == "Where" && m.IsGenericMethodDefinition && m.GetParameters().ToList().Count == 2)
                .MakeGenericMethod(typeof(T));
            var query = (IQueryable<T>)genericMethod.Invoke(genericMethod, new object[] { source, expr });
            return query;
        }
    }
}
