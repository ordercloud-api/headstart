using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ApiResource
    {
        public Type ControllerType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<ApiEndpoint> Endpoints { get; set; }
        public IList<string> Comments { get; set; }
    }
}
