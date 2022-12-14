namespace OrderCloud.Integrations.AzureStorage
{
    public class StorageAccountSettings
    {
        /// <summary>
        /// Connection string for the storage account. Found in Azure Storage Account under "Access Keys". - Required for running Headstart.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Url for the blob storage that is hosted in Azure. Found in Storage Account > Endpoints > Primary endpoint. Format is https://{STORAGEACCOUNT_NAME}.blob.core.windows.net/                                                 |
        /// </summary>
        public string BlobPrimaryEndpoint { get; set; }

        /// <summary>
        /// Unique name for the blob container housing data for Azure Queues (Optional, defaults to "queue") [Microsoft Reference](https://docs.microsoft.com/en-us/rest/api/storageservices/naming-queues-and-metadata)
        /// </summary>
        public string BlobContainerNameQueue { get; set; } = "queue";

        /// <summary>
        /// Unique name for the blob container housing data for exchange rates (Optional, defaults to "currency").
        /// </summary>
        public string BlobContainerNameExchangeRates { get; set; } = "currency";

        /// <summary>
        /// Unique name for the blob container housing data for translations (Optional, defaults to ngx-translate).
        /// </summary>
        public string BlobContainerNameTranslations { get; set; } = "ngx-translate";
    }
}
