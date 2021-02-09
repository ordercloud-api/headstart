using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ordercloud.integrations.library.extensions;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public class VerifiedUserContext
    {
        public ClaimsPrincipal Principal { get; set; }
        private JwtSecurityToken _token { get; set; }
        private IOrderCloudClient _oc { get; set;}

        public VerifiedUserContext(IOrderCloudClient oc) {
            _oc = oc;
        }

        /// <summary>
        /// This method is not for regular use. If you are attempting to use it for any reason consult the project lead
        /// If you need the context of an authenticated user ensure you're using a valid OrderCloudIntegrationsAuth attribute
        /// and reference the VerifiedUserContext from the BaseController
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task<VerifiedUserContext> Define(OrderCloudClientConfig config)
        {
            var auth = await _oc.AuthenticateAsync();
            var user = await _oc.Me.GetAsync();
            var jwt = new JwtSecurityToken(auth.AccessToken);

            var cid = new ClaimsIdentity("OrderCloudIntegrations");
            cid.AddClaim(new Claim("accesstoken", auth.AccessToken));
            cid.AddClaim(new Claim("clientid", jwt.GetClientID()));
            cid.AddClaim(new Claim("usrtype", jwt.GetUserType()));
            cid.AddClaim(new Claim("username", user.Username));
            cid.AddClaim(new Claim("userid", user.ID));
            cid.AddClaim(new Claim("email", user.Email ?? ""));
            cid.AddClaim(new Claim("buyer", user.Buyer?.ID ?? ""));
            cid.AddClaim(new Claim("supplier", user.Supplier?.ID ?? ""));
            cid.AddClaim(new Claim("seller", user.Seller?.ID ?? ""));
            cid.AddClaims(user.AvailableRoles.Select(r => new Claim(ClaimTypes.Role, r)));
            var roles = user.AvailableRoles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
            roles.Add(new Claim(ClaimTypes.Role, "BaseUserRole"));
            cid.AddClaims(roles);

            Principal = new ClaimsPrincipal(cid);
            _token = new JwtSecurityTokenHandler().ReadJwtToken(auth.AccessToken);
            return this;
        }

        public VerifiedUserContext(ClaimsPrincipal principal)
        {
            Principal = principal;
            if (Principal.Claims.Any())
                _token = new JwtSecurityTokenHandler().ReadJwtToken(this.AccessToken);
        }

        public string UsrType
        {
            get { return _token.Payload.FirstOrDefault(t => t.Key == "usrtype").Value?.ToString(); }
        }

        public string UserID
        {
            get { return Principal.Claims.First(c => c.Type == "userid").Value; }
        }
        public string Username
        {
            get { return Principal.Claims.FirstOrDefault(c => c.Type == "username")?.Value; }
        }
        public string ClientID
        {
            get { return Principal.Claims.First(c => c.Type == "clientid").Value; }
        }
        public string Email
        {
            get { return Principal.Claims.FirstOrDefault(c => c.Type == "email")?.Value; }
        }
        public string SupplierID
        {
            get { return Principal.Claims.FirstOrDefault(c => c.Type == "supplier")?.Value; }
        }
        public string BuyerID
        {
            get { return Principal.Claims.FirstOrDefault(c => c.Type == "buyer")?.Value; }
        }

        public string SellerID
        {
            get { return Principal.Claims.FirstOrDefault(c => c.Type == "seller")?.Value; }
        }

        public string AccessToken
        {
            get { return Principal.Claims.First(c => c.Type == "accesstoken")?.Value; }
            set => AccessToken = value;
        }

        public string AuthUrl
        {
            get { return _token.Payload.FirstOrDefault(t => t.Key == "iss").Value?.ToString(); }
        }

        public string ApiUrl
        {
            get { return _token.Payload.FirstOrDefault(t => t.Key == "aud").Value?.ToString(); }
        }

        public DateTime AccessTokenExpiresUTC
        {
            get
            {
                return _token.Payload.FirstOrDefault(t => t.Key == "exp").Value.ToString().UnixToDateTimeUTC();
            }
        }
    }
}
