using Flurl.Http;
using Headstart.Common;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Headstart.API.Commands
{
    public interface IPromotionCommand
    {
        Task AutoApplyPromotions(string orderID);
    }

    public class PromotionCommand : IPromotionCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly AppSettings _settings;
        public PromotionCommand(IOrderCloudClient oc, AppSettings settings)
        {
            _oc = oc;
            _settings = settings;
        }

        public async Task AutoApplyPromotions(string orderID)
        {
            try
            {
                await _oc.Orders.ValidateAsync(OrderDirection.Incoming, orderID);
            }
            catch (Exception ex)
            {
                await RemoveOrderPromotions(orderID);
            }

            var ocAuth = await _oc.AuthenticateAsync();

            var autoEligablePromos = await _oc.Promotions.ListAsync(filters: "xp.Automatic=true");
            await Throttler.RunAsync(autoEligablePromos.Items, 100, 5, promo =>
            {
                //  Not useing the sdk here because we need to ignore errors on promos that are not able to be 
                //  applied to the order. We need to try this request on every automatic promo.
                return $"{_settings.OrderCloudSettings.ApiUrl}/v1/orders/Incoming/{orderID}/promotions/{promo.Code}"
                    .WithOAuthBearerToken(ocAuth.AccessToken)
                    .AllowAnyHttpStatus().PostAsync(null);
            });
        }

        private async Task RemoveOrderPromotions(string orderID)
        {
            var curPromos = await _oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID);
            if(curPromos?.Items?.Count > 0)
            {
                var removeQueue = new List<Task>();

                await Throttler.RunAsync(curPromos.Items, 100, 5, promo =>
                {
                    return _oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderID, promo.Code);

                });
            }
        }
    }
}
