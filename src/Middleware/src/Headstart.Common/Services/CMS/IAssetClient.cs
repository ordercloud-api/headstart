using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Common.Services.CMS.Models;
using OrderCloud.Integrations.Library;

namespace Headstart.Common.Services.CMS
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
        private readonly IOrderCloudIntegrationsBlobService blob;
        private readonly StorageAccountSettings storageAccountSettings;

        public AssetClient(IOrderCloudIntegrationsBlobService blob, StorageAccountSettings storageAccountSettings)
        {
            this.blob = blob;
            this.storageAccountSettings = storageAccountSettings;
        }

        public async Task<ImageAsset> CreateImage(AssetUpload asset)
        {
            var container = blob.Container.Name;
            var assetGuid = Guid.NewGuid().ToString();

            using (var image = Image.FromStream(asset.File.OpenReadStream()))
            {
                var small = image.ResizeSmallerDimensionToTarget(100);
                var medium = image.ResizeSmallerDimensionToTarget(300);
                await Task.WhenAll(new[]
                {
                    blob.Save(assetGuid, medium.ToBytes(ImageFormat.Png), "image/png"),
                    blob.Save($"{assetGuid}-s", small.ToBytes(ImageFormat.Png), "image/png"),
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
            var container = blob.Container.Name;
            var assetGuid = Guid.NewGuid().ToString();
            await blob.Save(assetGuid, asset.File, "application/pdf");
            return new DocumentAsset()
            {
                FileName = asset.Filename,
                Url = $"{GetBaseUrl()}{container}/{assetGuid}",
            };
        }

        public async Task DeleteAsset(string id)
        {
            await blob.Delete(id);
            try
            {
                await blob.Delete($"{id}-s");
            }
            catch
            {
            }
        }

        public async Task DeleteAssetByUrl(string assetUrl)
        {
            var id = GetAssetIDFromUrl(assetUrl);
            await blob.Delete(id);
            try
            {
                await blob.Delete($"{id}-s");
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
