using System;
using Flurl.Http;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Headstart.Common.Services.Zoho.Models;
using Headstart.Common.Services.Zoho.Resources;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services.Zoho
{
	public partial interface IZohoClient
	{
		IZohoContactResource Contacts { get; }
		IZohoCurrencyResource Currencies { get; }
		IZohoItemResource Items { get; }
		IZohoSalesOrderResource SalesOrders { get; }
		IZohoPurchaseOrderResource PurchaseOrders { get; }
		IZohoOrganizationResource Organizations { get; }
		Task<ZohoTokenResponse> AuthenticateAsync();
	}

	public partial class ZohoClient
	{
		private readonly IFlurlClientFactory _flurlFactory;
		private IFlurlClient ApiClient => _flurlFactory.Get(Config.ApiUrl);
		private IFlurlClient AuthClient => _flurlFactory.Get("https://accounts.zoho.com/oauth/v2/");
		private readonly WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

		public ZohoTokenResponse TokenResponse { get; set; } = new ZohoTokenResponse();
		public bool IsAuthenticated => (TokenResponse?.access_token != null);
		public ZohoClientConfig Config { get; } = new ZohoClientConfig();
		public ZohoClient() : this(new ZohoClientConfig()) { }

		public ZohoClient(IFlurlClientFactory flurlFactory)
		{
			try
			{
				_flurlFactory = flurlFactory;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public ZohoClient(ZohoClientConfig config, IFlurlClientFactory flurlFactory)
		{
			try
			{
				_flurlFactory = flurlFactory;
				Config = config;
				InitResources();
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public ZohoClient(ZohoClientConfig config)
		{
			try
			{
				Config = config;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task<ZohoTokenResponse> AuthenticateAsync()
		{
			try
			{
				var response = await AuthClient.Request($@"token")
					.SetQueryParam($@"client_id", Config.ClientId)
					.SetQueryParam($@"client_secret", Config.ClientSecret)
					.SetQueryParam($@"grant_type", $@"refresh_token")
					.SetQueryParam($@"refresh_token", Config.AccessToken)
					.SetQueryParam($@"redirect_uri", $@"https://ordercloud.io")
					.PostAsync(null);
				TokenResponse = JObject.Parse(await response.ResponseMessage.Content.ReadAsStringAsync()).ToObject<ZohoTokenResponse>();
				return TokenResponse;
			}
			catch (FlurlHttpException ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
				throw new CatalystBaseException($@"ZohoAuthenticationError", ex.Message, null, ex.Call.Response.StatusCode);
			}
		}

		internal IFlurlRequest Request(object[] segments, string access_token = null)
		{
			return ApiClient.Request(segments).WithHeader($@"Authorization", $@"Zoho-oauthtoken {access_token ?? TokenResponse.access_token}")
				.ConfigureRequest(settings =>
				{
					settings.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
					{
						Formatting = Formatting.Indented,
						NullValueHandling = NullValueHandling.Ignore,
						MissingMemberHandling = MissingMemberHandling.Ignore,
						DefaultValueHandling = DefaultValueHandling.Ignore
					});
				});
		}

		internal IFlurlRequest Put(object obj, object[] segments)
		{
			return WriteRequest(obj, segments);
		}

		internal IFlurlRequest Post(object obj, object[] segments)
		{
			return WriteRequest(obj, segments);
		}

		private IFlurlRequest WriteRequest(object obj, object[] segments, string access_token = null)
		{
			return ApiClient.Request(segments).WithHeader($@"Authorization", $@"Zoho-oauthtoken {access_token ?? TokenResponse.access_token}")
				.ConfigureRequest(settings =>
				{
					settings.JsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
					{
						Formatting = Formatting.Indented,
						NullValueHandling = NullValueHandling.Ignore,
						MissingMemberHandling = MissingMemberHandling.Ignore,
						DefaultValueHandling = DefaultValueHandling.Ignore
					});
				});
		}
	}

	public partial class ZohoClient : IZohoClient
	{
		public IZohoOrganizationResource Organizations { get; private set; }

		public IZohoContactResource Contacts { get; private set; }

		public IZohoCurrencyResource Currencies { get; private set; }

		public IZohoItemResource Items { get; private set; }

		public IZohoSalesOrderResource SalesOrders { get; private set; }

		public IZohoPurchaseOrderResource PurchaseOrders { get; private set; }

		private void InitResources()
		{
			Contacts = new ZohoContactResource(this);
			Currencies = new ZohoCurrencyResource(this);
			Items = new ZohoItemResource(this);
			SalesOrders = new ZohoSalesOrderResource(this);
			PurchaseOrders = new ZohoPurchaseOrderResource(this);
			Organizations = new ZohoOrganizationResource(this);
		}
	}
}