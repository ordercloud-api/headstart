using System;
using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Common.Extensions
{
    public static class ListExtensions
    {
        public static bool HasItem<T>(this List<T> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            {
                return false;
            }

            return true;
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
                    ItemRange = new[] { first, last },
                },
            };
            return result;
        }
    }
}
