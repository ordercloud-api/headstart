using System.Threading.Tasks;
using Headstart.Common.Settings;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.EnvironmentSeed.Commands;
using OrderCloud.Integrations.EnvironmentSeed.Models;

namespace Headstart.API.Controllers
{
    public class EnvironmentSeedController : CatalystController
    {
        private readonly IEnvironmentSeedCommand command;
        private readonly AppSettings settings;

        public EnvironmentSeedController(
            IEnvironmentSeedCommand command,
            AppSettings settings)
        {
            this.command = command;
            this.settings = settings;
        }

        /// <summary>
        /// Seeds a brand new organization.
        /// </summary>
        /// <remarks>
        /// Check out the readme for more info https://github.com/ordercloud-api/headstart#seeding-ordercloud-data.
        /// </remarks>
        [HttpPost, Route("seed")]
        public async Task<EnvironmentSeedResponse> Seed([FromBody] EnvironmentSeedRequest seed)
        {
            return await command.Seed(seed);
        }

        [HttpPost, Route("post-staging-restore"), OrderCloudWebhookAuth]
        public async Task PostStagingRestore()
        {
            if (settings.EnvironmentSettings.Environment == AppEnvironment.Production)
            {
                return;
            }

            await command.PostStagingRestore();
        }
    }
}
