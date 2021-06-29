using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using OrderCloud.Catalyst;
using Container = Microsoft.Azure.Cosmos.Container;

namespace ordercloud.integrations.library
{
    public abstract class CosmosDbRepository<T> : IRepository<T>, IContainerContext<T> where T : CosmosObject
    {
        public abstract string ContainerName { get; }
        public abstract PartitionKey ResolvePartitionKey(string entityId);
        private readonly ICosmosDbContainerFactory _cosmosDbContainerFactory;
        private readonly Container _container;
        public CosmosDbRepository(ICosmosDbContainerFactory cosmosDbContainerFactory)
        {
            _cosmosDbContainerFactory = cosmosDbContainerFactory ?? throw new ArgumentNullException(nameof(ICosmosDbContainerFactory));
            _container = _cosmosDbContainerFactory.GetContainer(ContainerName)._container;
        }

        public async Task<T> AddItemAsync(T item)
        {
            return await _container.CreateItemAsync<T>(item, ResolvePartitionKey(item.id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<T>(id, ResolvePartitionKey(id));
        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<T> response = await _container.ReadItemAsync<T>(id, ResolvePartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<CosmosListPage<T>> GetItemsAsync(IQueryable<T> queryable, QueryRequestOptions requestOptions, CosmosListOptions listOptions)
        {
            IQueryable<T> filteredQueryable = queryable;

            if (listOptions.Search != null && listOptions.SearchOn != null)
            {
                filteredQueryable = ApplySearchToQueryable(listOptions.Search, listOptions.SearchOn, filteredQueryable);
            }

            if (listOptions.Filters.Any())
            {
                filteredQueryable = ApplyFiltersToQueryable(listOptions.Filters, filteredQueryable);
            }

            if (listOptions.Sort != null)
            {
                filteredQueryable = ApplySortToQueryable(listOptions, filteredQueryable);
            }

            QueryDefinition queryDefinition = filteredQueryable.ToQueryDefinition();

            FeedIterator<T> queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition, listOptions.ContinuationToken, requestOptions);

            List<T> results = new List<T>();
            FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            foreach (T document in currentResultSet)
            {
                results.Add(document);
            }

            Response<int> count = await filteredQueryable.CountAsync();

            return new CosmosListPage<T>()
            {
                Meta = new CosmosMeta() { PageSize = (int)requestOptions.MaxItemCount, Total = count.Resource, ContinuationToken = currentResultSet.ContinuationToken },
                Items = results
            };
        }

        private IQueryable<T> ApplySearchToQueryable(string search, string searchOn, IQueryable<T> filteredQueryable)
        {
            ParameterExpression param = Expression.Parameter(typeof(T));

            Type type = typeof(T);
            Type propertyType = type
                .GetProperties()
                .FirstOrDefault(prop => prop?.Name == searchOn)?
                .PropertyType;

            string propertyTypeName = propertyType?.Name;

            Expression paramProp = Expression.Property(param, searchOn);

            Expression toLower = Expression.Call(paramProp,
                              typeof(string).GetMethod("ToLower", Type.EmptyTypes));

            Expression body = BuildExpressionBody(Expression.Equal, Expression.OrElse, null, toLower, search.ToLower(), propertyType);
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, param);

            return filteredQueryable.Where(lambda);
        }

        private IQueryable<T> ApplyFiltersToQueryable(IList<ListFilter> filters, IQueryable<T> filteredQueryable)
        {
            foreach (ListFilter filter in filters)
            {
                ParameterExpression param = Expression.Parameter(typeof(T));

                Type type = typeof(T);
                Type propertyType = GetPropertyType(filter.PropertyName, type, true);
                Type nullablePropertyType = GetPropertyType(filter.PropertyName, type, false);
                bool useNullablePropType = propertyType != nullablePropertyType;

                string propertyTypeName = propertyType?.Name;

                Expression body = null;

                foreach (ListFilterValue filterValue in filter.FilterValues)
                {
                    object parsedFilterValue = ParseFilterValue(propertyTypeName, filterValue, propertyType);

                    Type adjustedPropType = propertyType;
                    Expression paramProp;
                    if (filter.PropertyName.Contains("."))
                    {
                        string[] nestedProps = filter.PropertyName.Split(".");
                        paramProp = param;
                        var nestedType = type;
                        foreach (string nestedProp in nestedProps)
                        {
                            var matchingProps = nestedType.GetProperties().Where(x => x.Name == nestedProp);
                            var derivedProp = matchingProps.FirstOrDefault(x => x.DeclaringType == nestedType) ?? matchingProps.First();
                            paramProp = Expression.Property(paramProp, derivedProp);
                            nestedType = Nullable.GetUnderlyingType(derivedProp.PropertyType) == null ?
                                derivedProp.PropertyType :
                                Nullable.GetUnderlyingType(derivedProp.PropertyType);
                        }
                    }
                    else
                    {
                        paramProp = Expression.Property(param, filter?.PropertyName);
                    }

                    // Adjust prop type to int for enums
                    if (propertyType.IsEnum)
                    {
                        paramProp = Expression.Convert(paramProp, typeof(int));
                        adjustedPropType = typeof(int);
                    }

                    Type propTypeForExpression = useNullablePropType && !propertyType.IsEnum ? nullablePropertyType : adjustedPropType;

                    if (propTypeForExpression == typeof(List<string>))
                    {
                        MethodInfo contains = typeof(List<string>).GetMethod("Contains");
                        Expression call = Expression.Call(paramProp, contains, Expression.Constant(parsedFilterValue, typeof(string)));
                        body = body == null ? call : Expression.OrElse(body, call);
                    }
                    else if (filter.FilterExpression.EndsWith("*"))
                    {
                        MethodInfo mi = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
                        Expression call = Expression.Call(paramProp, mi, Expression.Constant(parsedFilterValue, typeof(string)));
                        body = body == null ? call : Expression.OrElse(body, call);
                    }
                    else
                    {
                        switch (filterValue.Operator)
                        {
                            case ListFilterOperator.Equal:
                                body = BuildExpressionBody(Expression.Equal, Expression.OrElse, body, paramProp, parsedFilterValue, propTypeForExpression);
                                break;
                            case ListFilterOperator.NotEqual:
                                body = BuildExpressionBody(Expression.NotEqual, Expression.OrElse, body, paramProp, parsedFilterValue, propTypeForExpression);
                                break;
                            case ListFilterOperator.LessThan:
                                body = BuildExpressionBody(Expression.LessThan, Expression.AndAlso, body, paramProp, parsedFilterValue, propTypeForExpression);
                                break;
                            case ListFilterOperator.GreaterThan:
                                body = BuildExpressionBody(Expression.GreaterThan, Expression.AndAlso, body, paramProp, parsedFilterValue, propTypeForExpression);
                                break;
                            case ListFilterOperator.LessThanOrEqual:
                                body = BuildExpressionBody(Expression.LessThanOrEqual, Expression.AndAlso, body, paramProp, parsedFilterValue, propTypeForExpression);
                                break;
                            case ListFilterOperator.GreaterThanOrEqual:
                                body = BuildExpressionBody(Expression.GreaterThanOrEqual, Expression.AndAlso, body, paramProp, parsedFilterValue, propTypeForExpression);
                                break;
                        }
                    }
                }

                Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, param);

                filteredQueryable = filteredQueryable.Where(lambda);
            }

            return filteredQueryable;
        }

        private IQueryable<T> ApplySortToQueryable(CosmosListOptions listOptions, IQueryable<T> filteredQueryable)
        {
            string orderDirection = listOptions.SortDirection == null || listOptions.SortDirection == SortDirection.ASC ? "OrderBy" : "OrderByDescending";
            Type type = typeof(T);
            PropertyInfo property = type.GetProperty(listOptions.Sort);
            ParameterExpression parameter = Expression.Parameter(type, "p");
            MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);
            LambdaExpression orderByExp = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), orderDirection, new Type[] { type, property.PropertyType }, filteredQueryable.Expression, Expression.Quote(orderByExp));
            return filteredQueryable.Provider.CreateQuery<T>(resultExp);
        }

        private Expression BuildExpressionBody(Func<Expression, Expression, BinaryExpression> filterOperator,
                                               Func<Expression, Expression, BinaryExpression> expressionOperator,
                                               Expression body,
                                               Expression paramProp,
                                               object filterValue,
                                               Type propType)
        {
            BinaryExpression predicate = filterOperator(
                            paramProp,
                            Expression.Constant(filterValue, propType)
                        );
            return body == null ? predicate : expressionOperator(body, predicate);
        }

        private object ParseFilterValue(string propertyTypeName, ListFilterValue filterValue, Type propertyType)
        {
            if (propertyTypeName == "DateTime")
            {
                return DateTime.Parse(filterValue.Term);
            }
            else if (propertyTypeName == "DateTimeOffset")
            {
                return DateTimeOffset.Parse(filterValue.Term);
            }
            else if (propertyTypeName == "Int32")
            {
                return Convert.ToInt32(filterValue.Term);
            }
            else if (propertyTypeName == "Decimal")
            {
                return Convert.ToDecimal(filterValue.Term);
            }
            else if (propertyTypeName == "Double")
            {
                return Convert.ToDouble(filterValue.Term);
            }
            else if (propertyTypeName == "Boolean")
            {
                return Convert.ToBoolean(filterValue.Term);
            }
            else if (propertyType.IsEnum)
            {
                return (int)Enum.Parse(propertyType, filterValue?.Term).To(propertyType);
            }
            // strings not converted
            return filterValue.Term;
        }

        // Recursively get nested property type
        private Type GetPropertyType(string filterKey, Type type, bool returnUnderlyingType)
        {
            if (type == null)
            {
                return null;
            }
            var filterKeys = filterKey.Split('.');
            for (var i = 0; i < filterKeys.Length; i++)
            {
                var properties = type.GetProperties();
                for (var j = 0; j < properties.Length; j++)
                {
                    var property = properties[j].Name;
                    if (property == filterKeys[i])
                    {
                        var underlyingType = Nullable.GetUnderlyingType(properties[j].PropertyType);
                        if (underlyingType != null && (i < filterKeys.Length - 1 || returnUnderlyingType))
                        {
                            type = underlyingType;
                        }
                        else
                        {
                            type = properties[j].PropertyType;
                        }
                        if (i < filterKeys.Length - 1)
                        {
                            string[] remainingLevels = new string[filterKeys.Length - i - 1];
                            Array.Copy(filterKeys, i + 1, remainingLevels, 0, filterKeys.Length - i - 1);
                            string remainingKeys = string.Join(".", remainingLevels);
                            return GetPropertyType(remainingKeys, type, returnUnderlyingType);
                        }
                        return type;
                    }
                }
            }
            return null;
        }

        public async Task UpsertItemAsync(string id, T item)
        {
            await _container.UpsertItemAsync<T>(item, ResolvePartitionKey(id));
        }

        public async Task<ItemResponse<T>> ReplaceItemAsync(string id, T item)
        {
            return await _container.ReplaceItemAsync<T>(item, id);
        }

        public IQueryable<T> GetQueryable()
        {
            return _container.GetItemLinqQueryable<T>();
        }
    }
}
