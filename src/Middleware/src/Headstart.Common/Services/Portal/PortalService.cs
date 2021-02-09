using System;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Headstart.Common.Services.Portal.Models;

namespace Headstart.Common.Services
{
    public interface IPortalService
    {
        Task<string> Login(string username, string password);
        Task<string> GetOrgToken(string orgID, string token);
        Task<PortalUser> GetMe(string token);
        Task CreateOrganization(Organization org, string token);
        Task<Organization> GetOrganization(string orgID, string token);
    }

    public class PortalService : IPortalService
    {
        private readonly IFlurlClient _client;
        private readonly AppSettings _settings;

        public PortalService(AppSettings settings, IFlurlClientFactory flurlFactory)
        {
            _settings = settings;
            _client = flurlFactory.Get("https://portal.ordercloud.io/api/v1/");
        }

        public async Task<string> Login(string username, string password)
        {
            var response = await _client.Request("oauth", "token")
                        .PostUrlEncodedAsync(new
                        {
                            grant_type = "password",
                            username = username,
                            password = password
                        }).ReceiveJson<PortalAuthResponse>();

            return response.access_token;
        }

        public async Task<PortalUser> GetMe(string token)
        {
            return await _client.Request("me")
                        .WithOAuthBearerToken(token)
                        .GetJsonAsync<PortalUser>();
        }

        public async Task<Organization> GetOrganization(string orgID, string token)
        {
            return await _client.Request("organizations", orgID)
                        .WithOAuthBearerToken(token)
                        .GetJsonAsync<Organization>();
        }

        // The portal API allows you to get an admin token for that org that isn't related to any user
        // and the roles granted are roles defined for the dev user. If you're the owner, that is full access
        public async Task<string> GetOrgToken(string orgID, string token)
        {
            var request = await _client.Request("organizations", orgID, "token")
                            .WithOAuthBearerToken(token)
                            .GetJsonAsync<OrgTokenResponse>();

            return request.access_token;
        }

        public async Task CreateOrganization(Organization org, string token)
        {
            // doesn't return anything
            await _client.Request("organizations", token)
                .PutJsonAsync(org);
        }
    }
}
