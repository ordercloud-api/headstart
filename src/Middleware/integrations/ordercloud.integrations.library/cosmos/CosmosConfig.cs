using System;

namespace ordercloud.integrations.library
{
    public class CosmosConfig
    {
        public CosmosConfig()
        {
        }

        public CosmosConfig(
            string db, 
            string uri, 
            string key, 
            int requestTimeoutInSeconds
        )
        {
            this.DatabaseName = db;
            this.EndpointUri = uri;
            this.PrimaryKey = key;
            this.RequestTimeout = TimeSpan.FromSeconds(requestTimeoutInSeconds);

        }
        public string DatabaseName { get; set; }
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public TimeSpan RequestTimeout { get; set; }
    }
}
