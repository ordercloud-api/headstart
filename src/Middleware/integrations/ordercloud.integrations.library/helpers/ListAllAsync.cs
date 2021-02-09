using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ordercloud.integrations.library.helpers
{
    public class ListAllAsync
    {
        public static async Task<List<T>> List<T>(Func<int, Task<ListPage<T>>> listFunc)
        {
            var pageTasks = new List<Task<ListPage<T>>>();
            var totalPages = 0;
            var i = 1;
            do
            {
                pageTasks.Add(listFunc(i++));
                var running = pageTasks.Where(t => !t.IsCompleted && !t.IsFaulted).ToList();
                if(running.Count == 0 && pageTasks?.FirstOrDefault()?.Result?.Meta?.TotalPages != null)
                {
                    totalPages = pageTasks.FirstOrDefault().Result.Meta.TotalPages;
                }else if (totalPages == 0 || running.Count >= 16) // throttle parallel tasks at 16
                    totalPages = (await await Task.WhenAny(running)).Meta.TotalPages;  //Set total number of pages based on returned Meta.
            } while (i <= totalPages);
            var data = (
                from finalResult in await Task.WhenAll(pageTasks) //When all pageTasks are complete, save items in data variable.
                from item in finalResult.Items
                select item).ToList();
            return data;
        }

        public static async Task<List<T>> ListWithFacets<T>(Func<int, Task<ListPageWithFacets<T>>> listFunc)
        {
            var pageTasks = new List<Task<ListPageWithFacets<T>>>();
            var totalPages = 0;
            var i = 1;
            do
            {
                pageTasks.Add(listFunc(i++));
                var running = pageTasks.Where(t => !t.IsCompleted && !t.IsFaulted).ToList();
                if (totalPages == 0 || running.Count >= 16) // throttle parallel tasks at 16
                    totalPages = (await await Task.WhenAny(running)).Meta.TotalPages;  //Set total number of pages based on returned Meta.
            } while (i <= totalPages);
            var data = (
                from finalResult in await Task.WhenAll(pageTasks) //When all pageTasks are complete, save items in data variable.
                from item in finalResult.Items
                select item).ToList();
            return data;
        }
    }


}
