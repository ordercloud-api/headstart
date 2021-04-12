using Headstart.Common.Extensions;
using Headstart.Common.Services.CMS.Models;
using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Common.Services.CMS
{
    public interface IImageClient
    {
        Task<ImageUrls> CreateImage(AssetUpload asset);
        Task DeleteAsset(string id);
        Task<string> CreateDocument(AssetUpload asset);
    }

    public class IAssetClient : IImageClient
    {
        private readonly IOrderCloudIntegrationsBlobService _blob;
        private readonly AppSettings _settings;

        public IAssetClient(IOrderCloudIntegrationsBlobService blob, AppSettings settings)
        {
            _blob = blob;
            _settings = settings;
        }

        public async Task<ImageUrls> CreateImage(AssetUpload asset)
        {
            var container = _blob.Container.Name;
            var assetGuid = Guid.NewGuid().ToString();

            using(var image = Image.FromStream(asset.File.OpenReadStream()))
            {
                var small = image.ResizeSmallerDimensionToTarget(100);
                var medium = image.ResizeSmallerDimensionToTarget(300);
                try
                {
                    await Task.WhenAll(new[] {
                        _blob.Save(assetGuid, medium.ToBytes(ImageFormat.Png), "image/png"),
                        _blob.Save($"{assetGuid}-s", small.ToBytes(ImageFormat.Png), "image/png")
                    });
                } finally
                {
                    small.Dispose();
                    medium.Dispose();
                }
            }
            return new ImageUrls
            {
                Url = $"{GetBaseUrl()}{container}/{assetGuid}",
                ThumbnailUrl = $"{GetBaseUrl()}{container}/{assetGuid}-s"
            };
        }

        public async Task<string> CreateDocument(AssetUpload asset)
        {
            var container = _blob.Container.Name;
            var assetGuid = Guid.NewGuid().ToString();
            await _blob.Save(assetGuid, asset.File, "application/pdf");
            return $"{GetBaseUrl()}{container}/{assetGuid}";
        }

        public async Task DeleteAsset(string id)
        {
            await _blob.Delete(id);
            try
            {
                await _blob.Delete($"{id}-s");
            } catch { }
        }

        private string GetBaseUrl()
        {
            return _settings.BlobSettings.HostUrl.EndsWith("/") ? _settings.BlobSettings.HostUrl : _settings.BlobSettings.HostUrl + "/";
        } 
    }
}
