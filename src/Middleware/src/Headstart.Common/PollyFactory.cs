using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Polly;

namespace Headstart.Common
{
    // Polly is our resilience library and Flurl is our http library
    // This PollyFactory lets us express a default resilience policy
    // for our app on any flurl http call
    // https://stackoverflow.com/a/52284010/6147893
    public class PollyFactory : DefaultHttpClientFactory
    {
        private readonly IAsyncPolicy<HttpResponseMessage> policy;

        public PollyFactory(IAsyncPolicy<HttpResponseMessage> policy)
        {
            this.policy = policy;
        }

        public override HttpMessageHandler CreateMessageHandler()
        {
            return new PollyHandler(policy)
            {
                InnerHandler = base.CreateMessageHandler(),
            };
        }
    }

    public class PollyHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> policy;

        public PollyHandler(IAsyncPolicy<HttpResponseMessage> policy)
        {
            this.policy = policy;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return policy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
        }
    }
}
