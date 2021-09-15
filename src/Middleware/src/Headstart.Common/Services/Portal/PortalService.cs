using System;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Headstart.Common.Services.Portal.Models;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

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
        private readonly IFlurlClient _client;
        private readonly AppSettings _settings;

        public PortalService(AppSettings settings, IFlurlClientFactory flurlFactory)
        {
            _settings = settings;
            _client = flurlFactory.Get("https://portal.ordercloud.io/api/v1");
        }

        public async Task<string> Login(string username, string password)
        {
            try
            {
                var response = await _client.Request("oauth", "token")
                        .PostUrlEncodedAsync(new
                        {
                            grant_type = "password",
                            username = username,
                            password = password
                        }).ReceiveJson<PortalAuthResponse>();

                return response.access_token;
            } catch(FlurlHttpException ex)
            {
                throw new CatalystBaseException(
                    ex.Call.Response.StatusCode.ToString(),
                    "Error logging in to portal. Please make sure your username and password are correct");
            }
        }

        public async Task<PortalUser> GetMe(string token)
        {
            return await _client.Request("me")
                        .WithOAuthBearerToken(token)
                        .GetJsonAsync<PortalUser>();
        }

        public async Task<Marketplace> GetMarketplace(string marketplaceID, string token)
        {
            return await _client.Request("organizations", marketplaceID)
                        .WithOAuthBearerToken(token)
                        .GetJsonAsync<Marketplace>();
        }

        // The portal API allows you to get an admin token for that marketplace that isn't related to any user
        // and the roles granted are roles defined for the dev user. If you're the owner, that is full access
        public async Task<string> GetMarketplaceToken(string marketplaceID, string token)
        {
            var request = await _client.Request("organizations", marketplaceID, "token")
                            .WithOAuthBearerToken(token)
                            .GetJsonAsync<MarketplaceTokenResponse>();

            return request.access_token;
        }

        public async Task CreateMarketplace(Marketplace marketplace, string token)
        {
            //  doesn't return anything
            await _client.Request($"organizations/{marketplace.Id}")
                .WithOAuthBearerToken(token)
                .PutJsonAsync(marketplace);
        }
    }
}
