using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models.Misc;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
    public class EnvironmentSeedController : BaseController
    {
        private readonly IEnvironmentSeedCommand _command;
        private readonly AppSettings _settings;

        public EnvironmentSeedController(
            IEnvironmentSeedCommand command,
            AppSettings settings
        )
        {
            _command = command;
            _settings = settings;
        }

        [HttpPost, Route("seed")]
        public async Task<EnvironmentSeedResponse> Seed([FromBody] EnvironmentSeed seed)
        {
            return await _command.Seed(seed);
        }

		[HttpPost, Route("post-staging-restore"), OrderCloudWebhookAuth]
		public async Task PostStagingRestore()
		{
            if(_settings.EnvironmentSettings.Environment == AppEnvironment.Production)
            {
                return;
            }
			await _command.PostStagingRestore();
		}
	}
}
