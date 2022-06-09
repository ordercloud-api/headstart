using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using Headstart.API.Commands;
using Headstart.Common.Extensions;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Tests
{
    public class PromotionCommandTests
    {
        [Test, AutoNSubstituteData]
        public async Task AutoApplyPromotions_WithValidOrderID_AddsAllAutomaticPromotionsToOrder(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            Task<ListPage<OrderPromotion>> orderPromoList)
        {
            // Arrange
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderPromoList);

            // Act
            await sut.AutoApplyPromotions(orderID);

            // Assert
            foreach (var promo in promoList.Result.Items)
            {
                await oc.Orders.Received().AddPromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
            }
        }

        [Test, AutoNSubstituteData]
        public async Task AutoApplyPromotions_WithValidOrderID_RemovesExistingAutomaticPromotionsToOrder(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            Task<ListPage<OrderPromotion>> orderPromoList)
        {
            // Arrange
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderPromoList);

            // Act
            await sut.AutoApplyPromotions(orderID);

            // Assert
            foreach (var promo in orderPromoList.Result.Items)
            {
                await oc.Orders.Received().RemovePromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
            }
        }

        [Test, AutoNSubstituteData]
        public async Task AutoApplyPromotions_WithValidOrderID_RemovesMultiLinePromotionsOnce(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            ListPage<OrderPromotion> orderPromoList)
        {
            // a line item promo may be applied multiple times on an order (once for each line item)
            // we only want to remove that promo once else we'll get 404's
            // https://four51.atlassian.net/browse/SEB-1825

            // Arrange
            orderPromoList.Items = orderPromoList.Items.Select(p =>
            {
                p.ID = "PROMO1";
                p.Code = "PROMOCODE1";
                return p;
            }).ToList();
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderPromoList.ToTask());

            // Act
            await sut.AutoApplyPromotions(orderID);

            // Assert
            await oc.Orders.Received(1).RemovePromotionAsync(OrderDirection.Incoming, orderID, "PROMOCODE1");
        }

        [Test, AutoNSubstituteData]
        public void AutoApplyPromotions_ErrorsDuringRemovingPromotions_ThrowsException(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            Task<Order> promo1Result,
            Task<ListPage<OrderPromotion>> orderPromoList)
        {
            // Arrange
            var fixture = new Fixture();
            promoList.Result.Items = fixture.CreateMany<Promotion>(3).ToList();
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderPromoList);
            oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderID, Arg.Any<string>())
                .Returns(
                    promo1Result,
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

        public async Task AutoApplyPromotions_ErrorsDuringAddingPromotions_DoesNotThrowException(
            [Frozen] IOrderCloudClient oc,
            PromotionCommand sut,
            string orderID,
            Task<ListPage<Promotion>> promoList,
            Task<ListPage<OrderPromotion>> orderPromoList,
            Task<Order> removePromoResult,
            Task<Order> addPromoResult)
        {
            // Arrange
            var fixture = new Fixture();
            oc.Promotions.ListAsync()
                .ReturnsForAnyArgs(promoList);
            orderPromoList.Result.Items = fixture.CreateMany<OrderPromotion>(3).ToList();
            oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID, pageSize: 100)
                .Returns(orderPromoList);
            oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderID, Arg.Any<string>())
                .Returns(removePromoResult);
            oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderID, Arg.Any<string>())
                .Returns(
                    addPromoResult,
                    Task.FromException<Order>(new Exception("mockerror1")),
                    Task.FromException<Order>(new Exception("mockerror2")));

            // Act
            await sut.AutoApplyPromotions(orderID);

            // Assert
            foreach (var promo in promoList.Result.Items)
            {
                await oc.Orders.Received().AddPromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
            }

            foreach (var promo in orderPromoList.Result.Items)
            {
                await oc.Orders.Received().RemovePromotionAsync(OrderDirection.Incoming, orderID, promo.Code);
            }
        }
    }
}
