using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace ordercloud.integrations.library
{
    public interface IListArgs
    {
        string Search { get; set; }
        string SearchOn { get; set; }
        IList<string> SortBy { get; set; }
        int Page { get; set; }
        int PageSize { get; set; }
        IList<ListFilter> Filters { get; set; }
        void ValidateAndNormalize();
    }

    public class ListArgsPageOnly
	{
        public ListArgsPageOnly()
        {
            Page = 1;
            PageSize = 100;
        }

        public ListArgsPageOnly(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }


   [ModelBinder(typeof(ListArgsModelBinder))]
    public class ListArgs<T> : IListArgs
    {
        public ListArgs()
        {
            Page = 1;
            PageSize = 20;
        }
        public string SearchOn { get; set; }
        public string Search { get; set; }
        public IList<string> SortBy { get; set; } = new List<string>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public IList<ListFilter> Filters { get; set; } = new List<ListFilter>();

        public void ValidateAndNormalize()
        {
            var newSortBy = new List<string>();
            foreach (var s in this.SortBy)
            {
                if (newSortBy.Contains(s, StringComparer.InvariantCultureIgnoreCase))
                    continue;
                var desc = s.StartsWith("!");
                var name = s.TrimStart('!');
                var prop = FindSortableProp(typeof(T), name);
                newSortBy.Add(desc ? "!" + prop : prop);
            }
            this.SortBy = newSortBy;
        }

        private static string FindSortableProp(Type type, string path)
        {
            if (path.StartsWith("xp."))
                return path;

            var queue = new Queue<string>(path.Split('.'));
            var prop = type.GetProperty(queue.Dequeue(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            Require.That(prop != null, ErrorCodes.List.InvalidProperty, new InvalidPropertyError(type, path));
            //TODO: evaluate this requirement for reference sake
            //Require.That(prop.HasAttribute<SortableAttribute>(), ErrorCodes.List.InvalidSortProperty, new InvalidPropertyError(type, path));
            var result = prop?.Name;
            if (queue.Any())
                result += "." + FindSortableProp(prop.PropertyType, queue.JoinString("."));
            return result;
        }

        public string ToFilterString()
        {
            var filterList = (
                from filter in this.Filters
                from param in filter.QueryParams
                select new Tuple<string, string>(param.Item1, param.Item2))
                .ToList();

            var result = filterList.JoinString("&", t => $"{t.Item1}={t.Item2.Replace("&", "%26")}");

            return result;
        }
    }
}
