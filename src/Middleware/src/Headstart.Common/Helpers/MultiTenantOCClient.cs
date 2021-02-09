using System;
using System.Collections.Generic;
using System.Text;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Common.Helpers
{
    /// <summary>A version of OrderCloudClient that takes all of its config data from an Ordercloud token.
    /// It can be used to support multi-tenancy because it can remove the need to save OC credentials in config settings.
    /// </summary>
    public class MultiTenantOCClient : OrderCloudClient, IOrderCloudClient
    {
        public MultiTenantOCClient(VerifiedUserContext user) : base(
            new OrderCloudClientConfig()
            {
                ApiUrl = user.ApiUrl,
                AuthUrl = user.AuthUrl,
                ClientId = user.ClientID,
                Roles = new[] { ApiRole.FullAccess }
            }
        )
        {
            TokenResponse = new TokenResponse()
            {
                AccessToken = user.AccessToken,
                ExpiresUtc = user.AccessTokenExpiresUTC
            };
        }

        public MultiTenantOCClient(string token, string apiUrl, string authUrl, string clientID, DateTime tokenExpiresUTC) : base(
            new OrderCloudClientConfig()
            {
                ApiUrl = apiUrl,
                AuthUrl = authUrl,
                ClientId = clientID,
                Roles = new[] { ApiRole.FullAccess }
            }
        )
        {
            TokenResponse = new TokenResponse()
            {
                AccessToken = token,
                ExpiresUtc = tokenExpiresUTC
            };
        }
    }
}
