using System;
using System.Linq;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface IPromotionCommand
	{
		Task AutoApplyPromotions(string orderId);
	}

	public class PromotionCommand : IPromotionCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the PromotionCommand class object with Dependency Injection
		/// </summary>
		/// <param name="oc"></param>
		public PromotionCommand(IOrderCloudClient oc)
		{
			try
			{
				_oc = oc;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable AutoApplyPromotions task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		public async Task AutoApplyPromotions(string orderId)
		{
			try
			{
				await RemoveAllPromotionsAsync(orderId);
				var autoEligiblePromos = await _oc.Promotions.ListAsync(filters: "xp.Automatic=true");
				var requests = autoEligiblePromos.Items.Select(p => TryApplyPromoAsync(orderId, p.Code));
				await Task.WhenAll(requests);
			}
			catch (Exception ex)
			{
				var exception = new CatalystBaseException(new ApiError
				{
					ErrorCode = @"Order.ErrorAutoApplyPromotions",
					Message = $@"Unable to auto apply the promotion with for the Order: {orderId}."
				});
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
				throw exception;
			}
		}

		/// <summary>
		/// Private re-usable RemoveAllPromotionsAsync task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		/// <exception cref="CatalystBaseException"></exception>
		private async Task RemoveAllPromotionsAsync(string orderId)
		{
			// ordercloud does not re-evaluate promotions when line items change
			// we must remove all promos and re-apply them to ensure promotion discounts are accurate
			var promos = await _oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderId, pageSize: 100);
			var requests = promos.Items.DistinctBy(p => p.ID).Select(p => RemovePromoAsync(orderId, p)); // the same promo may be applied to multiple line items on one order
			var allTasks = Task.WhenAll(requests);
			try
			{
				await allTasks;
			}
			catch (Exception ex)
			{
				var exception = new CatalystBaseException(new ApiError
				{
					ErrorCode = @"Promotion.ErrorRemovingAll",
					Message = @"One or more promotions could not be removed.",
					Data = allTasks?.Exception?.InnerExceptions
				});
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {exception.Message}", ex.StackTrace, this, true);
				throw exception;
			}
		}

		/// <summary>
		/// Private re-usable RemovePromoAsync task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="promo"></param>
		/// <returns>The Order response object from the RemovePromoAsync process</returns>
		/// <exception cref="CatalystBaseException"></exception>
		private async Task<Order> RemovePromoAsync(string orderId, OrderPromotion promo)
		{
			var resp = new Order();
			try
			{
				resp = await _oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderId, promo.Code);
			}
			catch (Exception ex)
			{
				// This can leave us in a bad state if the promotion can't be deleted but no longer valid
				var exception = new CatalystBaseException(new ApiError
				{
					ErrorCode = @"Promotion.ErrorRemoving",
					Message = $@"Unable to remove the promotion with the PromoCode: {promo.Code}.",
					Data = new
					{
						Promotion = promo
					}
				});
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {exception.Message}", ex.StackTrace, this, true);
				throw exception;
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable TryApplyPromoAsync task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="promoCode"></param>
		/// <returns></returns>
		private async Task TryApplyPromoAsync(string orderId, string promoCode)
		{
			try
			{
				await _oc.Orders.AddPromotionAsync(OrderDirection.Incoming, orderId, promoCode);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}
	}
}