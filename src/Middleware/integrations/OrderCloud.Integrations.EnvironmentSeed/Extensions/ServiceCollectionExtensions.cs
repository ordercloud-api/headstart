using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Blob;
using OrderCloud.Integrations.AzureStorage;
using OrderCloud.Integrations.EnvironmentSeed.Commands;

namespace OrderCloud.Integrations.EnvironmentSeed.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultTranslationsProvider(this IServiceCollection services, StorageAccountSettings storageAccountSettings)
        {
            var translationsConfig = new CloudBlobServiceConfig()
            {
                ConnectionString = storageAccountSettings.ConnectionString,
                Container = storageAccountSettings.BlobContainerNameTranslations,
                AccessType = BlobContainerPublicAccessType.Container,
            };

            services.AddSingleton<IUploadTranslationsCommand>(provider => new UploadTranslationsCommand(new CloudBlobService(translationsConfig)));

            return services;
        }
    }
}
