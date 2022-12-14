using System;
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
            if (string.IsNullOrEmpty(storageAccountSettings.ConnectionString) || string.IsNullOrEmpty(storageAccountSettings.BlobPrimaryEndpoint))
            {
                // Storage account is used for saving files (product images, product documents, translations)
                throw new Exception("Required app settings are missing: StorageAccountSettings:ConnectionString or StorageAccountSettings:BlobPrimaryEndpoint");
            }

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
