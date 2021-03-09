using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models.Misc;
using ordercloud.integrations.library;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
    public class EnvironmentSeedController : BaseController
    {
        private readonly IEnvironmentSeedCommand _command;

        public EnvironmentSeedController(
            IEnvironmentSeedCommand command
        )
        {
            _command = command;
        }

        [HttpPost, Route("seed")]
        public async Task<EnvironmentSeedResponse> Seed([FromBody] EnvironmentSeed seed)
        {
            return await _command.Seed(seed);
        }

		[HttpPost, Route("post-staging-restore"), OrderCloudWebhookAuth]
		public async Task PostStagingRestore()
		{
			await _command.PostStagingRestore();
		}
	}
}
