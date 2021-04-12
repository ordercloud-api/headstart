using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using Headstart.Common.Services.CMS;
using Headstart.Common.Services.CMS.Models;

namespace Headstart.API.Controllers
{
    [Route("assets")]
    public class AssetController : BaseController
    {
        private readonly IImageClient _command;
        public AssetController(IImageClient command)
        {
            _command = command;
        }
        [DocName("POST Image")]
        [HttpPost, Route("image"), OrderCloudUserAuth()]
        public async Task<ImageUrls> CreateImage([FromForm] AssetUpload asset)
        {
            return await _command.CreateImage(asset);
        }

        [DocName("DELETE Asset")]
        [HttpDelete, Route("asset/{id}"), OrderCloudUserAuth()]
        public async Task DeleteImage(string id)
        {
            await _command.DeleteAsset(id);
        }

        [DocName("Post Document")]
        [HttpPost, Route("document"), OrderCloudUserAuth()]
        public async Task<string> CreateDocument([FromForm] AssetUpload asset)
        {
            return await _command.CreateDocument(asset);
        }
    }
}
