using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class CosmosSettings
    {
        public string DatabaseName { get; set; }
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public int RequestTimeoutInSeconds { get; set; }
    }
}
