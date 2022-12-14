using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using OrderCloud.Integrations.CardConnect.Exceptions;
using OrderCloud.Integrations.CardConnect.Extensions;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.CardConnect
{
    public enum AppEnvironment
    {
        Test,
        Staging,
        Production,
    }

    public interface ICardConnectClient
    {
        Task<CardConnectAccountResponse> Tokenize(CardConnectAccountRequest request);

        Task<CardConnectAuthorizationResponse> AuthWithoutCapture(CardConnectAuthorizationRequest request);

        Task<CardConnectAuthorizationResponse> AuthWithCapture(CardConnectAuthorizationRequest request);

        Task<CardConnectVoidResponse> VoidAuthorization(CardConnectVoidRequest request);

        Task<CardConnectCaptureResponse> Capture(CardConnectCaptureRequest request);

        Task<CardConnectInquireResponse> Inquire(CardConnectInquireRequest request);

        Task<CardConnectRefundResponse> Refund(CardConnectRefundRequest request);
    }

    public class CardConnectClient : ICardConnectClient
    {
        private readonly IFlurlClient flurl;
        private AppEnvironment appEnvironment;

        public CardConnectClient(CardConnectSettings config, string environment, IFlurlClientFactory flurlFactory)
        {
            Config = config;
            appEnvironment = (AppEnvironment)Enum.Parse(typeof(AppEnvironment), environment);
            flurl = flurlFactory.Get($"https://{Config?.Site}.{Config?.BaseUrl}/");
        }

        public CardConnectSettings Config { get; }

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

        public async Task<CardConnectCaptureResponse> Capture(CardConnectCaptureRequest request)
        {
            var attempt = await this
                .Request("cardconnect/rest/capture", request.currency)
                .PostJsonAsync(request)
                .ReceiveJson<CardConnectCaptureResponse>();

            if (attempt.WasSuccessful())
            {
                return attempt;
            }

            throw new CardConnectCaptureException(
                new ApiError
                {
                    Data = attempt,
                    Message = attempt.resptext,
                    ErrorCode = attempt.respcode,
                }, attempt);
        }

        public async Task<CardConnectVoidResponse> VoidAuthorization(CardConnectVoidRequest request)
        {
            CardConnectVoidResponse attempt;
            attempt = await this
                            .Request("cardconnect/rest/void", request.currency)
                            .PutJsonAsync(request)
                            .ReceiveJson<CardConnectVoidResponse>();

            if (attempt.WasSuccessful())
            {
                return attempt;
            }

            throw new CreditCardVoidException(
                new ApiError()
                {
                    Data = attempt,
                    Message = attempt.resptext, // response codes: https://developer.cardconnect.com/cardconnect-api?lang=json#void-service-url
                    ErrorCode = attempt.respcode,
                }, attempt);
        }

        // https://developer.cardconnect.com/cardconnect-api#inquire-by-order-id
        public async Task<CardConnectInquireResponse> Inquire(CardConnectInquireRequest request)
        {
            var rawAttempt = await this
                .Request($"cardconnect/rest/inquireByOrderid/{request.orderid}/{request.merchid}/{request.set}", request.currency)
                .GetStringAsync();

            var attempt = ExtractResponse(rawAttempt, request.retref);
            if (attempt != null && attempt.WasSuccessful())
            {
                return attempt;
            }

            throw new CardConnectInquireException(
                new ApiError()
                {
                    Data = attempt,
                    Message = attempt.resptext,
                    ErrorCode = attempt.respcode,
                }, attempt);
        }

        public async Task<CardConnectRefundResponse> Refund(CardConnectRefundRequest request)
        {
            var attempt = await this
                .Request("cardconnect/rest/refund", request.currency)
                .PutJsonAsync(request)
                .ReceiveJson<CardConnectRefundResponse>();
            if (attempt.WasSuccessful())
            {
                return attempt;
            }

            throw new CreditCardRefundException(
                new ApiError()
                {
                    Data = attempt,
                    Message = attempt.resptext,
                    ErrorCode = attempt.respcode,
                }, attempt);
        }

        private IFlurlRequest Request(string resource, string currency)
        {
            return flurl.Request($"{resource}").WithHeader("Authorization", $"Basic {Config.Authorization}");
        }

        private CardConnectInquireResponse ExtractResponse(string body, string retref)
        {
            // cardconnect inquire response may be either a single item or an array of items
            // for consistency sake just return a single item, if its a list find the associated transaction by retref
            try
            {
                return JsonConvert.DeserializeObject<CardConnectInquireResponse>(body);
            }
            catch (Exception)
            {
                var list = JsonConvert.DeserializeObject<List<CardConnectInquireResponse>>(body);
                return list.FirstOrDefault(t => t.retref == retref);
            }
        }

        private async Task<CardConnectAuthorizationResponse> PostAuthorizationAsync(CardConnectAuthorizationRequest request)
        {
            CardConnectAuthorizationResponse attempt = new CardConnectAuthorizationResponse();
            attempt = await this
                            .Request("cardconnect/rest/auth", request.currency)
                            .PutJsonAsync(request)
                            .ReceiveJson<CardConnectAuthorizationResponse>();

            if (attempt.WasSuccessful())
            {
                return attempt;
            }

            throw new CreditCardAuthorizationException(
                new ApiError()
                {
                    Data = attempt,
                    Message = attempt.resptext, // response codes: https://developer.cardconnect.com/assets/developer/assets/authResp_2-11-19.txt
                    ErrorCode = attempt.respcode,
                },
                attempt);
        }
    }
}
