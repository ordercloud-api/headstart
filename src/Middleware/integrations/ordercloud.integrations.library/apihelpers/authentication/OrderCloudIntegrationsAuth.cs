using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ordercloud.integrations.library.extensions;
using OrderCloud.SDK;
using Flurl.Http;
using LazyCache;

namespace ordercloud.integrations.library
{
    public interface IApiAuthAttribute
    {
        ApiRole[] ApiRoles { get; }
    }

    public class OrderCloudIntegrationsAuthAttribute : AuthorizeAttribute, IApiAuthAttribute
    {
        public ApiRole[] ApiRoles { get; }
        /// <param name="roles">Optional list of roles. If provided, user must have just one of them, otherwise authorization fails.</param>
        public OrderCloudIntegrationsAuthAttribute(params ApiRole[] roles)
        {
            AuthenticationSchemes = "OrderCloudIntegrations";
            ApiRoles = roles.Append(ApiRole.FullAccess).ToArray(); // Full Access is always included in the list of roles that give access to a resource.  
            var rolesString = string.Join(",", ApiRoles);
            if (roles.Length == 0) rolesString += $",{OrderCloudIntegrationsAuthHandler.BaseUserRole}"; // If no roles are provided, auth succeeds.
            Roles = rolesString;
        }
    }

    public class OrderCloudIntegrationsAuthHandler : AuthenticationHandler<OrderCloudIntegrationsAuthOptions>
    {
        public const string BaseUserRole = "BaseUserRole"; // Everyone with a valid OC token has this role 

        private readonly IAppCache _cache;

        public OrderCloudIntegrationsAuthHandler(
            IOptionsMonitor<OrderCloudIntegrationsAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IAppCache cache) : base(options, logger, encoder, clock)
        {
            _cache = cache;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var token = GetTokenFromAuthHeader();

                if (string.IsNullOrEmpty(token))
                    return AuthenticateResult.Fail("The Headstart bearer token was not provided in the Authorization header.");

                var jwt = new JwtSecurityToken(token);
				var clientId = jwt.GetClientID();
				var usrtype = jwt.GetUserType();

                if (clientId == null)
                    return AuthenticateResult.Fail("The provided bearer token does not contain a 'cid' (Client ID) claim.");

                // we've validated the token as much as we can on this end, go make sure it's ok on OC

                var cid = new ClaimsIdentity("OrderCloudIntegrations");
                cid.AddClaim(new Claim("clientid", clientId));
                cid.AddClaim(new Claim("accesstoken", token));

                var allowFetchUserRetry = false;
                var user = await _cache.GetOrAddAsync(token, () => {
                    try {
                        // TODO: winmark calls this from other oc environments so we cant use sdk
                        // remove once winmark uses cms api
                        //var user = await Options.OrderCloudClient.Me.GetAsync(token);
                        return $"{jwt.GetApiUrl()}/v1/me".WithOAuthBearerToken(token).GetJsonAsync<MeUser>();
                    }
                    catch (FlurlHttpException ex) when ((int?)ex.Call.Response?.StatusCode < 500) {
                        return null;
                    }
                    catch (Exception) {
                        allowFetchUserRetry = true;
                        return null;
                    }
                }, TimeSpan.FromMinutes(5));

                if (allowFetchUserRetry)
                    _cache.Remove(token); // not their fault, don't make them wait 5 min

                if (user == null || !user.Active)
                    return AuthenticateResult.Fail("Authentication failure");

                cid.AddClaim(new Claim("username", user.Username));
                cid.AddClaim(new Claim("userid", user.ID));
                cid.AddClaim(new Claim("email", user.Email ?? ""));
                cid.AddClaim(new Claim("buyer", user.Buyer?.ID ?? ""));
                cid.AddClaim(new Claim("supplier", user.Supplier?.ID ?? ""));
                cid.AddClaim(new Claim("seller", user?.Seller?.ID ?? ""));

                cid.AddClaims(user.AvailableRoles.Select(r => new Claim(ClaimTypes.Role, r)));
                var roles = jwt.Claims.Where(c => c.Type == "role").Select(c => new Claim(ClaimTypes.Role, c.Value)).ToList();
                roles.Add(new Claim(ClaimTypes.Role, BaseUserRole));
                cid.AddClaims(roles);

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(cid), "OrderCloudIntegrations");
                var ticketResult = AuthenticateResult.Success(ticket);
                return ticketResult;
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }
        }

        private string GetTokenFromAuthHeader()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var header))
                return null;

            var parts = header.FirstOrDefault()?.Split(new[] { ' ' }, 2);
            if (parts?.Length != 2)
                return null;

            return parts[0] != "Bearer" ? null : parts[1].Trim();
        }
    }

    public class OrderCloudIntegrationsAuthOptions : AuthenticationSchemeOptions
    {
        public IOrderCloudClient OrderCloudClient { get; set; }
    }
}
