using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Content;

namespace Headstart.Common.Extensions
{
    public static class FlurlExtensions
    {
        public static Task<IFlurlResponse> PutMultipartAsync(this IFlurlRequest request, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cmc = new CapturedMultipartContent(request.Settings);
            buildContent(cmc);
            return request.SendAsync(HttpMethod.Put, cmc, cancellationToken);
        }

        public static Task<IFlurlResponse> PutMultipartAsync(this Url url, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new FlurlRequest(url).PutMultipartAsync(buildContent, cancellationToken);
        }

        public static Task<IFlurlResponse> PutMultipartAsync(this string url, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new FlurlRequest(url).PutMultipartAsync(buildContent, cancellationToken);
        }
    }
}
