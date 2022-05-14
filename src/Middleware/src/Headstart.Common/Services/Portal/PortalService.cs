using System;
using Flurl.Http;
using Flurl.Http.Configuration;
using Headstart.Common.Services.Portal.Models;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services
{
	public interface IPortalService
	{
		Task<string> Login(string username, string password);
		Task<string> GetMarketplaceToken(string marketplaceID, string token);
		Task<PortalUser> GetMe(string token);
		Task CreateMarketplace(Marketplace marketplace, string token);
		Task<Marketplace> GetMarketplace(string marketplaceID, string token);
	}

	public class PortalService : IPortalService
	{
		private readonly IFlurlClient client;
		private readonly AppSettings settings;

		/// <summary>
		/// The IOC based constructor method for the PortalService class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="flurlFactory"></param>
		public PortalService(AppSettings settings, IFlurlClientFactory flurlFactory)
		{
			try
			{
				this.settings = settings;
				client = flurlFactory.Get("https://portal.ordercloud.io/api/v1");
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable Login task method
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <returns>The string response value from the PortalService.Login process</returns>
		/// <exception cref="CatalystBaseException"></exception>
		public async Task<string> Login(string username, string password)
		{
			try
			{
                var response = await client.Request("oauth", "token")
					.PostUrlEncodedAsync(new
					{
						grant_type = "password",
						username = username,
                            password = password,
					}).ReceiveJson<PortalAuthResponse>();

				return response.access_token;
            }
            catch (FlurlHttpException ex)
			{
				throw new CatalystBaseException(
					ex.Call.Response.StatusCode.ToString(),
					"Error logging in to portal. Please make sure your username and password are correct");
			}
		}

		/// <summary>
		/// Public re-usable GetMe task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns>The PortalUser object value from the PortalService.GetMe process</returns>
		public async Task<PortalUser> GetMe(string token)
		{
            return await client.Request("me")
				.WithOAuthBearerToken(token)
				.GetJsonAsync<PortalUser>();
		}

		/// <summary>
		/// Public re-usable GetMarketplace task method
		/// </summary>
		/// <param name="marketplaceID"></param>
		/// <param name="token"></param>
		/// <returns>The Marketplace object value from the PortalService.GetMarketplace process</returns>
		public async Task<Marketplace> GetMarketplace(string marketplaceID, string token)
		{
            return await client.Request("organizations", marketplaceID)
				.WithOAuthBearerToken(token)
				.GetJsonAsync<Marketplace>();
		}

		/// <summary>
		/// Public re-usable GetMarketplaceToken task method
		/// The portal API allows you to get an admin token for that marketplace that isn't related to any user
		/// and the roles granted are roles defined for the dev user. If you're the owner, that is full access
		/// </summary>
		/// <param name="marketplaceID"></param>
		/// <param name="token"></param>
		/// <returns>The string response value from the PortalService.GetMarketplaceToken process</returns>
		public async Task<string> GetMarketplaceToken(string marketplaceID, string token)
		{
			var request = await client.Request("organizations", marketplaceID, "token")
				.WithOAuthBearerToken(token)
				.GetJsonAsync<MarketplaceTokenResponse>();

			return request.access_token;
		}

		/// <summary>
		/// Public re-usable CreateMarketplace task method
		/// </summary>
		/// <param name="marketplace"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task CreateMarketplace(Marketplace marketplace, string token)
		{
			//  doesn't return anything
			await client.Request($"organizations/{marketplace.Id}")
				.WithOAuthBearerToken(token)
				.PutJsonAsync(marketplace);
		}
	}
}
