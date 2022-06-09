using System.Collections.Generic;
using System.Linq;

namespace OrderCloud.Integrations.CosmosDB.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunksize)
                .Select(x => x.Select(v => v.Value));
        }
    }
}
