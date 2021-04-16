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
    public class AssetController : BaseController
    {
        private readonly IAssetClient _command;
        public AssetController(IAssetClient command)
        {
            _command = command;
        }
        [DocName("POST Image")]
        [HttpPost, Route("image"), OrderCloudUserAuth()]
        public async Task<ImageAsset> CreateImage([FromForm] AssetUpload asset)
        {
            return await _command.CreateImage(asset);
        }

        [DocName("DELETE Asset")]
        [HttpDelete, Route("{id}"), OrderCloudUserAuth()]
        public async Task DeleteImage(string id)
        {
            await _command.DeleteAsset(id);
        }

        [DocName("Post Document")]
        [HttpPost, Route("document"), OrderCloudUserAuth()]
        public async Task<DocumentAsset> CreateDocument([FromForm] AssetUpload asset)
        {
            return await _command.CreateDocument(asset);
        }
    }
}
