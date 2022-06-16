using System;
using System.IO;
using System.Threading.Tasks;
using OrderCloud.Integrations.AzureStorage;
using OrderCloud.Integrations.EnvironmentSeed.Models;

namespace OrderCloud.Integrations.EnvironmentSeed.Commands
{
    public interface IUploadTranslationsCommand
    {
        Task UploadTranslationsFiles();

        Task UploadTranslationsFile(UploadTranslationsRequest translationsRequest);
    }

    public class UploadTranslationsCommand : IUploadTranslationsCommand
    {
        private readonly ICloudBlobService cloudBlobService;

        public UploadTranslationsCommand(ICloudBlobService cloudBlobService)
        {
            this.cloudBlobService = cloudBlobService;
        }

        public async Task UploadTranslationsFiles()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"wwwroot\i18n"));
            var files = Directory.GetFiles(path, "*.json");

            foreach (var file in files)
            {
                await cloudBlobService.Save($"i18n/{Path.GetFileName(file)}", File.ReadAllText(file));
            }
        }

        public async Task UploadTranslationsFile(UploadTranslationsRequest translationsRequest)
        {
            await cloudBlobService.Save($"i18n/{translationsRequest.LanguageCode}.json", translationsRequest.TranslationsFile);
        }
    }
}
