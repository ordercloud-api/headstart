using Microsoft.WindowsAzure.Storage.Blob;

namespace ordercloud.integrations.library
{
	public class BlobServiceConfig
	{
		public string ConnectionString { get; set; }
		public string Container { get; set; }
		public BlobContainerPublicAccessType AccessType { get; set; }

	}
}
