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
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the ZohoClient class object with Dependency Injection
		/// </summary>
		/// <param name="flurlFactory"></param>
		/// <param name="settings"></param>
		public ZohoClient(IFlurlClientFactory flurlFactory, AppSettings settings)
		{
			try
			{
				_settings = settings;
				_flurlFactory = flurlFactory;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}
		
		private IFlurlClient ApiClient => _flurlFactory.Get(Config.ApiUrl);
		private IFlurlClient AuthClient => _flurlFactory.Get("https://accounts.zoho.com/oauth/v2/");
		public ZohoTokenResponse TokenResponse { get; set; }

		public bool IsAuthenticated => TokenResponse?.access_token != null;

		public ZohoClientConfig Config { get; }

		public ZohoClient() : this(new ZohoClientConfig()) { }

		/// <summary>
		/// The IOC based constructor method for the ZohoClient class object with Dependency Injection
		/// </summary>
		/// <param name="config"></param>
		/// <param name="flurlFactory"></param>
		/// <param name="settings"></param>
		public ZohoClient(ZohoClientConfig config, IFlurlClientFactory flurlFactory, AppSettings settings)
		{
			try
			{
				_settings = settings;
				_flurlFactory = flurlFactory;
				Config = config;
				InitResources();
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// The IOC based constructor method for the ZohoClient class object with Dependency Injection
		/// </summary>
		/// <param name="config"></param>
		public ZohoClient(ZohoClientConfig config)
		{
			this.Config = config;
		}

		/// <summary>
		/// Public re-usable AuthenticateAsync task method
		/// </summary>
		/// <returns>The ZohoTokenResponse object value from the AuthenticateAsync process</returns>
		/// <exception cref="CatalystBaseException"></exception>
		public async Task<ZohoTokenResponse> AuthenticateAsync()
		{
			try
			{
				var response = await AuthClient.Request("token")
					.SetQueryParam("client_id", Config.ClientId)
					.SetQueryParam("client_secret", Config.ClientSecret)
					.SetQueryParam("grant_type", "refresh_token")
					.SetQueryParam("refresh_token", Config.AccessToken)
					.SetQueryParam("redirect_uri", "https://ordercloud.io")
					.PostAsync(null);
				this.TokenResponse = JObject.Parse(await response.ResponseMessage.Content.ReadAsStringAsync()).ToObject<ZohoTokenResponse>();
				return this.TokenResponse;
			}
			catch (FlurlHttpException ex)
			{
				throw new CatalystBaseException("ZohoAuthenticationError", ex.Message, null, (int)ex.Call.Response.StatusCode);
			}
		}

		/// <summary>
		/// Internal re-usable Request method
		/// </summary>
		/// <param name="segments"></param>
		/// <param name="access_token"></param>
		/// <returns>The WriteRequest value</returns>
		internal IFlurlRequest Request(object[] segments, string access_token = null)
		{
			return ApiClient
				.Request(segments)
				.WithHeader("Authorization", $"Zoho-oauthtoken {access_token ?? this.TokenResponse.access_token}")
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

		/// <summary>
		/// Internal re-usable Put method
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="segments"></param>
		/// <returns>The WriteRequest value</returns>
		internal IFlurlRequest Put(object obj, object[] segments) => WriteRequest(obj, segments);

		/// <summary>
		/// Internal re-usable Post method
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="segments"></param>
		/// <returns>The WriteRequest  value</returns>
		internal IFlurlRequest Post(object obj, object[] segments) => WriteRequest(obj, segments);

		/// <summary>
		/// Internal re-usable Request method
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="segments"></param>
		/// <param name="access_token"></param>
		/// <returns>The WriteRequest value</returns>
		private IFlurlRequest WriteRequest(object obj, object[] segments, string access_token = null) => ApiClient
			.Request(segments)
			.WithHeader("Authorization", $"Zoho-oauthtoken {access_token ?? this.TokenResponse.access_token}")
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

	public partial class ZohoClient : IZohoClient
	{
		private void InitResources()
		{
			Contacts = new ZohoContactResource(this);
			Currencies = new ZohoCurrencyResource(this);
			Items = new ZohoItemResource(this);
			SalesOrders = new ZohoSalesOrderResource(this);
			PurchaseOrders = new ZohoPurchaseOrderResource(this);
			Organizations = new ZohoOrganizationResource(this);
		}

		public IZohoOrganizationResource Organizations { get; private set; }
		public IZohoContactResource Contacts { get; private set; }
		public IZohoCurrencyResource Currencies { get; private set; }
		public IZohoItemResource Items { get; private set; }
		public IZohoSalesOrderResource SalesOrders { get; private set; }
		public IZohoPurchaseOrderResource PurchaseOrders { get; private set; }
	}
}