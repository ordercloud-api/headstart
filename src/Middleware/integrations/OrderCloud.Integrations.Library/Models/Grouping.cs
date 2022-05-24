using System.Collections.Generic;
using System.Linq;

namespace OrderCloud.Integrations.Library.Models
{
    public class Grouping<TKey, TElement> : List<TElement>, IGrouping<TKey, TElement>
    {
        public Grouping(TKey key)
            : base() => Key = key;

        public Grouping(TKey key, int capacity)
            : base(capacity) => Key = key;

        public Grouping(TKey key, IEnumerable<TElement> collection)
            : base(collection) => Key = key;

        public TKey Key { get; private set; }
    }
}
