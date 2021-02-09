using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Common.Helpers
{
    public interface IOrderCloudIntegrationsFunctionToken
    {
        Task<VerifiedUserContext> Authorize(HttpRequest request, ApiRole[] roles, IOrderCloudClient oc = null);
        Task<VerifiedUserContext> Authorize(string token, ApiRole[] roles, IOrderCloudClient oc = null);
    }

    /// <summary>
    /// Validates a incoming request and extracts any <see cref="ClaimsPrincipal"/> contained within the bearer token.
    /// </summary>
    public class OrderCloudIntegrationsFunctionToken : IOrderCloudIntegrationsFunctionToken
    {
        private const string AUTH_HEADER_NAME = "Authorization";
        private const string BEARER_PREFIX = "Bearer ";
        private readonly AppSettings _settings;

        public OrderCloudIntegrationsFunctionToken(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task<VerifiedUserContext> Authorize(string token, ApiRole[] roles, IOrderCloudClient oc = null)
        {
            return await ParseJwt(token, roles, oc);
        }

        public async Task<VerifiedUserContext> Authorize(HttpRequest request, ApiRole[] roles, IOrderCloudClient oc = null)
        {
            Require.That(request.Headers.ContainsKey(AUTH_HEADER_NAME), new ErrorCode("Authorization.InvalidToken", 401, "Authorization.InvalidToken: Access token is invalid or expired."));
            Require.That(request.Headers[AUTH_HEADER_NAME].ToString().StartsWith(BEARER_PREFIX), new ErrorCode("Authorization.InvalidToken", 401, "Authorization.InvalidToken: Access token is invalid or expired."));
            // Get the token from the header
            var token = request.Headers[AUTH_HEADER_NAME].ToString().Substring(BEARER_PREFIX.Length);
            return await ParseJwt(token, roles, oc);
        }

        private async Task<VerifiedUserContext> ParseJwt(string token, ApiRole[] roles, IOrderCloudClient oc = null)
        {
            var jwt = new JwtSecurityToken(token);
            var clientId = jwt.Claims.FirstOrDefault(x => x.Type == "cid")?.Value;
            var usrtype = jwt.Claims.FirstOrDefault(x => x.Type == "usrtype")?.Value;
            var scope = jwt.Claims.Where(x => x.Type == "role").Select(x => x.Value)?.ToList();
            var usr = jwt.Claims.FirstOrDefault(x => x.Type == "usr")?.Value;
            // validate scope
            if (!scope.Contains("FullAccess"))
                Require.That(scope.Count(s => roles.Any(role => s == role.ToString())) > 0, new ErrorCode("Authorization.InvalidToken", 401, "Authorization.InvalidToken: Access token is invalid or expired."));

            var cid = new ClaimsIdentity("OrderCloudIntegrations");
            cid.AddClaim(new Claim("clientid", clientId));
            cid.AddClaim(new Claim("accesstoken", token));

            if (oc == null)
                oc = new OrderCloudClient(new OrderCloudClientConfig()
                {
                    AuthUrl = _settings.OrderCloudSettings.ApiUrl,
                    ApiUrl = _settings.OrderCloudSettings.ApiUrl,
                    ClientId = clientId
                });
            var user = await oc.Me.GetAsync(token);

            if (!user.Active || user.Username != usr)
                throw new Exception("Invalid User");
            cid.AddClaim(new Claim("username", user.Username));
            cid.AddClaim(new Claim("userid", user.ID));
            cid.AddClaim(new Claim("email", user.Email ?? ""));
            cid.AddClaim(new Claim("buyer", user.Buyer?.ID ?? ""));
            cid.AddClaim(new Claim("supplier", user.Supplier?.ID ?? ""));
            cid.AddClaim(new Claim("seller", user?.Seller?.ID ?? ""));

            cid.AddClaims(user.AvailableRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            //Validate the token
            return new VerifiedUserContext(new ClaimsPrincipal(cid));
        }
    }
}
