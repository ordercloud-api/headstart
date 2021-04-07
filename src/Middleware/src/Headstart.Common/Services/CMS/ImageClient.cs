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
    }

    public class ImageClient : IImageClient
    {
        private readonly IOrderCloudIntegrationsBlobService _blob;
        private readonly AppSettings _settings;

        public ImageClient(IOrderCloudIntegrationsBlobService blob, AppSettings settings)
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
                ImageUrl = $"{_settings.BlobSettings.HostUrl}/{container}/{assetGuid}",
                ThumbnailUrl = $"{_settings.BlobSettings.HostUrl}/{container}/{assetGuid}-s"
            };
        }
    }
}
