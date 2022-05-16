using System;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands
{
    public interface IPromotionCommand
    {
        Task AutoApplyPromotions(string orderId);
    }

    public class PromotionCommand : IPromotionCommand
    {
        private readonly IOrderCloudClient oc; 
        private readonly AppSettings settings;

        /// <summary>
        /// The IOC based constructor method for the PromotionCommand class object with Dependency Injection
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="settings"></param>
        public PromotionCommand(IOrderCloudClient oc, AppSettings settings)
        {
            try
            {
                this.settings = settings;
                this.oc = oc;
            }
            catch (Exception ex)
            {
                LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
                    LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
            }
        }

        /// <summary>
        /// Public re-usable AutoApplyPromotions task method
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public async Task AutoApplyPromotions(string orderID)
        {
            await RemoveAllPromotionsAsync(orderID);
            var autoEligiblePromos = await oc.Promotions.ListAsync(filters: "xp.Automatic=true");
            var requests = autoEligiblePromos.Items.Select(p => TryApplyPromoAsync(orderID, p.Code));
            await Task.WhenAll(requests);
        }

        /// <summary>
        /// Private re-usable RemoveAllPromotionsAsync task method
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        /// <exception cref="CatalystBaseException"></exception>
        private async Task RemoveAllPromotionsAsync(string orderID)
        {
            // ordercloud does not re-evaluate promotions when line items change
            // we must remove all promos and re-apply them to ensure promotion discounts are accurate
            var promos = await oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100);
            var requests = promos.Items
                .DistinctBy(p => p.ID) // the same promo may be applied to multiple line items on one order
                .Select(p => RemovePromoAsync(orderID, p));
            var allTasks = Task.WhenAll(requests);

            try
            {
                await allTasks;
            }
            catch (Exception)
            {
                throw new CatalystBaseException(new ApiError
                {
                    ErrorCode = "Promotion.ErrorRemovingAll",
                    Message = "One or more promotions could not be removed",
                    Data = allTasks.Exception.InnerExceptions,
                });
            }
        }

        /// <summary>
        /// Private re-usable RemovePromoAsync task method
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="promo"></param>
        /// <returns>The Order object from the RemovePromoAsync process</returns>
        /// <exception cref="CatalystBaseException"></exception>
        private async Task<Order> RemovePromoAsync(string orderID, OrderPromotion promo)
        {
            try
            {
                return await oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
            }
            catch
            {
                // this can leave us in a bad state if the promotion can't be deleted but no longer valid
                throw new CatalystBaseException(new ApiError
                {
                    ErrorCode = "Promotion.ErrorRemoving",
                    Message = $"Unable to remove promotion {promo.Code}",
                    Data = new
                    {
                        Promotion = promo,
                    },
                });
            }
        }

        /// <summary>
        /// Private re-usable TryApplyPromoAsync task method
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="promoCode"></param>
        /// <returns></returns>
        private async Task TryApplyPromoAsync(string orderID, string promoCode)
        {
            try
            {
                await oc.Orders.AddPromotionAsync(OrderDirection.Incoming, orderID, promoCode);
            }
            catch
            {
                // Its not possible to know if a promo is valid without trying to add it
                // if it isn't valid it shouldn't show up as an error to the user
            }
        }
    }
}
