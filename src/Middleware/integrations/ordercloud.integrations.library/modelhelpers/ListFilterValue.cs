using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ListFilterValue
    {
        public string Term { get; set; } = "";
        public ListFilterOperator Operator { get; set; } = ListFilterOperator.Equal;
        public IList<int> WildcardPositions { get; set; } = new List<int>();
        public bool HasWildcard => WildcardPositions.Any();

    }
}
