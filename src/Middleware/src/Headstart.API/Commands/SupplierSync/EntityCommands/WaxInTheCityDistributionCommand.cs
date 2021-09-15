﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Extensions;
using Headstart.Common.Services.ShippingIntegration.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.API.Commands
{
    [SupplierSync("027"), SupplierSync("093"), SupplierSync("129"), SupplierSync("waxinthecitydistribution")]
    public class WaxInTheCityDistributionCommand : ISupplierSyncCommand
    {
        private readonly IOrderCloudClient _ocSeller;

        public WaxInTheCityDistributionCommand(AppSettings settings)
        {
            _ocSeller = new OrderCloudClient(new OrderCloudClientConfig
            {
                ApiUrl = settings.OrderCloudSettings.ApiUrl,
                AuthUrl = settings.OrderCloudSettings.ApiUrl,
                ClientId = settings.OrderCloudSettings.MiddlewareClientID,
                ClientSecret = settings.OrderCloudSettings.MiddlewareClientSecret,
                Roles = new[]
                {
                    ApiRole.FullAccess
                }
            });
        }

        public async Task<JObject> GetOrderAsync(string ID, DecodedToken decodedToken)
        {
            HSShipEstimate estimate;
            HSShipMethod ship_method = null;
            var supplierWorksheet = await _ocSeller.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, ID);

            var buyerWorksheet = await _ocSeller.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, ID.Split('-')[0]);
            var buyerLineItems = buyerWorksheet.GetBuyerLineItemsBySupplierID(supplierWorksheet.Order.ToCompanyID);
            if (buyerWorksheet?.ShipEstimateResponse != null && buyerWorksheet?.ShipEstimateResponse?.ShipEstimates.Count > 0)
            {
                estimate = buyerWorksheet.GetMatchingShipEstimate(supplierWorksheet?.LineItems?.FirstOrDefault()?.ShipFromAddressID);
                ship_method = estimate?.ShipMethods?.FirstOrDefault(m => m.ID == estimate.SelectedShipMethodID);

            }

            var returnObject = new JObject { };

            if (supplierWorksheet.Order != null)
            {
                returnObject.Add(new JProperty("SupplierOrder", new JObject {
                    {"Order", JToken.FromObject(supplierWorksheet?.Order)},
                    new JProperty("LineItems", JToken.FromObject(supplierWorksheet?.LineItems))
                }));
            }

            if (buyerWorksheet.Order != null)
            {
                returnObject.Add(new JProperty("BuyerOrder", new JObject {
                    {"Order", JToken.FromObject(buyerWorksheet?.Order)},
                    new JProperty("LineItems", JToken.FromObject(buyerLineItems))
                }));
            }

            if (ship_method != null)
            {
                returnObject.Add(new JProperty("ShipMethod", JToken.FromObject(ship_method)));
            }

            return JObject.FromObject(returnObject);
        }

        public Task<List<TemplateHydratedProduct>> ParseProductTemplate(IFormFile file, DecodedToken decodedToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
