using Microsoft.WindowsAzure.Storage.Blob;

namespace Sitecore.Foundation.SitecoreExtensions.MVC.Extensions
{
	public class AppEnvSettingsModel : BaseCommonSettingsModel
	{
		public bool IsNonProdEnv { get; set; }
	}

	public class BaseCommonSettingsModel
	{
		public bool EnableCustomFileLogging { get; set; } = false;

		public string AppLogFileKey { get; set; } = "CustomApiLogs";

		public string ConnectionString { get; set; } = string.Empty;
	}

	public class LogSettings : BaseCommonSettingsModel
	{
		public string Container { get; set; } = string.Empty;
	}

	public class BlobServiceConfig : BaseCommonSettingsModel
	{
		public string Container { get; set; } = string.Empty;

		public BlobContainerPublicAccessType AccessType { get; set; }
	}
}