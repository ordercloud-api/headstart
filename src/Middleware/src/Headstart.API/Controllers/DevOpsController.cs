using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.EnvironmentSeed.Commands;
using OrderCloud.Integrations.EnvironmentSeed.Models;

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
        /// Call this endpoint to upload translation files from source.
        /// </summary>
        [HttpPut, Route("translations")]
        public async Task UploadTranslations([FromForm] UploadTranslationsRequest translationsRequest)
        {
            await command.UploadTranslationsFile(translationsRequest);
        }
    }
}
