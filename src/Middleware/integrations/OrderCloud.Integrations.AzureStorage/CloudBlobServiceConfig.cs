using Microsoft.WindowsAzure.Storage.Blob;

namespace OrderCloud.Integrations.AzureStorage
{
    public class CloudBlobServiceConfig
    {
        public string ConnectionString { get; set; }

        public string Container { get; set; }

        public BlobContainerPublicAccessType AccessType { get; set; }
    }
}
