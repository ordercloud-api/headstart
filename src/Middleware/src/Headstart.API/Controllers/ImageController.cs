using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using Headstart.Common.Services.CMS;
using Headstart.Common.Services.CMS.Models;

namespace Headstart.API.Controllers
{
    [Route("images")]
    public class ImageController : BaseController
    {
        private readonly IImageClient _command;
        public ImageController(IImageClient command)
        {
            _command = command;
        }
        [DocName("POST Image")]
        [HttpPost, Route(""), OrderCloudUserAuth()]
        public async Task<ImageUrls> CreateImage([FromForm] AssetUpload asset)
        {
            return await _command.CreateImage(asset);
        }
    }
}
