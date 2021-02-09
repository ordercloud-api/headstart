using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models.Misc;
using ordercloud.integrations.library;
using Headstart.API.Controllers;
using Headstart.API.Commands;

namespace Headstart.Common.Controllers
{
    public class EnvironmentSeedController : BaseController
    {
        private readonly IEnvironmentSeedCommand _command;

        public EnvironmentSeedController(
            AppSettings settings,
            IEnvironmentSeedCommand command
        ) : base(settings)
        {
            _command = command;
        }

        [HttpPost, Route("seed")]
        public async Task Seed([FromBody] EnvironmentSeed seed)
        {
            await _command.Seed(seed);
        }

		[HttpPost, Route("post-staging-restore"), OrderCloudWebhookAuth]
		public async Task PostStagingRestore()
		{
			await _command.PostStagingRestore();
		}
	}
}
