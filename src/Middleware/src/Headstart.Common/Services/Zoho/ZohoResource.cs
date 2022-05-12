using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;

namespace Headstart.Common.Services.Zoho
{
    // https://stackoverflow.com/questions/52541918/flurl-extension-for-multi-part-put
    public static class MultipartPutExtensions
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

    public abstract class ZohoResource
    {
        private readonly ZohoClient client;
        private readonly string resource;
        private readonly object[] segments;

        protected ZohoResource(ZohoClient client, string resource, params object[] segments)
        {
            this.client = client;
            this.resource = resource;
            this.segments = segments;
        }

        protected internal IFlurlRequest Get(params object[] segments) =>
            client.Request(this.AppendSegments(segments));

        protected internal IFlurlRequest Delete(params object[] segments) =>
            client.Request(this.AppendSegments(segments));

        protected internal IFlurlRequest Post(params object[] segments) =>
            client.Request(this.AppendSegments(segments));

        protected internal async Task<T> Post<T>(object obj) =>
            await Parse<T>(await client.Post(obj, segments).PostMultipartAsync(f =>
            {
                f.AddString("JSONString", JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                }));
            }));

        protected internal async Task<T> Put<T>(object obj, params object[] segments) =>
            await Parse<T>(await client.Put(obj, this.AppendSegments(segments)).PutMultipartAsync(f =>
            {
                f.AddString("JSONString", JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                }));
            }));

        private object[] AppendSegments(params object[] segments)
        {
            if (segments.Length <= 0)
            {
                return this.segments;
            }

            var appended = this.segments.ToList();
            appended.AddRange(segments);
            return appended.ToArray();
        }

        private async Task<T> Parse<T>(IFlurlResponse res) =>
            JObject.Parse(await res.ResponseMessage.Content.ReadAsStringAsync()).SelectToken(resource).ToObject<T>();
    }
}
