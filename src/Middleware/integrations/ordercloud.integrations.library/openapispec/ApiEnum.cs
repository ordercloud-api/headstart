using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ApiEnum
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public string Description { get; set; }
        public IList<ApiEnumValue> Values { get; set; }
    }
}
