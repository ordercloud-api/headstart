using Headstart.Common.Settings;
using Headstart.Integrations.CMS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.WindowsAzure.Storage.Blob;
using OrderCloud.Integrations.AzureStorage;

namespace OrderCloud.Integrations.CMS.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultCMSProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, StorageAccountSettings storageAccountSettings)
        {
            var assetConfig = new CloudBlobServiceConfig()
            {
                ConnectionString = storageAccountSettings.ConnectionString,
                Container = "assets",
                AccessType = BlobContainerPublicAccessType.Container,
            };

            services.TryAddSingleton<IAssetClient>(provider => new AssetClient(new CloudBlobService(assetConfig), storageAccountSettings));

            return services;
        }
    }
}
