using System;
using System.Globalization;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using OrderCloud.SDK;

namespace ordercloud.integrations.cardconnect
{
    public interface IOrderCloudIntegrationsCardConnectService
    {
        Task<CardConnectAccountResponse> Tokenize(CardConnectAccountRequest request);
        Task<CardConnectAuthorizationResponse> AuthWithoutCapture(CardConnectAuthorizationRequest request);
        Task<CardConnectAuthorizationResponse> AuthWithCapture(CardConnectAuthorizationRequest request);
        Task<CardConnectVoidResponse> VoidAuthorization(CardConnectVoidRequest request);
    }

    public class OrderCloudIntegrationsCardConnectConfig 
    {
        public string Site { get; set; }
        public string BaseUrl { get; set; }
        public string Authorization { get; set; }
        public string AuthorizationCad { get; set; } // we need a separate merchant account for canadian currency
        public string UsdMerchantID { get; set; }
        public string CadMerchantID { get; set; }
        public string EurMerchantID { get; set; }
    }
    public enum AppEnvironment { Test, Staging, Production }

    public class OrderCloudIntegrationsCardConnectService : IOrderCloudIntegrationsCardConnectService
    {
        private readonly IFlurlClient _flurl;
        private bool noAccountCredentials;
        private AppEnvironment appEnvironment;
        public OrderCloudIntegrationsCardConnectConfig Config { get; }

        public OrderCloudIntegrationsCardConnectService(OrderCloudIntegrationsCardConnectConfig config, string environment, IFlurlClientFactory flurlFactory)
        {
            Config = config;
            // if no credentials are provided in Test and UAT, responses will be mocked.
            noAccountCredentials = config == null || (config?.Authorization == null && config.AuthorizationCad == null) || (config?.Authorization == "" && config?.AuthorizationCad == "");
            appEnvironment = (AppEnvironment)Enum.Parse(typeof(AppEnvironment), environment);
            _flurl = flurlFactory.Get($"https://{Config?.Site}.{Config?.BaseUrl}/");
        }

        private IFlurlRequest Request(string resource, string currency)
        {
            return _flurl.Request($"{resource}").WithHeader("Authorization", $"Basic {((currency == "USD") ? Config.Authorization : Config.AuthorizationCad)}");
        }

        public async Task<CardConnectAccountResponse> Tokenize(CardConnectAccountRequest request)
        {
            if (ShouldMockCardConnectResponse())
            {
                // Returns a mocked account object (only in Test and UAT)
                return MockCardConnectAccountResponse(request);
            }
            else
            {
                return await this.Request("cardsecure/api/v1/ccn/tokenize", request.currency).PostJsonAsync(request).ReceiveJson<CardConnectAccountResponse>();
            }
        }

        public async Task<CardConnectAuthorizationResponse> AuthWithoutCapture(CardConnectAuthorizationRequest request)
        {
            return await PostAuthorizationAsync(request);
        }

        public async Task<CardConnectAuthorizationResponse> AuthWithCapture(CardConnectAuthorizationRequest request)
        {
            request.capture = "Y";
            return await PostAuthorizationAsync(request);
        }

        public async Task<CardConnectVoidResponse> VoidAuthorization(CardConnectVoidRequest request)
        {
            CardConnectVoidResponse attempt;
            if (ShouldMockCardConnectResponse())
            {
                // Returns a mocked void object (only in Test and UAT)
                attempt = MockCardConnectVoidResponse(request);
            }
            else
            {
                attempt = await this
                               .Request("cardconnect/rest/void", request.currency)
                               .PutJsonAsync(request)
                               .ReceiveJson<CardConnectVoidResponse>();

            }
            
            if (attempt.WasSuccessful())
            {
                return attempt;
            }
            throw new CreditCardVoidException(new ApiError()
            {
                Data = attempt,
                Message = attempt.resptext, // response codes: https://developer.cardconnect.com/cardconnect-api?lang=json#void-service-url
                ErrorCode = attempt.respcode
            }, attempt);
        }

        private bool ShouldMockCardConnectResponse()
        {
            // To give a bigger "headstart" in Test and UAT, Responses can be mocked by simply
            // not providing CardConnect credentials. (They are still needed for Production)
            return noAccountCredentials && appEnvironment != AppEnvironment.Production;
        }

        private CardConnectAccountResponse MockCardConnectAccountResponse(CardConnectAccountRequest request)
        {
            CardConnectAccountResponse response;
            response = new CardConnectAccountResponse()
            {
                message = "Mock CardConnect account response",
                token = ""
            };
            return response;
        }

        private CardConnectVoidResponse MockCardConnectVoidResponse(CardConnectVoidRequest request)
        {
            CardConnectVoidResponse response;

            response = new CardConnectVoidResponse()
            {
                amount = 100000,
                resptext = "Successful Mocked Response",
                respstat = "A",
                respcode = "0",
            };

            return response;
        }

        private CardConnectAuthorizationResponse MockCardConnectAuthorizationResponse(CardConnectAuthorizationRequest request)
        {
            CardConnectAuthorizationResponse response;

            response = new CardConnectAuthorizationResponse()
            {
                amount = decimal.Parse(request.amount, CultureInfo.InvariantCulture),
                resptext = "Success",
                cvvresp = "U",
                commcard = "Mock Response",
                respstat = "A",
                respcode = "0"
            };

            return response;

        }
        private async Task<CardConnectAuthorizationResponse> PostAuthorizationAsync(CardConnectAuthorizationRequest request)
        {
            CardConnectAuthorizationResponse attempt = new CardConnectAuthorizationResponse();
            if (ShouldMockCardConnectResponse())
            {
                attempt = MockCardConnectAuthorizationResponse(request);
            }
            else
            {
                attempt = await this
                               .Request("cardconnect/rest/auth", request.currency)
                               .PutJsonAsync(request)
                               .ReceiveJson<CardConnectAuthorizationResponse>();

            }
           
            if (attempt.WasSuccessful())
            {
                return attempt;
            }
            throw new CreditCardAuthorizationException(new ApiError()
            {
                Data = attempt,
                Message = attempt.resptext, // response codes: https://developer.cardconnect.com/assets/developer/assets/authResp_2-11-19.txt
                ErrorCode = attempt.respcode
            }, attempt);
        }
    }
}
