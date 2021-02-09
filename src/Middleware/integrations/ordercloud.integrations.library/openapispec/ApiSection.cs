using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ApiSection
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public IList<string> Description { get; set; }
        public IList<ApiResource> Resources { get; set; }
    }
}
