using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ordercloud.integrations.library
{
    public static class Throttler
    {
        /// <summary>
        /// Concurrently run a task for each data item. Like Task.WhenAll, except you can cap the number allowed to run at a time,
        /// and enforce a minimum pause between the start of each.
        /// ex usage:
        /// await Throttler.RunAsync(assignmentList, 100, 100, assignment => _oc.Catalogs.SaveProductAssignmentAsync(assignment));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">A list of data items.</param>
        /// <param name="minPause">Minimum wait time in milliseconds between starting tasks.</param>
        /// <param name="maxConcurrent">Maximum tasks allowed to run at the same time.</param>
        /// <param name="op">An async operation to perform on each data item.</param>
        /// <returns></returns>
        public static async Task RunAsync<T>(IEnumerable<T> items, int minPause, int maxConcurrent, Func<T, Task> op)
        {
            using (var sem = new SemaphoreSlim(maxConcurrent))
            {
                async Task RunOneAsync(T item)
                {
                    try { await op(item); }
                    finally { sem.Release(); }
                }

                var tasks = new List<Task>();
                foreach (var item in items)
                {
                    if (tasks.Any()) // avoid pausing before the first one
                        await Task.WhenAll(sem.WaitAsync(), Task.Delay(minPause)); // wait until we're under the concurrency limit AND at least minPause has passed
                    tasks.Add(RunOneAsync(item));
                }
                await Task.WhenAll(tasks);
            }
        }

        public static async Task<IList<TOutput>> RunAsync<TInput, TOutput>(IEnumerable<TInput> items, int minPause, int maxConcurrent, Func<TInput, Task<TOutput>> op)
        {
            using (var sem = new SemaphoreSlim(maxConcurrent))
            {
                async Task<TOutput> RunOneAsync(TInput item)
                {
                    try { return await op(item); }
                    finally { sem.Release(); }
                }

                var tasks = new List<Task<TOutput>>();
                foreach (var item in items)
                {
                    if (tasks.Any()) // avoid pausing before the first one
                        await Task.WhenAll(sem.WaitAsync(), Task.Delay(minPause)); // wait until we're under the concurrency limit AND at least minPause has passed
                    tasks.Add(RunOneAsync(item));
                }

                var result = await Task.WhenAll(tasks);
                return result.ToList();
            }
        }
    }
}
