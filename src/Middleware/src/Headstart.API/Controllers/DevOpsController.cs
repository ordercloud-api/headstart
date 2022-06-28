using System.Threading.Tasks;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.EnvironmentSeed.Commands;

namespace Headstart.API.Controllers
{
    [Route("devops")]
    public class DevOpsController : CatalystController
    {
        private readonly IUploadTranslationsCommand command;

        public DevOpsController(IUploadTranslationsCommand command)
        {
            this.command = command;
        }

        /// <summary>
        /// Uploads translation files from the 'wwwroot/i18n' folder.
        /// </summary>
        [HttpPut, Route("translations")]
        public async Task UploadTranslations()
        {
            await command.UploadTranslationsFiles();
        }

        /// <summary>
        /// Uploads a translation file.
        /// </summary>
        [HttpPut, Route("translations/{languageCode}")]
        public async Task UploadTranslations([FromForm] FileUploadRequest fileUploadRequest, string languageCode)
        {
            await command.UploadTranslationsFile(languageCode, fileUploadRequest.File);
        }
    }
}
