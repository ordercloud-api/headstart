using Flurl.Http;
using Headstart.Common;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderCloud.Catalyst;

namespace Headstart.API.Commands
{
    public interface IPromotionCommand
    {
        Task AutoApplyPromotions(string orderID);
    }

    public class PromotionCommand : IPromotionCommand
    {
        private readonly IOrderCloudClient _oc;
        public PromotionCommand(IOrderCloudClient oc)
        {
            _oc = oc;
        }

        public async Task AutoApplyPromotions(string orderID)
        {
            await RemoveAllPromotionsAsync(orderID);
            var autoEligiblePromos = await _oc.Promotions.ListAsync(filters: "xp.Automatic=true");
            var requests = autoEligiblePromos.Items.Select(p => TryApplyPromoAsync(orderID, p.Code));
            await Task.WhenAll(requests);
        }

        private async Task RemoveAllPromotionsAsync(string orderID)
        {
            // ordercloud does not re-evaluate promotions when line items change
            // we must remove all promos and re-apply them to ensure promotion discounts are accurate
            var promos = await _oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100);
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
                    Data = allTasks.Exception.InnerExceptions
                });
            }
        }

        private async Task<Order> RemovePromoAsync(string orderID, OrderPromotion promo)
        {
            try
            {
                return await _oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
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
                        Promotion = promo
                    }
                });
            }
        }

        private async Task TryApplyPromoAsync(string orderID, string promoCode)
        {
            try
            {
                await _oc.Orders.AddPromotionAsync(OrderDirection.Incoming, orderID, promoCode);
            }
            catch
            {
                // Its not possible to know if a promo is valid without trying to add it
                // if it isn't valid it shouldn't show up as an error to the user
            }
        }
    }
}
