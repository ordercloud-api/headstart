using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Zoho.Models;
using OrderCloud.Integrations.Zoho.Resources;

namespace OrderCloud.Integrations.Zoho
{
    public interface IZohoClient
    {
        ZohoConfig Config { get; }

        IZohoContactResource Contacts { get; }

        IZohoCurrencyResource Currencies { get; }

        IZohoItemResource Items { get; }

        IZohoSalesOrderResource SalesOrders { get; }

        IZohoPurchaseOrderResource PurchaseOrders { get; }

        IZohoOrganizationResource Organizations { get; }

        Task<ZohoTokenResponse> AuthenticateAsync();
    }

    public class ZohoClient : IZohoClient
    {
        private readonly IFlurlClientFactory flurlFactory;

        public ZohoClient()
            : this(new ZohoConfig())
        {
        }

        public ZohoClient(IFlurlClientFactory flurlFactory)
        {
            this.flurlFactory = flurlFactory;
        }

        public ZohoClient(ZohoConfig config, IFlurlClientFactory flurlFactory)
        {
            this.flurlFactory = flurlFactory;
            Config = config;
            InitResources();
        }

        public ZohoClient(ZohoConfig config)
        {
            this.Config = config;
        }

        public IZohoOrganizationResource Organizations { get; private set; }

        public IZohoContactResource Contacts { get; private set; }

        public IZohoCurrencyResource Currencies { get; private set; }

        public IZohoItemResource Items { get; private set; }

        public IZohoSalesOrderResource SalesOrders { get; private set; }

        public IZohoPurchaseOrderResource PurchaseOrders { get; private set; }

        public ZohoTokenResponse TokenResponse { get; set; }

        public bool IsAuthenticated => TokenResponse?.access_token != null;

        public ZohoConfig Config { get; }

        private IFlurlClient ApiClient => flurlFactory.Get(Config.ApiUrl);

        private IFlurlClient AuthClient => flurlFactory.Get("https://accounts.zoho.com/oauth/v2/");

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
                    DefaultValueHandling = DefaultValueHandling.Ignore,
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
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                });
            });

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
