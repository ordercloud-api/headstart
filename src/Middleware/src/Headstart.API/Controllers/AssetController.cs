using System.Threading.Tasks;
using Headstart.Common.Services.CMS;
using Headstart.Common.Services.CMS.Models;
using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;

namespace Headstart.API.Controllers
{
    [Route("assets")]
    public class AssetController : CatalystController
    {
        private readonly IAssetClient command;

        public AssetController(IAssetClient command)
        {
            this.command = command;
        }

        /// <summary>
        /// Create Image.
        /// </summary>
        [HttpPost, Route("image"), OrderCloudUserAuth]
        public async Task<ImageAsset> CreateImage([FromForm] AssetUpload asset)
        {
            return await command.CreateImage(asset);
        }

        /// <summary>
        /// Delete Asset.
        /// </summary>
        [HttpDelete, Route("{id}"), OrderCloudUserAuth]
        public async Task DeleteImage(string id)
        {
            await command.DeleteAsset(id);
        }

        /// <summary>
        /// Create Document.
        /// </summary>
        [HttpPost, Route("document"), OrderCloudUserAuth]
        public async Task<DocumentAsset> CreateDocument([FromForm] AssetUpload asset)
        {
            return await command.CreateDocument(asset);
        }
    }
}
