using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Extensions;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands
{
	[SupplierSync("Generic")]
	public class GenericSupplierCommand : ISupplierSyncCommand
	{
		private readonly IOrderCloudClient ocSeller;
		private readonly AppSettings settings;

		/// <summary>
		/// The IOC based constructor method for the GenericSupplierCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		public GenericSupplierCommand(AppSettings settings)
		{
			try
			{
				this.settings = settings;
				ocSeller = new OrderCloudClient(new OrderCloudClientConfig
				{
					ApiUrl = settings.OrderCloudSettings.ApiUrl,
					AuthUrl = settings.OrderCloudSettings.ApiUrl,
					ClientId = settings.OrderCloudSettings.MiddlewareClientID,
					ClientSecret = settings.OrderCloudSettings.MiddlewareClientSecret,
					Roles = new[]
					{
						ApiRole.FullAccess,
					},
				});
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable GetOrderAsync task method
		/// </summary>
		/// <param name="id"></param>
		/// <param name="orderType"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The JObject object for the Get Order process</returns>
		public async Task<JObject> GetOrderAsync(string id, OrderType orderType, DecodedToken decodedToken)
		{
			//TODO: BaseUrl cannot be found here
			var ocAuth = await ocSeller.AuthenticateAsync();
			HSShipEstimate estimate;
			HSShipMethod ship_method = null;
			HSOrderWorksheet supplierWorksheet = null;

			// Supplier worksheet will not exist on quote orders.
			try
			{
				supplierWorksheet = await ocSeller.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, id, ocAuth.AccessToken);
			}
			catch (OrderCloudException)
			{
			}

			var salesOrderID = orderType == OrderType.Standard ? id.Split('-')[0] : id;
			var buyerWorksheet = await ocSeller.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, salesOrderID, ocAuth.AccessToken);
			var supplierID = supplierWorksheet?.Order?.ToCompanyID;
			if (buyerWorksheet.Order.xp?.OrderType == OrderType.Quote)
			{
				supplierID = buyerWorksheet.LineItems[0].SupplierID;
			}

			var buyerLineItems = buyerWorksheet.GetBuyerLineItemsBySupplierID(supplierID);
			if (buyerWorksheet?.ShipEstimateResponse != null && buyerWorksheet?.ShipEstimateResponse?.ShipEstimates.Count > 0)
			{
				estimate = buyerWorksheet.GetMatchingShipEstimate(supplierWorksheet?.LineItems?.FirstOrDefault()?.ShipFromAddressID);
				ship_method = estimate?.ShipMethods?.FirstOrDefault(m => m.ID == estimate.SelectedShipMethodID);
			}

			var returnObject = new JObject { };
			if (supplierWorksheet?.Order != null)
			{
				returnObject.Add(new JProperty("SupplierOrder", new JObject 
				{
					{"Order", JToken.FromObject(supplierWorksheet?.Order)},
					new JProperty("LineItems", JToken.FromObject(supplierWorksheet?.LineItems)),
				}));
			}

			if (buyerWorksheet.Order != null)
			{
				returnObject.Add(new JProperty("BuyerOrder", new JObject 
				{
					{"Order", JToken.FromObject(buyerWorksheet?.Order)},
					new JProperty("LineItems", JToken.FromObject(buyerLineItems)),
				}));

				// No supplier worksheet exists in these scenarios, just treat buyer order as supplier order.
				if (buyerWorksheet.Order.xp?.OrderType == OrderType.Quote)
				{
					returnObject.Add(new JProperty("SupplierOrder", new JObject 
					{
						{"Order", JToken.FromObject(buyerWorksheet?.Order)},
						new JProperty("LineItems", JToken.FromObject(buyerWorksheet?.LineItems)),
					}));
				}
			}

			if (ship_method != null)
			{
				returnObject.Add(new JProperty("ShipMethod", JToken.FromObject(ship_method)));
			}
			return JObject.FromObject(returnObject);
		}

		/// <summary>
		/// Public re-usable ParseProductTemplate task method
		/// </summary>
		/// <param name="file"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The TemplateHydratedProduct object for the Parse Product Template process</returns>
		/// <exception cref="NotImplementedException"></exception>
		public Task<List<TemplateHydratedProduct>> ParseProductTemplate(IFormFile file, DecodedToken decodedToken)
		{
			throw new System.NotImplementedException();
		}
	}
}
