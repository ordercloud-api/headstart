namespace OrderCloud.Integrations.AzureStorage
{
    public class StorageAccountSettings
    {
        public string ConnectionString { get; set; }

        public string BlobContainerNameQueue { get; set; } = "queue";

        public string BlobContainerNameCache { get; set; } = "cache";

        public string BlobContainerNameExchangeRates { get; set; } = "currency";

        public string BlobContainerNameTranslations { get; set; } = "ngx-translate";

        public string BlobPrimaryEndpoint { get; set; }
    }
}
