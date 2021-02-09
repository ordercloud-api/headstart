 using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;

namespace ordercloud.integrations.freightpop
{
	public interface IFreightPopService
	{
		Task<Response<dynamic>> ImportOrderAsync(List<OrderRequest> orderRequestBody);
		Task<Response<GetRatesData>> GetRatesAsync(RateRequestBody rateRequestBody);
		Task<Response<List<ShipmentDetails>>> GetShipmentsForOrder(string orderID);
		Task<Response<List<ShipmentDetails>>> GetShipmentsByDate(int daysAgo, string token);
		Task<string> AuthenticateAync();
	}

	public class FreightPopService : IFreightPopService
	{
		private readonly string _username;
		private readonly string _password;
		private readonly string _freightPopBaseUrl;
		private readonly IFlurlClient _flurl;
		private string accessToken;
		private DateTime tokenExpireDate;
		public FreightPopService(FreightPopConfig config)
		{
			_flurl = new FlurlClient();
			_username = config.Username;
			_password = config.Password;
			_freightPopBaseUrl = config.BaseUrl;
		}

		private IFlurlRequest MakeRequest(string resource, string token = "")
		{
			if(token.Length == 0)
			{
				// use token that is passed in if it's available
				token = accessToken;
			}
			return _flurl.Request($"{_freightPopBaseUrl}/{resource}")
				.WithHeader("Authorization", $"Bearer {token}");
		}
		public async Task<string> AuthenticateAync()
		{
			// authenticate if more than 13 days from the previous token request
			if(tokenExpireDate == null || DateTime.Now > tokenExpireDate)
			{
				var passwordGrantRequest = new PasswordGrantRequestData
				{
					Username = _username,
					Password = _password
				};
				var passwordGrantResponse = await _flurl.Request($"{_freightPopBaseUrl}/token/getToken").PostJsonAsync(passwordGrantRequest).ReceiveJson<Response<PasswordGrantResponseData>>();
			
				// freightpop tokens expire in 14 days but I don't know how to decode (not JWTs) so I am maintaining the expire date when the toke is grabbed
				tokenExpireDate = DateTime.Now.AddDays(13);
				accessToken = passwordGrantResponse.Data.AccessToken;
			}
			return accessToken;
		}
		public async Task<Response<GetRatesData>> GetRatesAsync(RateRequestBody rateRequestBody)
		{

			// change back when freightpop test is back up
			try
			{
				await AuthenticateAync();
				var rateRequestResponse = await MakeRequest("rate/getRates").PostJsonAsync(rateRequestBody).ReceiveJson<Response<GetRatesData>>();
				return rateRequestResponse;
			} catch (Exception ex) { 
				var mockRatesResponse = new Response<GetRatesData>()
				{
					Code = 200,
					Data = new GetRatesData()
					{
						Rates = new List<ShippingRate>()
						{
							new ShippingRate()
							{
								AccountName = "mock rate account",
								Currency = "USD",
								ListCost = 10,
								Carrier = "mock carrier",
								CarrierQuoteId = "mock1",
								DeliveryDays = 3,
								QuoteId = "mockratequote1",
								TotalCost = 10,
								Id = "rate1",
								Service = "Ground",
							},
							new ShippingRate()
							{
								AccountName = "mock rate account",
								Currency = "USD",
								ListCost = 20,
								Carrier = "mock carrier",
								CarrierQuoteId = "mock1",
								DeliveryDays = 2,
								QuoteId = "mockratequote2",
								TotalCost = 20,
								Id = "rate2",
								Service = "Express",
							},
							new ShippingRate()
							{
								AccountName = "mock rate account",
								Currency = "USD",
								ListCost = 30,
								Carrier = "mock carrier",
								CarrierQuoteId = "mock1",
								DeliveryDays = 1,
								QuoteId = "mockratequote3",
								TotalCost = 30,
								Id = "rate3",
								Service = "Air",
							},

						}
					},
					Message = "Mock Rate Response"
				};
				return mockRatesResponse;
			}
		}

		public async Task<Response<dynamic>> ImportOrderAsync(List<OrderRequest> orderRequestBody)
		{
			await AuthenticateAync();
			var orderRequestResponse = await MakeRequest("order/ImportOrder").PostJsonAsync(orderRequestBody).ReceiveJson<Response<dynamic>>();
			return orderRequestResponse;
		}
		public async Task<Response<List<ShipmentDetails>>> GetShipmentsForOrder(string orderID)
		{
			await AuthenticateAync();
			var getShipmentResponse = await MakeRequest($"shipment/getShipment?id={orderID}&type={GetShipmentBy.OrderNo}").GetAsync().ReceiveJson<Response<List<ShipmentDetails>>>();
			return getShipmentResponse;
		}
		public async Task<Response<List<ShipmentDetails>>> GetShipmentsByDate(int daysAgo, string token = "")
		{
			if (token.Length == 0)
			{
				// allows a token to be passed into the request, will be important when there are multiple Supplier FreightPop accounts
				 token = await AuthenticateAync();
			}
			var getShipmentResponse = await MakeRequest($"shipment/getShipment?id={GetDateStringForQuery(daysAgo)}&type={GetShipmentBy.Date}", token).GetAsync().ReceiveJson<Response<List<ShipmentDetails>>>();
			return getShipmentResponse;
		}
		private string GetDateStringForQuery(int daysAgo)
		{
			var dateToQuery = DateTime.Now.AddDays(-daysAgo);
			return dateToQuery.ToString("MM/dd/yyyy");
		}
	}
}

