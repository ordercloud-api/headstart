﻿using Headstart.API.Commands;
using Headstart.Common;
using Headstart.Models.Misc;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using System.Threading.Tasks;

namespace Headstart.API.Controllers
{
	public class EnvironmentSeedController : CatalystController
	{
		private readonly IEnvironmentSeedCommand _command;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the EnvironmentSeedController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		/// <param name="settings"></param>
		public EnvironmentSeedController(IEnvironmentSeedCommand command, AppSettings settings)
		{
			_command = command;
			_settings = settings;
		}

		/// <summary>
		/// Seeding of a brand new organization (POST method)
		/// </summary>
		/// <remarks>
		/// Check out the readme for more info https://github.com/ordercloud-api/headstart#seeding-ordercloud-data
		/// </remarks>
		/// <param name="seed"></param>
		/// <returns>The response from the Seeding of a brand new organization</returns>
		[HttpPost, Route("seed")]
		public async Task<EnvironmentSeedResponse> Seed([FromBody] EnvironmentSeed seed)
		{
			return await _command.Seed(seed);
		}

		/// <summary>
		/// Call this endpoint to update translation files from source
		/// useful if pulling in updates from repo and only want to update translation files
		/// assumes storage account connection string is in settings (PUT method)
		/// </summary>
		/// <returns></returns>
		[HttpPut, Route("updatetranslations")]
		public async Task UpdateTranslations()
		{
			await _command.UpdateTranslations(
				_settings.StorageAccountSettings.ConnectionString,
				_settings.StorageAccountSettings.BlobContainerNameTranslations
			);
		}

		/// <summary>
		/// Posts the Staging Restore action
		/// </summary>
		/// <returns></returns>
		[HttpPost, Route("post-staging-restore"), OrderCloudWebhookAuth]
		public async Task PostStagingRestore()
		{
			if (_settings.EnvironmentSettings.Environment == AppEnvironment.Production)
			{
				return;
			}
			await _command.PostStagingRestore();
		}
	}
}