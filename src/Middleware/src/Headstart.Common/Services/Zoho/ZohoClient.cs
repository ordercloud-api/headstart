using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Headstart.Common.Services.Zoho.Models;
using Headstart.Common.Services.Zoho.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

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

        public ZohoClient(IFlurlClientFactory flurlFactory)
        {
            _flurlFactory = flurlFactory;
        }

        private IFlurlClient ApiClient => _flurlFactory.Get(Config.ApiUrl);
        private IFlurlClient AuthClient => _flurlFactory.Get("https://accounts.zoho.com/oauth/v2/");
        public ZohoTokenResponse TokenResponse { get; set; }

        public bool IsAuthenticated => TokenResponse?.access_token != null;

        public ZohoClientConfig Config { get; }

        public ZohoClient() : this(new ZohoClientConfig()) { }

        public ZohoClient(ZohoClientConfig config, IFlurlClientFactory flurlFactory)
        {
            _flurlFactory = flurlFactory;
            Config = config;
            InitResources();
        }

        public ZohoClient(ZohoClientConfig config)
        {
            this.Config = config;
        }

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

        internal IFlurlRequest Put(object obj, object[] segments) => WriteRequest(obj, segments);
        internal IFlurlRequest Post(object obj, object[] segments) => WriteRequest(obj, segments);

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
