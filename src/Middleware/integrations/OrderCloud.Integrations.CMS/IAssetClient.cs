using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Integrations.CMS.Models;
using OrderCloud.Integrations.AzureStorage;

namespace Headstart.Integrations.CMS
{
    public interface IAssetClient
    {
        Task<ImageAsset> CreateImage(AssetUpload asset);

        Task DeleteAsset(string id);

        Task<DocumentAsset> CreateDocument(AssetUpload asset);

        Task DeleteAssetByUrl(string assetUrl);
    }

    public class AssetClient : IAssetClient
    {
        private readonly ICloudBlobService cloudBlobService;
        private readonly StorageAccountSettings storageAccountSettings;

        public AssetClient(ICloudBlobService blob, StorageAccountSettings storageAccountSettings)
        {
            this.cloudBlobService = blob;
            this.storageAccountSettings = storageAccountSettings;
        }

        public async Task<ImageAsset> CreateImage(AssetUpload asset)
        {
            var container = cloudBlobService.Container.Name;
            var assetGuid = Guid.NewGuid().ToString();

            using (var image = Image.FromStream(asset.File.OpenReadStream()))
            {
                var small = image.ResizeSmallerDimensionToTarget(100);
                var medium = image.ResizeSmallerDimensionToTarget(300);
                await Task.WhenAll(new[]
                {
                    cloudBlobService.Save(assetGuid, medium.ToBytes(ImageFormat.Png), "image/png"),
                    cloudBlobService.Save($"{assetGuid}-s", small.ToBytes(ImageFormat.Png), "image/png"),
                });
            }

            return new ImageAsset
            {
                Url = $"{GetBaseUrl()}{container}/{assetGuid}",
                ThumbnailUrl = $"{GetBaseUrl()}{container}/{assetGuid}-s",
            };
        }

        public async Task<DocumentAsset> CreateDocument(AssetUpload asset)
        {
            var container = cloudBlobService.Container.Name;
            var assetGuid = Guid.NewGuid().ToString();
            await cloudBlobService.Save(assetGuid, asset.File, "application/pdf");
            return new DocumentAsset()
            {
                FileName = asset.Filename,
                Url = $"{GetBaseUrl()}{container}/{assetGuid}",
            };
        }

        public async Task DeleteAsset(string id)
        {
            await cloudBlobService.Delete(id);
            try
            {
                await cloudBlobService.Delete($"{id}-s");
            }
            catch
            {
            }
        }

        public async Task DeleteAssetByUrl(string assetUrl)
        {
            var id = GetAssetIDFromUrl(assetUrl);
            await cloudBlobService.Delete(id);
            try
            {
                await cloudBlobService.Delete($"{id}-s");
            }
            catch
            {
            }
        }

        public string GetAssetIDFromUrl(string url)
        {
            var parts = url.Split("/");
            return parts[parts.Length - 1];
        }

        private string GetBaseUrl()
        {
            return storageAccountSettings.BlobPrimaryEndpoint.EndsWith("/") ? storageAccountSettings.BlobPrimaryEndpoint : storageAccountSettings.BlobPrimaryEndpoint + "/";
        }
    }
}
