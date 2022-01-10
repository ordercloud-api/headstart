using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models.Misc;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
    public class EnvironmentSeedController : CatalystController
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

        /// <summary>
        /// Seeds a brand new organization
        /// </summary>
        /// <remarks>
        /// Check out the readme for more info https://github.com/ordercloud-api/headstart#seeding-ordercloud-data
        /// </remarks>
        [HttpPost, Route("seed")]
        public async Task<EnvironmentSeedResponse> Seed([FromBody] EnvironmentSeed seed)
        {
            return await _command.Seed(seed);
        }

        /// <summary>
        /// Call this endpoint to update translation files from source
        /// useful if pulling in updates from repo and only want to update translation files
        /// assumes storage account connection string is in settings
        /// </summary>
        [HttpPut, Route("updatetranslations")]
        public async Task UpdateTranslations()
        {
            await _command.UpdateTranslations(
                _settings.StorageAccountSettings.ConnectionString,
                _settings.StorageAccountSettings.BlobContainerNameTranslations
            );
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
