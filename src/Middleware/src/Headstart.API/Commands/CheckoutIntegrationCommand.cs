using Headstart.Common;
using Headstart.Common.Constants;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using ordercloud.integrations.easypost;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Headstart.API.Commands
{
	public interface ICheckoutIntegrationCommand
	{
		Task<ShipEstimateResponse> GetRatesAsync(HSOrderCalculatePayload orderCalculatePayload);
		Task<HSOrderCalculateResponse> CalculateOrder(HSOrderCalculatePayload orderCalculatePayload);
		Task<HSOrderCalculateResponse> CalculateOrder(string orderID, DecodedToken decodedToken);
		Task<ShipEstimateResponse> GetRatesAsync(string orderID);
	}

	public class CheckoutIntegrationCommand : ICheckoutIntegrationCommand
	{
		private readonly ordercloud.integrations.library.ITaxCalculator _taxCalculator;
		private readonly IEasyPostShippingService _shippingService;
		private readonly IExchangeRatesCommand _exchangeRates;
		private readonly IOrderCloudClient _oc;
		private readonly IDiscountDistributionService _discountDistribution;
		private readonly HSShippingProfiles _profiles;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the CheckoutIntegrationCommand class object with Dependency Injection
		/// </summary>
		/// <param name="discountDistribution"></param>
		/// <param name="taxCalculator"></param>
		/// <param name="exchangeRates"></param>
		/// <param name="orderCloud"></param>
		/// <param name="shippingService"></param>
		/// <param name="settings"></param>
		public CheckoutIntegrationCommand(IDiscountDistributionService discountDistribution, ordercloud.integrations.library.ITaxCalculator taxCalculator, IExchangeRatesCommand exchangeRates, IOrderCloudClient orderCloud, IEasyPostShippingService shippingService, AppSettings settings)
		{
			try
			{
				_taxCalculator = taxCalculator;
				_exchangeRates = exchangeRates;
				_oc = orderCloud;
				_shippingService = shippingService;
				_settings = settings;
				_discountDistribution = discountDistribution;
				_profiles = new HSShippingProfiles(_settings);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable GetRatesAsync task method
		/// </summary>
		/// <param name="orderCalculatePayload"></param>
		/// <returns>The ShipEstimateResponse response object from the GetRates process</returns>
		public async Task<ShipEstimateResponse> GetRatesAsync(HSOrderCalculatePayload orderCalculatePayload)
		{
			return await this.GetRatesAsync(orderCalculatePayload.OrderWorksheet, orderCalculatePayload.ConfigData);
		}

		/// <summary>
		/// Public re-usable GetRatesAsync task method
		/// </summary>
		/// <param name="orderID"></param>
		/// <returns>The ShipEstimateResponse response object from the GetRates process</returns>
		public async Task<ShipEstimateResponse> GetRatesAsync(string orderID)
		{
			var order = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
			return await this.GetRatesAsync(order);
		}

		/// <summary>
		/// Private re-usable GetRatesAsync task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="config"></param>
		/// <returns>The ShipEstimateResponse object from the GetRates process</returns>
		private async Task<ShipEstimateResponse> GetRatesAsync(HSOrderWorksheet worksheet, CheckoutIntegrationConfiguration config = null)
		{
			var groupedLineItems = worksheet.LineItems.GroupBy(li => new AddressPair { ShipFrom = li.ShipFromAddress, ShipTo = li.ShippingAddress }).ToList();
			var shipResponse = (await _shippingService.GetRates(groupedLineItems, _profiles)).Reserialize<HSShipEstimateResponse>(); // include all accounts at this stage so we can save on order worksheet and analyze 

			// Certain suppliers use certain shipping accounts. This filters available rates based on those accounts.  
			for (var i = 0; i < groupedLineItems.Count; i++)
			{
				var supplierID = groupedLineItems[i].First().SupplierID;
				var profile = _profiles.FirstOrDefault(supplierID);
				var methods = FilterMethodsBySupplierConfig(shipResponse.ShipEstimates[i].ShipMethods.Where(s => profile.CarrierAccountIDs.Contains(s.xp.CarrierAccountID)).ToList(), profile);
				shipResponse.ShipEstimates[i].ShipMethods = methods.Select(s =>
				{
					// there is logic here to support not marking up shipping over list rate. But USPS is always list rate
					// so adding an override to the suppliers that use USPS
					var carrier = _profiles.ShippingProfiles.First(p => p.CarrierAccountIDs.Contains(s.xp?.CarrierAccountID));
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
			return shipResponse;
		}

		/// <summary>
		/// Public re-usable FilterMethodsBySupplierConfig task method
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="profile"></param>
		/// <returns>The HSShipMethod object from the FilterMethodsBySupplierConfig process</returns>
		public IEnumerable<HSShipMethod> FilterMethodsBySupplierConfig(List<HSShipMethod> methods, EasyPostShippingProfile profile)
		{
			// will attempt to filter out by supplier method specs, but if there are filters and the result is none and there are valid methods still return the methods
			if (profile.AllowedServiceFilter.Count == 0) return methods;
			var filtered_methods = methods.Where(s => profile.AllowedServiceFilter.Contains(s.Name)).Select(s => s).ToList();
			return filtered_methods.Any() ? filtered_methods : methods;
		}

		/// <summary>
		/// Public re-usable WhereRateIsCheapestOfItsKind task method
		/// </summary>
		/// <param name="methods"></param>
		/// <returns>The HSShipMethod response object from the WhereRateIsCheapestOfItsKind process</returns>
		public IEnumerable<HSShipMethod> WhereRateIsCheapestOfItsKind(IEnumerable<HSShipMethod> methods)
		{
			return methods
				.GroupBy(method => method.EstimatedTransitDays)
				.Select(kind => kind.OrderBy(method => method.Cost).First());
		}

		/// <summary>
		/// Private re-usable ConvertShippingRatesCurrency task method
		/// </summary>
		/// <param name="shipEstimates"></param>
		/// <param name="shipperCurrency"></param>
		/// <param name="buyerCurrency"></param>
		/// <returns>The list of HSShipEstimate objects from the ConvertShippingRatesCurrency process</returns>
		private async Task<List<HSShipEstimate>> ConvertShippingRatesCurrency(IList<HSShipEstimate> shipEstimates, CurrencySymbol shipperCurrency, CurrencySymbol buyerCurrency)
		{
			var rates = (await _exchangeRates.Get(buyerCurrency)).Rates;
			var conversionRate = rates.Find(r => r.Currency == shipperCurrency).Rate;
			return shipEstimates.Select(estimate =>
			{
				estimate.ShipMethods = estimate.ShipMethods.Select(method =>
				{
					method.xp.OriginalCurrency = shipperCurrency;
					method.xp.OrderCurrency = buyerCurrency;
					method.xp.ExchangeRate = conversionRate;
					if (conversionRate != null) method.Cost /= (decimal) conversionRate;
					return method;
				}).ToList();
				return estimate;
			}).ToList();
		}

		/// <summary>
		/// Private re-usable ApplyFreeShipping task method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <param name="shipEstimates"></param>
		/// <returns>The list of HSShipEstimate objects from the ApplyFreeShipping process</returns>
		private async Task<List<HSShipEstimate>> ApplyFreeShipping(HSOrderWorksheet orderWorksheet, IList<HSShipEstimate> shipEstimates)
		{
			var supplierIDs = orderWorksheet.LineItems.Select(li => li.SupplierID);
			var suppliers = await _oc.Suppliers.ListAsync<HSSupplier>(filters: $"ID={string.Join("|", supplierIDs)}");
			var updatedEstimates = new List<HSShipEstimate>();

			foreach(var estimate in shipEstimates)
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
						if (method.Name.Contains("GROUND") || method.ID == ShippingConstants.NoRatesID)
						{
							method.xp.FreeShippingApplied = true;
							method.xp.FreeShippingThreshold = supplier.xp.FreeShippingThreshold;
							method.Cost = 0;
						}
					}
				}
				updatedEstimates.Add(estimate);
			}
			return updatedEstimates;
		}

		/// <summary>
		/// Public re-usable FilterSlowerRatesWithHighCost method
		/// </summary>
		/// <param name="estimates"></param>
		/// <returns>The list of HSShipEstimate objects from the FilterSlowerRatesWithHighCost process</returns>
		public static IList<HSShipEstimate> FilterSlowerRatesWithHighCost(IList<HSShipEstimate> estimates)
		{
			// filter out rate estimates with slower transit days and higher costs than faster transit days
			// ex: 3 days for $20 vs 1 day for $10. Filter out the 3 days option

			var result = estimates.Select(estimate =>
			{
				var methodsList = estimate.ShipMethods.OrderBy(m => m.EstimatedTransitDays).ToList();
				var filtered = new List<HSShipMethod>();
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

			return result;
		}

		/// <summary>
		/// Public re-usable ApplyFlatRateShipping method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <param name="estimates"></param>
		/// <returns>The list of HSShipEstimate objects from the ApplyFlatRateShipping process</returns>
		public static IList<HSShipEstimate> ApplyFlatRateShipping(HSOrderWorksheet orderWorksheet, IList<HSShipEstimate> estimates)
		{
			var result = estimates.Select(estimate => ApplyFlatRateShippingOnEstimate(estimate, orderWorksheet)).ToList();
			return result;
		}

		/// <summary>
		/// Public re-usable ApplyFlatRateShippingOnEstimate method
		/// </summary>
		/// <param name="estimate"></param>
		/// <param name="orderWorksheet"></param>
		/// <returns>The HSShipEstimate object from the ApplyFlatRateShippingOnEstimate process</returns>
		public static HSShipEstimate ApplyFlatRateShippingOnEstimate(HSShipEstimate estimate, HSOrderWorksheet orderWorksheet)
		{
			var supplierID = orderWorksheet.LineItems.First(li => li.ID == estimate.ShipEstimateItems.FirstOrDefault()?.LineItemID).SupplierID;
           	
			var supplierLineItems = orderWorksheet.LineItems.Where(li => li.SupplierID == supplierID);
			var supplierSubTotal = supplierLineItems.Select(li => li.LineSubtotal).Sum();
			var qualifiesForFlatRateShipping = supplierSubTotal > .01M && estimate.ShipMethods.Any(method => method.Name.Contains("GROUND"));
			if(qualifiesForFlatRateShipping)
			{
				estimate.ShipMethods = estimate.ShipMethods
					.Where(method => method.Name.Contains("GROUND")) // flat rate shipping only applies to ground shipping methods
					.Select(method => ApplyFlatRateShippingOnShipmethod(method, supplierSubTotal))
					.ToList();
			}
			return estimate;
		}

		/// <summary>
		/// Public re-usable ApplyFlatRateShippingOnShipMethod method
		/// </summary>
		/// <param name="method"></param>
		/// <param name="supplierSubTotal"></param>
		/// <returns>The HSShipMethod object from the ApplyFlatRateShippingOnShipMethod process</returns>
		public static HSShipMethod ApplyFlatRateShippingOnShipmethod(HSShipMethod method, decimal supplierSubTotal)
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

			return method;
		}

		/// <summary>
		/// Public re-usable CheckForEmptyRates method
		/// </summary>
		/// <param name="estimates"></param>
		/// <param name="noRatesCost"></param>
		/// <param name="noRatesTransitDays"></param>
		/// <returns>The list of HSShipEstimate objects from the CheckForEmptyRates process</returns>
		public static IList<HSShipEstimate> CheckForEmptyRates(IList<HSShipEstimate> estimates, decimal noRatesCost, int noRatesTransitDays)
		{
			// if there are no rates for a set of line items then return a mocked response so user can check out
			// this order will additionally get marked as needing attention

			foreach (var shipEstimate in estimates)
			{
				if (!shipEstimate.ShipMethods.Any())
				{
					shipEstimate.ShipMethods = new List<HSShipMethod>()
					{
						new HSShipMethod
						{
							ID = ShippingConstants.NoRatesID,
							Name = "No shipping rates",
							Cost = noRatesCost,
							EstimatedTransitDays = noRatesTransitDays,
							xp = new ShipMethodXP
							{
								OriginalCost = noRatesCost
							}
						}
					};
				}
			}
			return estimates;
		}

		/// <summary>
		/// Public re-usable UpdateFreeShippingRates method
		/// </summary>
		/// <param name="estimates"></param>
		/// <param name="freeShippingTransitDays"></param>
		/// <returns>The list of HSShipEstimate objects from the UpdateFreeShippingRates process</returns>
		public static IList<HSShipEstimate> UpdateFreeShippingRates(IList<HSShipEstimate> estimates, int freeShippingTransitDays)
		{
			foreach (var shipEstimate in estimates)
			{
				if (shipEstimate.ID.StartsWith(ShippingConstants.FreeShippingID))
				{
					foreach (var method in shipEstimate.ShipMethods)
					{
						method.ID = shipEstimate.ID;
						method.Cost = 0;
						method.EstimatedTransitDays = freeShippingTransitDays;
					}
				}
			}
			return estimates;
		}

		/// <summary>
		/// Public re-usable CalculateOrder task method
		/// </summary>
		/// <param name="orderID"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HSOrderCalculateResponse response object from the CalculateOrder process</returns>
		public async Task<HSOrderCalculateResponse> CalculateOrder(string orderID, DecodedToken decodedToken)
		{
			var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID, decodedToken.AccessToken);
			return await this.CalculateOrder(new HSOrderCalculatePayload()
			{
				ConfigData = null,
				OrderWorksheet = worksheet
			});
		}

		/// <summary>
		/// Public re-usable CalculateOrder task method
		/// </summary>
		/// <param name="orderCalculatePayload"></param>
		/// <returns>The HSOrderCalculateResponse response object from the CalculateOrder process</returns>
		public async Task<HSOrderCalculateResponse> CalculateOrder(HSOrderCalculatePayload orderCalculatePayload)
		{
			if (orderCalculatePayload.OrderWorksheet.Order.xp != null && orderCalculatePayload.OrderWorksheet.Order.xp.OrderType == OrderType.Quote)
			{
				// quote orders do not have tax cost associated with them
				return new HSOrderCalculateResponse();
			} else
			{
				var promotions = await _oc.Orders.ListAllPromotionsAsync(OrderDirection.All, orderCalculatePayload.OrderWorksheet.Order.ID);
				var promoCalculationTask = _discountDistribution.SetLineItemProportionalDiscount(orderCalculatePayload.OrderWorksheet, promotions);
				var taxCalculationTask = _taxCalculator.CalculateEstimateAsync(orderCalculatePayload.OrderWorksheet.Reserialize<OrderWorksheet>(), promotions);
				var taxCalculation = await taxCalculationTask;
				await promoCalculationTask;

				return new HSOrderCalculateResponse
				{
					TaxTotal = taxCalculation.TotalTax,
					xp = new OrderCalculateResponseXp()
					{
						TaxCalculation = taxCalculation
					}
				};
			}
		}
	}
}