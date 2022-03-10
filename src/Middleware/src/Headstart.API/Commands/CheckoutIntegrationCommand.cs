using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Headstart.Common.Constants;
using System.Collections.Generic;
using Headstart.Common.Extensions;
using ordercloud.integrations.library;
using ordercloud.integrations.easypost;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.exchangerates;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface ICheckoutIntegrationCommand
	{
		Task<ShipEstimateResponse> GetRatesAsync(HsOrderCalculatePayload orderCalculatePayload);
		Task<HsOrderCalculateResponse> CalculateOrder(HsOrderCalculatePayload orderCalculatePayload);
		Task<HsOrderCalculateResponse> CalculateOrder(string orderId, DecodedToken decodedToken);
		Task<ShipEstimateResponse> GetRatesAsync(string orderId);
	}

	public class CheckoutIntegrationCommand : ICheckoutIntegrationCommand
	{
		private readonly ITaxCalculator _taxCalculator;
		private readonly IEasyPostShippingService _shippingService;
		private readonly IExchangeRatesCommand _exchangeRates;
		private readonly IOrderCloudClient _oc;
		private readonly IDiscountDistributionService _discountDistribution;
		private readonly HsShippingProfiles _profiles;
		private readonly AppSettings _settings;
		private readonly WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the CheckoutIntegrationCommand class object with Dependency Injection
		/// </summary>
		/// <param name="discountDistribution"></param>
		/// <param name="taxCalculator"></param>
		/// <param name="exchangeRates"></param>
		/// <param name="orderCloud"></param>
		/// <param name="shippingService"></param>
		/// <param name="settings"></param>
		public CheckoutIntegrationCommand(IDiscountDistributionService discountDistribution, ITaxCalculator taxCalculator, IExchangeRatesCommand exchangeRates, IOrderCloudClient orderCloud, IEasyPostShippingService shippingService, AppSettings settings)
		{
			try
			{
				_taxCalculator = taxCalculator;
				_exchangeRates = exchangeRates;
				_oc = orderCloud;
				_shippingService = shippingService;
				_settings = settings;
				_discountDistribution = discountDistribution;
				_profiles = new HsShippingProfiles(_settings);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable GetRatesAsync task method
		/// </summary>
		/// <param name="orderCalculatePayload"></param>
		/// <returns>The ShipEstimateResponse response object from the GetRates process</returns>
		public async Task<ShipEstimateResponse> GetRatesAsync(HsOrderCalculatePayload orderCalculatePayload)
		{
			var resp = new ShipEstimateResponse();
			try
			{
				resp = await this.GetRatesAsync(orderCalculatePayload.OrderWorksheet, orderCalculatePayload.ConfigData);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable GetRatesAsync task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The ShipEstimateResponse response object from the GetRates process</returns>
		public async Task<ShipEstimateResponse> GetRatesAsync(string orderId)
		{
			var resp = new ShipEstimateResponse();
			try
			{
				var order = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, orderId);
				resp = await this.GetRatesAsync(order);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable GetRatesAsync task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="config"></param>
		/// <returns>The HsShipEstimateResponse response object from the GetRates process</returns>
		private async Task<ShipEstimateResponse> GetRatesAsync(HsOrderWorksheet worksheet, CheckoutIntegrationConfiguration config = null)
		{
			var shipResponse = new HsShipEstimateResponse();
			try
			{
				var groupedLineItems = worksheet.LineItems.GroupBy(li => new AddressPair { ShipFrom = li.ShipFromAddress, ShipTo = li.ShippingAddress }).ToList();
				shipResponse = (await _shippingService.GetRates(groupedLineItems, _profiles)).Reserialize<HsShipEstimateResponse>(); // include all accounts at this stage so we can save on order worksheet and analyze 

				// Certain suppliers use certain shipping accounts. This filters available rates based on those accounts.  
				for (var i = 0; i < groupedLineItems.Count; i++)
				{
					var supplierID = groupedLineItems[i].First().SupplierID;
					var profile = _profiles.FirstOrDefault(supplierID);
					var methods = FilterMethodsBySupplierConfig(shipResponse.ShipEstimates[i].ShipMethods.Where(s => profile.CarrierAccountIDs.Contains(s.xp.CarrierAccountId)).ToList(), profile);
					shipResponse.ShipEstimates[i].ShipMethods = methods.Select(s =>
					{

						// there is logic here to support not marking up shipping over list rate. But USPS is always list rate
						// so adding an override to the suppliers that use USPS
						var carrier = _profiles.ShippingProfiles.First(p => p.CarrierAccountIDs.Contains(s.xp?.CarrierAccountId));
						s.Cost = carrier.MarkupOverride ?
							s.xp.OriginalCost * carrier.Markup :
							Math.Min((s.xp.OriginalCost * carrier.Markup), s.xp.ListRate);

						return s;
					}).ToList();
				}

				var buyerCurrency = worksheet.Order.xp.Currency ?? CurrencySymbol.USD;
				await shipResponse.ShipEstimates
					.CheckForEmptyRates(_settings.EasyPostSettings.NoRatesFallbackCost, _settings.EasyPostSettings.NoRatesFallbackTransitDays)
					.ApplyShippingLogic(worksheet, _oc, _settings.EasyPostSettings.FreeShippingTransitDays).Result
					.ConvertCurrency(CurrencySymbol.USD, buyerCurrency, _exchangeRates);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return shipResponse;
		}

		/// <summary>
		/// Public re-usable FilterMethodsBySupplierConfig task method
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="profile"></param>
		/// <returns>The HsShipMethod response object from the FilterMethodsBySupplierConfig process</returns>
		public IEnumerable<HsShipMethod> FilterMethodsBySupplierConfig(List<HsShipMethod> methods, EasyPostShippingProfile profile)
		{
			var resp = new List<HsShipMethod>();
			try
			{
				// will attempt to filter out by supplier method specs, but if there are filters and the result is none and there are valid methods still return the methods
				if (profile.AllowedServiceFilter.Count == 0) return methods;
				var filtered_methods = methods.Where(s => profile.AllowedServiceFilter.Contains(s.Name)).Select(s => s).ToList();
				resp = filtered_methods.Any() ? filtered_methods : methods;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable WhereRateIsCheapestOfItsKind task method
		/// </summary>
		/// <param name="methods"></param>
		/// <returns>The HsShipMethod response object from the WhereRateIsCheapestOfItsKind process</returns>
		public IEnumerable<HsShipMethod> WhereRateIsCheapestOfItsKind(IEnumerable<HsShipMethod> methods)
		{
			var resp = methods;
			try
			{
				resp = methods.GroupBy(method => method.EstimatedTransitDays).Select(kind => kind.OrderBy(method => method.Cost).First());
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable ConvertShippingRatesCurrency task method
		/// </summary>
		/// <param name="shipEstimates"></param>
		/// <param name="shipperCurrency"></param>
		/// <param name="buyerCurrency"></param>
		/// <returns>The list of HsShipEstimate response objects from the ConvertShippingRatesCurrency process</returns>
		private async Task<List<HsShipEstimate>> ConvertShippingRatesCurrency(IList<HsShipEstimate> shipEstimates, CurrencySymbol shipperCurrency, CurrencySymbol buyerCurrency)
		{
			var resp = new List<HsShipEstimate>();
			try
			{
				var rates = (await _exchangeRates.Get(buyerCurrency)).Rates;
				var conversionRate = rates.Find(r => r.Currency == shipperCurrency).Rate;
				resp = shipEstimates.Select(estimate =>
				{
					estimate.ShipMethods = estimate.ShipMethods.Select(method =>
					{
						method.xp.OriginalCurrency = shipperCurrency;
						method.xp.OrderCurrency = buyerCurrency;
						method.xp.ExchangeRate = conversionRate;
						if (conversionRate != null) method.Cost /= (decimal)conversionRate;
						return method;
					}).ToList();
					return estimate;
				}).ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable ApplyFreeShipping task method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <param name="shipEstimates"></param>
		/// <returns>The list of HsShipEstimate response objects from the ApplyFreeShipping process</returns>
		private async Task<List<HsShipEstimate>> ApplyFreeShipping(HsOrderWorksheet orderWorksheet, IList<HsShipEstimate> shipEstimates)
		{
			var updatedEstimates = new List<HsShipEstimate>();
			try
			{
				var supplierIDs = orderWorksheet.LineItems.Select(li => li.SupplierID);
				var suppliers = await _oc.Suppliers.ListAsync<HsSupplier>(filters: $@"ID={string.Join("|", supplierIDs)}");

				foreach (var estimate in shipEstimates)
				{
					//  get supplier and supplier subtotal
					var supplierID = orderWorksheet.LineItems.First(li => li.ID == estimate.ShipEstimateItems.FirstOrDefault()?.LineItemID).SupplierID;
					var supplier = suppliers.Items.FirstOrDefault(s => s.ID == supplierID);
					var supplierLineItems = orderWorksheet.LineItems.Where(li => li.SupplierID == supplier?.ID);
					var supplierSubTotal = supplierLineItems.Select(li => li.LineSubtotal).Sum();
					if (supplier?.xp?.FreeShippingThreshold != null && supplier.xp?.FreeShippingThreshold < supplierSubTotal) // free shipping for this supplier
					{
						foreach (var method in estimate.ShipMethods)
						{
							// free shipping on ground shipping or orders where we weren't able to calculate a shipping rate
							if (method.Name.Contains("GROUND") || method.ID.Equals(ShippingConstants.NoRatesId, StringComparison.OrdinalIgnoreCase))
							{
								method.xp.FreeShippingApplied = true;
								method.xp.FreeShippingThreshold = supplier.xp.FreeShippingThreshold;
								method.Cost = 0;
							}
						}
					}
					updatedEstimates.Add(estimate);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}            
			return updatedEstimates;
		}

		/// <summary>
		/// Public re-usable FilterSlowerRatesWithHighCost method
		/// </summary>
		/// <param name="estimates"></param>
		/// <returns>The list of HsShipEstimate response objects from the FilterSlowerRatesWithHighCost process</returns>
		public IList<HsShipEstimate> FilterSlowerRatesWithHighCost(IList<HsShipEstimate> estimates)
		{
			var resp = new List<HsShipEstimate>();
			try
			{
				// filter out rate estimates with slower transit days and higher costs than faster transit days
				// ex: 3 days for $20 vs 1 day for $10. Filter out the 3 days option

				resp = estimates.Select(estimate =>
				{
					var methodsList = estimate.ShipMethods.OrderBy(m => m.EstimatedTransitDays).ToList();
					var filtered = new List<HsShipMethod>();
					for (var i = methodsList.Count - 1; i >= 0; i--)
					{
						var method = methodsList[i];
						var fasterMethods = methodsList.GetRange(0, i);
						var existsFasterCheaperRate = fasterMethods.Any(m => m.Cost < method.Cost);
						if (!existsFasterCheaperRate)
						{
							filtered.Add(method);
						}
					}
					filtered.Reverse(); // reorder back to original since we looped backwards
					estimate.ShipMethods = filtered;
					return estimate;
				}).ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ApplyFlatRateShipping method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <param name="estimates"></param>
		/// <returns>The list of HsShipEstimate response objects from the ApplyFlatRateShipping process</returns>
		public IList<HsShipEstimate> ApplyFlatRateShipping(HsOrderWorksheet orderWorksheet, IList<HsShipEstimate> estimates)
		{
			var resp = new List<HsShipEstimate>();
			try
			{
				resp = estimates.Select(estimate => ApplyFlatRateShippingOnEstimate(estimate, orderWorksheet)).ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ApplyFlatRateShippingOnEstimate method
		/// </summary>
		/// <param name="estimate"></param>
		/// <param name="orderWorksheet"></param>
		/// <returns>The HsShipEstimate response object from the ApplyFlatRateShippingOnEstimate process</returns>
		public HsShipEstimate ApplyFlatRateShippingOnEstimate(HsShipEstimate estimate, HsOrderWorksheet orderWorksheet)
		{
			try
			{
				var supplierID = orderWorksheet.LineItems.First(li => li.ID == estimate.ShipEstimateItems.FirstOrDefault()?.LineItemID).SupplierID;
				var supplierLineItems = orderWorksheet.LineItems.Where(li => li.SupplierID == supplierID);
				var supplierSubTotal = supplierLineItems.Select(li => li.LineSubtotal).Sum();
				var qualifiesForFlatRateShipping = supplierSubTotal > .01M && estimate.ShipMethods.Any(method => method.Name.Contains("GROUND"));
				if (qualifiesForFlatRateShipping)
				{
					estimate.ShipMethods = estimate.ShipMethods.Where(method => method.Name.Contains("GROUND")) // flat rate shipping only applies to ground shipping methods
						.Select(method => ApplyFlatRateShippingOnShipMethod(method, supplierSubTotal)).ToList();
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return estimate;
		}

		/// <summary>
		/// Public re-usable ApplyFlatRateShippingOnShipMethod method
		/// </summary>
		/// <param name="method"></param>
		/// <param name="supplierSubTotal"></param>
		/// <returns>The HsShipMethod response object from the ApplyFlatRateShippingOnShipMethod process</returns>
		public HsShipMethod ApplyFlatRateShippingOnShipMethod(HsShipMethod method, decimal supplierSubTotal)
		{
			try
			{
				if (supplierSubTotal > .01M && supplierSubTotal <= 499.99M)
				{
					method.Cost = 29.99M;
				}
				else if (supplierSubTotal > 499.9M)
				{
					method.Cost = 0;
					method.xp.FreeShippingApplied = true;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return method;
		}

		/// <summary>
		/// Public re-usable CheckForEmptyRates method
		/// </summary>
		/// <param name="estimates"></param>
		/// <param name="noRatesCost"></param>
		/// <param name="noRatesTransitDays"></param>
		/// <returns>The list of HsShipMethod response objects from the CheckForEmptyRates process</returns>
		public IList<HsShipEstimate> CheckForEmptyRates(IList<HsShipEstimate> estimates, decimal noRatesCost, int noRatesTransitDays)
		{
			try
			{
				// if there are no rates for a set of line items then return a mocked response so user can check out
				// this order will additionally get marked as needing attention

				foreach (var shipEstimate in estimates)
				{
					if (!shipEstimate.ShipMethods.Any())
					{
						shipEstimate.ShipMethods = new List<HsShipMethod>()
						{
							new HsShipMethod
							{
								ID = ShippingConstants.NoRatesId,
								Name = @"No shipping rates",
								Cost = noRatesCost,
								EstimatedTransitDays = noRatesTransitDays,
								xp = new ShipMethodXp
								{
									OriginalCost = noRatesCost
								}
							}
						};
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return estimates;
		}

		/// <summary>
		/// Public re-usable UpdateFreeShippingRates method
		/// </summary>
		/// <param name="estimates"></param>
		/// <param name="freeShippingTransitDays"></param>
		/// <returns>The list of HsShipEstimate response objects from the UpdateFreeShippingRates process</returns>
		public IList<HsShipEstimate> UpdateFreeShippingRates(IList<HsShipEstimate> estimates, int freeShippingTransitDays)
		{
			try
			{
				foreach (var shipEstimate in estimates)
				{
					if (shipEstimate.ID.StartsWith(ShippingConstants.FreeShippingId))
					{
						foreach (var method in shipEstimate.ShipMethods)
						{
							method.ID = shipEstimate.ID;
							method.Cost = 0;
							method.EstimatedTransitDays = freeShippingTransitDays;
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return estimates;
		}

		/// <summary>
		/// Public re-usable CalculateOrder task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HsOrderCalculateResponse response object from the CalculateOrder process</returns>
		public async Task<HsOrderCalculateResponse> CalculateOrder(string orderId, DecodedToken decodedToken)
		{
			var resp = new HsOrderCalculateResponse();
			try
			{
				var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, orderId, decodedToken.AccessToken);
				resp = await this.CalculateOrder(new HsOrderCalculatePayload()
				{
					ConfigData = null,
					OrderWorksheet = worksheet
				});
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable CalculateOrder task method
		/// </summary>
		/// <param name="orderCalculatePayload"></param>
		/// <returns>The HsOrderCalculateResponse response object from the CalculateOrder process</returns>
		public async Task<HsOrderCalculateResponse> CalculateOrder(HsOrderCalculatePayload orderCalculatePayload)
		{
			var resp = new HsOrderCalculateResponse();
			try
			{
				if (orderCalculatePayload.OrderWorksheet?.Order?.xp != null && orderCalculatePayload.OrderWorksheet.Order.xp.OrderType == OrderType.Quote)
				{
					// Quote orders do not have tax cost associated with them
					return new HsOrderCalculateResponse();
				}
				else
				{
					var promotions = await _oc.Orders.ListAllPromotionsAsync(OrderDirection.All, orderCalculatePayload.OrderWorksheet.Order.ID);
					var promoCalculationTask = _discountDistribution.SetLineItemProportionalDiscount(orderCalculatePayload.OrderWorksheet, promotions);
					var taxCalculationTask = _taxCalculator.CalculateEstimateAsync(orderCalculatePayload.OrderWorksheet.Reserialize<OrderWorksheet>(), promotions);
					var taxCalculation = await taxCalculationTask;
					await promoCalculationTask;

					return new HsOrderCalculateResponse
					{
						TaxTotal = taxCalculation.TotalTax,
						xp = new OrderCalculateResponseXp()
						{
							TaxCalculation = taxCalculation
						}
					};
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}
	}
}