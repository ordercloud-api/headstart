using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Newtonsoft.Json.Linq;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Headstart.Common.Extensions;
using Headstart.Common.Models.Headstart;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands.SupplierSync.EntityCommands
{
	[SupplierSync("027"), SupplierSync("093"), SupplierSync("129"), SupplierSync("waxinthecitydistribution")]
	public class WaxInTheCityDistributionCommand : ISupplierSyncCommand
	{
		private readonly IOrderCloudClient _ocSeller;
		private readonly WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the WaxInTheCityDistributionCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		public WaxInTheCityDistributionCommand(AppSettings settings)
		{
			try
			{
				_ocSeller = new OrderCloudClient(new OrderCloudClientConfig
				{
					ApiUrl = settings.OrderCloudSettings.ApiUrl,
					AuthUrl = settings.OrderCloudSettings.ApiUrl,
					ClientId = settings.OrderCloudSettings.MiddlewareClientId,
					ClientSecret = settings.OrderCloudSettings.MiddlewareClientSecret,
					Roles = new[]
					{
						ApiRole.FullAccess
					}
				});
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable GetOrderAsync task method
		/// </summary>
		/// <param name="ID"></param>
		/// <param name="orderType"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The JObject response object for the Get Order process</returns>
		public async Task<JObject> GetOrderAsync(string ID, OrderType orderType, DecodedToken decodedToken)
		{
			var returnObject = new JObject { };
			try
			{
				HsShipEstimate estimate;
				HsShipMethod shipMethod = null;
				var supplierWorksheet = await _ocSeller.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Outgoing, ID);

				var buyerWorksheet = await _ocSeller.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, ID.Split('-')[0]);
				var buyerLineItems = buyerWorksheet.GetBuyerLineItemsBySupplierId(supplierWorksheet.Order.ToCompanyID);
				if (buyerWorksheet?.ShipEstimateResponse != null && buyerWorksheet?.ShipEstimateResponse?.ShipEstimates.Count > 0)
				{
					estimate = buyerWorksheet.GetMatchingShipEstimate(supplierWorksheet?.LineItems?.FirstOrDefault()?.ShipFromAddressID);
					shipMethod = estimate?.ShipMethods?.FirstOrDefault(m => m.ID == estimate.SelectedShipMethodID);
				}

				returnObject = new JObject { };
				if (supplierWorksheet.Order != null)
				{
					returnObject.Add(new JProperty("SupplierOrder", new JObject 
					{
						{"Order", JToken.FromObject(supplierWorksheet?.Order)},
						new JProperty("LineItems", JToken.FromObject(supplierWorksheet?.LineItems))
					}));
				}

				if (buyerWorksheet.Order != null)
				{
					returnObject.Add(new JProperty("BuyerOrder", new JObject 
					{
						{"Order", JToken.FromObject(buyerWorksheet?.Order)},
						new JProperty("LineItems", JToken.FromObject(buyerLineItems))
					}));
				}

				if (shipMethod != null)
				{
					returnObject.Add(new JProperty("ShipMethod", JToken.FromObject(shipMethod)));
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return JObject.FromObject(returnObject);
		}

		/// <summary>
		/// Public re-usable ParseProductTemplate task method
		/// </summary>
		/// <param name="file"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The TemplateHydratedProduct response object for the Parse Product Template process</returns>
		/// <exception cref="NotImplementedException"></exception>
		public Task<List<TemplateHydratedProduct>> ParseProductTemplate(IFormFile file, DecodedToken decodedToken)
		{
			throw new NotImplementedException();
		}
	}
}