using AutoFixture;
using AutoFixture.NUnit3;
using Headstart.API.Commands;
using NSubstitute;
using NUnit.Framework;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Tests
{
    class PromotionCommandTests
    {
        [Test, AutoNSubstituteData]
        public async Task should_add_all_promos(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            Task<ListPage<OrderPromotion>> orderpromolist
        )
        {
            // Arrange
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderpromolist);

            // Act
            await sut.AutoApplyPromotions(orderID);

            // Assert
            foreach (var promo in promoList.Result.Items)
            {
                await oc.Orders.Received().AddPromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
            }
        }

        [Test, AutoNSubstituteData]
        public async Task should_remove_all_promos(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            Task<ListPage<OrderPromotion>> orderpromolist
        )
        {
            // Arrange
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderpromolist);

            // Act
            await sut.AutoApplyPromotions(orderID);

            // Assert
            foreach (var promo in orderpromolist.Result.Items)
            {
                await oc.Orders.Received().RemovePromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
            }
        }

        [Test, AutoNSubstituteData]
        public async Task should_remove_all_distinct_promos(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            ListPage<OrderPromotion> orderpromolist
        )
        {
            // a line item promo may be applied multiple times on an order (once for each line item)
            // we only want to remove that promo once else we'll get 404's
            // https://four51.atlassian.net/browse/SEB-1825

            // Arrange
            orderpromolist.Items = orderpromolist.Items.Select(p => { p.ID = "PROMO1"; p.Code = "PROMOCODE1"; return p; }).ToList();
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderpromolist.ToTask());

            // Act
            await sut.AutoApplyPromotions(orderID);

            // Assert
            await oc.Orders.Received(1).RemovePromotionAsync(OrderDirection.Incoming, orderID, "PROMOCODE1");
        }

        [Test, AutoNSubstituteData]
        public void should_throw_if_any_errors_removing_promotions(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            Task<Order> promo1result,
            Task<ListPage<OrderPromotion>> orderpromolist
        )
        {
            // Arrange
            var fixture = new Fixture();
            promoList.Result.Items = fixture.CreateMany<Promotion>(3).ToList();
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderpromolist);
            oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderID, Arg.Any<string>())
                .Returns(
                    promo1result,
                    Task.FromException<Order>(new Exception("mockerror1")),
                    Task.FromException<Order>(new Exception("mockerror2")));

            // Act
            var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await sut.AutoApplyPromotions(orderID));

            // Assert
            Assert.AreEqual("One or more promotions could not be removed", ex.Errors[0].Message);
            Assert.AreEqual("Promotion.ErrorRemovingAll", ex.Errors[0].ErrorCode);

            var innerExceptions = ex.Errors[0].Data.To<IReadOnlyCollection<Exception>>();
            Assert.AreEqual(2, innerExceptions.Count);

            foreach (var e in innerExceptions)
            {
                Assert.IsInstanceOf<CatalystBaseException>(e);
                StringAssert.Contains("Unable to remove promotion", e.Message);
            }
        }

        public async Task should_not_throw_if_errors_adding_promotions(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            Task<ListPage<OrderPromotion>> orderpromolist,
            Task<Order> removePromoResult,
            Task<Order> addPromoResult
        )
        {
            // Arrange
            var fixture = new Fixture();
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            orderpromolist.Result.Items = fixture.CreateMany<OrderPromotion>(3).ToList();
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderpromolist);
            oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderID, Arg.Any<string>())
                .Returns(removePromoResult);
            oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderID, Arg.Any<string>())
                .Returns(
                    addPromoResult,
                    Task.FromException<Order>(new Exception("mockerror1")),
                    Task.FromException<Order>(new Exception("mockerror2"))
                );

            // Act
            await sut.AutoApplyPromotions(orderID);

            // Assert
            foreach (var promo in promoList.Result.Items)
            {
                await oc.Orders.Received().AddPromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
            }
            foreach (var promo in orderpromolist.Result.Items)
            {
                await oc.Orders.Received().RemovePromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
            }
        }
    }
}
