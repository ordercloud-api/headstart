using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ordercloud.integrations.library
{
    public class OrderCloudWebhookAuthAttribute : AuthorizeAttribute
    {
        public OrderCloudWebhookAuthAttribute()
        {
            AuthenticationSchemes = "OrderCloudWebhook";
        }
    }

    public class OrderCloudWebhookAuthHandler : AuthenticationHandler<OrderCloudWebhookAuthOptions>
    {
        private const string OC_HASH = "X-oc-hash";
        private const string AUTH_SCHEME = "OcWebhook";
        private const string AUTH_FAIL_MSG = "X-oc-hash header was not sent. Endpoint can only be hit from a valid OrderCloud webhook.";
        private const string HASH_MATCH_FAIL = "X-oc-hash header does not match. Endpoint can only be hit from a valid OrderCloud webhook.";

        public OrderCloudWebhookAuthHandler(IOptionsMonitor<OrderCloudWebhookAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock) { }

        //TODO: evaluate if using OrderCloudIntegrationException for errors works as expected
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Require.DoesNotExist(Options.HashKey, async () => await Task.FromResult(AuthenticateResult.Fail(AUTH_FAIL_MSG)));
            Require.DoesNotExist(Context.Request.Headers.ContainsKey(OC_HASH), async () => await Task.FromResult(AuthenticateResult.Fail(AUTH_FAIL_MSG)));
            Require.DoesNotExist(Context.Request.Headers[OC_HASH].FirstOrDefault(), async () => await Task.FromResult(AuthenticateResult.Fail(AUTH_FAIL_MSG)));

            var hash_header = Context.Request.Headers[OC_HASH].FirstOrDefault();

            // alternative to Context.Request.EnableRewind() which is no longer available
            // allows us to set Request Body position back to 0
            HttpRequestRewindExtensions.EnableBuffering(Context.Request);

            try
            {
                Require.ThatHashMatches(hash_header, Options.HashKey, Context.Request.Body, async () => await Task.FromResult(AuthenticateResult.Fail(HASH_MATCH_FAIL)));
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(AUTH_SCHEME)), AUTH_SCHEME);
                return await Task.FromResult(AuthenticateResult.Success(ticket));
            }
            finally
            {
                Context.Request.Body.Position = 0;
            }
        }
    }
    public class OrderCloudWebhookAuthOptions : AuthenticationSchemeOptions
    {
        public string HashKey { get; set; }
    }
}
