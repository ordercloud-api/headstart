using System;
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

    public class OrderCloudIntegrationsCardConnectService : IOrderCloudIntegrationsCardConnectService
    {
        private readonly IFlurlClient _flurl;
        public OrderCloudIntegrationsCardConnectConfig Config { get; }

        public OrderCloudIntegrationsCardConnectService(OrderCloudIntegrationsCardConnectConfig config, IFlurlClientFactory flurlFactory)
        {
            Config = config;
            _flurl = flurlFactory.Get($"https://{Config.Site}.{Config.BaseUrl}/");
        }

        private IFlurlRequest Request(string resource, string currency)
        {
            return _flurl.Request($"{resource}").WithHeader("Authorization", $"Basic {((currency == "USD") ? Config.Authorization : Config.AuthorizationCad)}");
        }

        public async Task<CardConnectAccountResponse> Tokenize(CardConnectAccountRequest request)
        {
            return await this.Request("cardsecure/api/v1/ccn/tokenize", request.currency).PostJsonAsync(request).ReceiveJson<CardConnectAccountResponse>();
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
            var attempt = await this
                .Request("cardconnect/rest/void", request.currency)
                .PutJsonAsync(request)
                .ReceiveJson<CardConnectVoidResponse>();

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

        private async Task<CardConnectAuthorizationResponse> PostAuthorizationAsync(CardConnectAuthorizationRequest request)
        {
            var attempt = await this
                .Request("cardconnect/rest/auth", request.currency)
                .PutJsonAsync(request)
                .ReceiveJson<CardConnectAuthorizationResponse>();

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
