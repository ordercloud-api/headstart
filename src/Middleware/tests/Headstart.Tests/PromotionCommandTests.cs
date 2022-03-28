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
using System.Threading.Tasks;

namespace Headstart.Tests
{
	class PromotionCommandTests
	{
		[Test, AutoNSubstituteData]
		public async Task should_add_all_promos(
			[Frozen] IOrderCloudClient oc,
			PromotionCommand sut,
			string orderId,
			Task<ListPage<Promotion>> promoList,
			Task<ListPage<OrderPromotion>> orderPromoList
		)
		{
			// Arrange
			oc.Promotions.ListAsync()
				.ReturnsForAnyArgs(promoList);
			oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderId, pageSize: 100)
				.Returns(orderPromoList);

			// Act
			await sut.AutoApplyPromotions(orderId);

			// Assert
			foreach (var promo in promoList.Result.Items)
			{
				await oc.Orders.Received().AddPromotionAsync(OrderDirection.Incoming, orderId, promo.Code);
			}
		}

		[Test, AutoNSubstituteData]
		public async Task should_remove_all_promos(
			[Frozen] IOrderCloudClient oc,
			PromotionCommand sut,
			string orderId,
			Task<ListPage<Promotion>> promoList,
			Task<ListPage<OrderPromotion>> orderPromoList
		)
		{
			// Arrange
			oc.Promotions.ListAsync()
				.ReturnsForAnyArgs(promoList);
			oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderId, pageSize: 100)
				.Returns(orderPromoList);

			// Act
			await sut.AutoApplyPromotions(orderId);

			// Assert
			foreach (var promo in orderPromoList.Result.Items)
			{
				await oc.Orders.Received().RemovePromotionAsync(OrderDirection.Incoming, orderId, promo.Code);
			}
		}

		[Test, AutoNSubstituteData]
		public async Task should_remove_all_distinct_promos(
			[Frozen] IOrderCloudClient oc,
			PromotionCommand sut,
			string orderId,
			Task<ListPage<Promotion>> promoList,
			ListPage<OrderPromotion> orderPromoList
		)
		{
			// a line item promo may be applied multiple times on an order (once for each line item)
			// we only want to remove that promo once else we'll get 404's
			// https://four51.atlassian.net/browse/SEB-1825

			// Arrange
			orderPromoList.Items = orderPromoList.Items.Select(p => { p.ID = "PROMO1"; p.Code = "PROMOCODE1"; return p; }).ToList();
			oc.Promotions.ListAsync()
				.ReturnsForAnyArgs(promoList);
			oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderId, pageSize: 100)
				.Returns(orderPromoList.ToTask());

			// Act
			await sut.AutoApplyPromotions(orderId);

			// Assert
			await oc.Orders.Received(1).RemovePromotionAsync(OrderDirection.Incoming, orderId, "PROMOCODE1");
		}

		[Test, AutoNSubstituteData]
		public void should_throw_if_any_errors_removing_promotions(
			[Frozen] IOrderCloudClient oc,
			PromotionCommand sut,
			string orderId,
			Task<ListPage<Promotion>> promoList,
			Task<Order> promo1Result,
			Task<ListPage<OrderPromotion>> orderPromoList
		)
		{
			// Arrange
			var fixture = new Fixture();
			promoList.Result.Items = fixture.CreateMany<Promotion>(3).ToList();
			oc.Promotions.ListAsync().ReturnsForAnyArgs(promoList);
			if (!string.IsNullOrEmpty(orderId))
			{
				oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderId, pageSize: 100)
					.Returns(orderPromoList);
				var promoCode = Arg.Any<string>();
				if (!string.IsNullOrEmpty(promoCode))
				{
					oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderId, promoCode).Returns(promo1Result, 
						Task.FromException<Order>(new Exception("mockerror1")), 
						Task.FromException<Order>(new Exception("mockerror2")));
				}
			}
			
			// Act
			var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await sut.AutoApplyPromotions(orderId));

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
			string orderId,
			Task<ListPage<Promotion>> promoList,
			Task<ListPage<OrderPromotion>> orderPromoList,
			Task<Order> removePromoResult,
			Task<Order> addPromoResult
		)
		{
			// Arrange
			var fixture = new Fixture();
			oc.Promotions.ListAsync()
				.ReturnsForAnyArgs(promoList);
			orderPromoList.Result.Items = fixture.CreateMany<OrderPromotion>(3).ToList();
			oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderId, pageSize: 100)
				.Returns(orderPromoList);
			oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderId, Arg.Any<string>())
				.Returns(removePromoResult);
			oc.Orders.RemovePromotionAsync(OrderDirection.Incoming, orderId, Arg.Any<string>())
				.Returns(
					addPromoResult,
					Task.FromException<Order>(new Exception("mockerror1")),
					Task.FromException<Order>(new Exception("mockerror2"))
				);

			// Act
			await sut.AutoApplyPromotions(orderId);

			// Assert
			foreach (var promo in promoList.Result.Items)
			{
				await oc.Orders.Received().AddPromotionAsync(OrderDirection.Incoming, orderId, promo.Code);
			}
			foreach (var promo in orderPromoList.Result.Items)
			{
				await oc.Orders.Received().RemovePromotionAsync(OrderDirection.Incoming, orderId, promo.Code);
			}
		}
	}
}
