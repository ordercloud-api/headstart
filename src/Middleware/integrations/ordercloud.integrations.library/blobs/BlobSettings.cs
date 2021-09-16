namespace ordercloud.integrations.library
{
    public class BlobSettings
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public string CacheName { get; set; }
        public string HostUrl { get; set; }
        public string EnvironmentString { get; set; }
    }
}
