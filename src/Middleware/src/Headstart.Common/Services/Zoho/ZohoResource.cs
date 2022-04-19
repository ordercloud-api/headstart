using Flurl;
using System;
using Flurl.Http;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using Flurl.Http.Content;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;

namespace Headstart.Common.Services.Zoho
{
	public abstract class ZohoResource
	{
		private readonly ZohoClient _client;
		private readonly string _resource;
		private readonly object[] _segments;

		protected ZohoResource(ZohoClient client, string resource, params object[] segments)
		{
			_client = client;
			_resource = resource;
			_segments = segments;
		}

		private object[] AppendSegments(params object[] segments)
		{
			if (segments.Length <= 0)
			{
				return _segments;
			}
			List<object> appended = _segments.ToList();
			appended.AddRange(segments);
			return appended.ToArray();
		}

		protected internal IFlurlRequest Get(params object[] segments)
		{
			return _client.Request(AppendSegments(segments));
		}

		protected internal IFlurlRequest Delete(params object[] segments)
		{
			return _client.Request(AppendSegments(segments));
		}

		protected internal IFlurlRequest Post(params object[] segments)
		{
			return _client.Request(AppendSegments(segments));
		}

		protected internal async Task<T> Post<T>(object obj)
		{
			return await Parse<T>(await _client.Post(obj, _segments).PostMultipartAsync(f =>
			{
				f.AddString($@"JSONString", JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
				{
					NullValueHandling = NullValueHandling.Ignore,
					Formatting = Formatting.None
				}));
			}));
		}

		protected internal async Task<T> Put<T>(object obj, params object[] segments)
		{
			return await Parse<T>(await _client.Put(obj, AppendSegments(segments)).PutMultipartAsync(f =>
			{
				f.AddString($@"JSONString", JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
				{
					NullValueHandling = NullValueHandling.Ignore,
					Formatting = Formatting.None
				}));
			}));
		}

		private async Task<T> Parse<T>(IFlurlResponse res)
		{
			return JObject.Parse(await res.ResponseMessage.Content.ReadAsStringAsync()).SelectToken(_resource).ToObject<T>();
		}
	}

	public static class MultipartPutExtensions
	{
		public static Task<IFlurlResponse> PutMultipartAsync(this IFlurlRequest request, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
		{
			CapturedMultipartContent cmc = new CapturedMultipartContent(request.Settings);
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