using Headstart.Common.Services.CMS.Models;
using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Common.Services.CMS
{
    public interface IImageClient
    {
        Task<string> SaveProductImage(AssetUpload asset);
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

        public async Task<string> SaveProductImage(AssetUpload asset)
        {
            var container = _blob.Container.Name;
            var assetGuid = Guid.NewGuid().ToString();

            await _blob.Save(Guid.NewGuid().ToString(), asset.File, "image/jpeg");
            return $"{_settings.BlobSettings.HostUrl}/{container}/{assetGuid}";
        }
    }
}
