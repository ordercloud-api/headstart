using System;
using System.Drawing;
using Headstart.Models;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Headstart.Common.Extensions;
using ordercloud.integrations.library;
using Headstart.Common.Services.CMS.Models;

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
        private readonly IOrderCloudIntegrationsBlobService _blob;
        private readonly AppSettings _settings;

        public AssetClient(IOrderCloudIntegrationsBlobService blob, AppSettings settings)
        {
            _blob = blob;
            _settings = settings;
        }

        public async Task<ImageAsset> CreateImage(AssetUpload asset)
        {
            string container = _blob.Container.Name;
            string assetGuid = Guid.NewGuid().ToString();

            using (Image image = Image.FromStream(asset.File.OpenReadStream()))
            {
                Image small = image.ResizeSmallerDimensionToTarget(100);
                Image medium = image.ResizeSmallerDimensionToTarget(300);
                await Task.WhenAll(new[] {
                    _blob.Save(assetGuid, medium.ToBytes(ImageFormat.Png), $@"image/png"),
                    _blob.Save($@"{assetGuid}-s", small.ToBytes(ImageFormat.Png), $@"image/png")
                });
            }
            return new ImageAsset
            {
                Url = $@"{GetBaseUrl()}{container}/{assetGuid}",
                ThumbnailUrl = $@"{GetBaseUrl()}{container}/{assetGuid}-s"
            };
        }

        public async Task<DocumentAsset> CreateDocument(AssetUpload asset)
        {
            string container = _blob.Container.Name;
            string assetGuid = Guid.NewGuid().ToString();
            await _blob.Save(assetGuid, asset.File, $@"application/pdf");
            return new DocumentAsset()
            {
                FileName = asset.Filename,
                Url = $@"{GetBaseUrl()}{container}/{assetGuid}"
            };
        }

        public async Task DeleteAsset(string id)
        {
            await _blob.Delete(id);
            try
            {
                await _blob.Delete($@"{id}-s");
            }
            catch { }
        }

        public async Task DeleteAssetByUrl(string assetUrl)
        {
            string id = GetAssetIDFromUrl(assetUrl);
            await _blob.Delete(id);
            try
            {
                await _blob.Delete($@"{id}-s");
            }
            catch { }
        }

        public string GetAssetIDFromUrl(string url)
        {
            string[] parts = url.Split($@"/");
            return parts[parts.Length - 1];
        }

        private string GetBaseUrl()
        {
            return _settings.StorageAccountSettings.BlobPrimaryEndpoint.EndsWith("/") ? _settings.StorageAccountSettings.BlobPrimaryEndpoint : _settings.StorageAccountSettings.BlobPrimaryEndpoint + "/";
        }
    }
}