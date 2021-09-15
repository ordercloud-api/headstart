using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using Headstart.Common.Services.CMS;
using Headstart.Common.Services.CMS.Models;
using Headstart.Models;

namespace Headstart.API.Controllers
{
    [Route("assets")]
    public class AssetController : CatalystController
    {
        private readonly IAssetClient _command;
        public AssetController(IAssetClient command)
        {
            _command = command;
        }

        /// <summary>
        /// Create Image
        /// </summary>
        [HttpPost, Route("image"), OrderCloudUserAuth()]
        public async Task<ImageAsset> CreateImage([FromForm] AssetUpload asset)
        {
            return await _command.CreateImage(asset);
        }

        /// <summary>
        /// Delete Asset
        /// </summary>
        [HttpDelete, Route("{id}"), OrderCloudUserAuth()]
        public async Task DeleteImage(string id)
        {
            await _command.DeleteAsset(id);
        }

        /// <summary>
        /// Create Document
        /// </summary>
        [HttpPost, Route("document"), OrderCloudUserAuth()]
        public async Task<DocumentAsset> CreateDocument([FromForm] AssetUpload asset)
        {
            return await _command.CreateDocument(asset);
        }
    }
}
