using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Models;
using Headstart.Common;
using OrderCloud.Catalyst;
using Newtonsoft.Json.Linq;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Headstart.Common.Extensions;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Headstart.Common.Services.ShippingIntegration.Models;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extenstions;

namespace Headstart.API.Commands
{
    [SupplierSync("Generic")]
    public class GenericSupplierCommand : ISupplierSyncCommand
    {
        private readonly IOrderCloudClient _ocSeller; 
        private WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

        /// <summary>
        /// The IOC based constructor method for the GenericSupplierCommand class object with Dependency Injection
        /// </summary>
        /// <param name="settings"></param>
        public GenericSupplierCommand(AppSettings settings)
        {
            try
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
                //TODO: BaseUrl cannot be found here
                var ocAuth = await _ocSeller.AuthenticateAsync();
                HSShipEstimate estimate;
                HSShipMethod ship_method = null;
                HSOrderWorksheet supplierWorksheet = null;

                // Supplier worksheet will not exist on quote orders.
                try
                {
                    supplierWorksheet = await _ocSeller.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, ID, ocAuth.AccessToken);
                }
                catch (OrderCloudException ex) 
                {
                    LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                }

                var salesOrderID = orderType == OrderType.Standard ? ID.Split('-')[0] : ID;
                var buyerWorksheet = await _ocSeller.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, salesOrderID, ocAuth.AccessToken);
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

                returnObject = new JObject { };
                if (supplierWorksheet?.Order != null)
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

                    // No supplier worksheet exists in these scenarios, just treat buyer order as supplier order.
                    if (buyerWorksheet.Order.xp?.OrderType == OrderType.Quote)
                    {
                        returnObject.Add(new JProperty("SupplierOrder", new JObject 
                        {
                            {"Order", JToken.FromObject(buyerWorksheet?.Order)},
                            new JProperty("LineItems", JToken.FromObject(buyerWorksheet?.LineItems))
                        }));
                    }
                }

                if (ship_method != null)
                {
                    returnObject.Add(new JProperty("ShipMethod", JToken.FromObject(ship_method)));
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