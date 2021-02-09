namespace ordercloud.integrations.library
{
    public class BlobSettings
    {
        public string ConnectionString { get; set; }
        public string ContainerNameQueue { get; set; } = "queue";
        public string ContainerNameCache { get; set; } = "cache";
        public string ContainerNameExchangeRates { get; set; } = "currency";
        public string ContainerNameTranslations { get; set; } = "ngx-translate";
        public string HostUrl { get; set; }
        public string EnvironmentString { get; set; }
    }
}
