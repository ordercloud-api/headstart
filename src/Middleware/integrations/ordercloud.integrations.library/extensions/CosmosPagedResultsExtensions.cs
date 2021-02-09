using System;
using System.Collections.Generic;
using Cosmonaut.Response;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public static class CosmosPagedResultsExtensions
    {
        public static ListPage<T> ToListPage<T>(this CosmosPagedResults<T> list, int page, int pageSize, int count)
        {
            var first = ((page - 1) * pageSize) + 1;
            var last = first + pageSize - 1;
            var result = new ListPage<T>
            {
                Items = list.Results,
                Meta = new ListPageMeta
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = count,
                    TotalPages = (int)Math.Ceiling((double)count / pageSize),
                    ItemRange = new[] { first, last }
                }
            };
            return result;
        }

        public static ListPage<T> ToListPage<T>(this List<T> list, int page, int pageSize)
        {
            var first = ((page - 1) * pageSize) + 1;
            var last = first + pageSize - 1;
            var result = new ListPage<T>
            {
                Items = list,
                Meta = new ListPageMeta
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = list.Count,
                    TotalPages = (int)Math.Ceiling((double)list.Count / pageSize),
                    ItemRange = new[] { first, last }
                }
            };
            return result;
        }
    }
}
