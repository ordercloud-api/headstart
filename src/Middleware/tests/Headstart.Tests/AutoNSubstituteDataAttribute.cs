using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;

namespace Headstart.Tests
{
	public class AutoNSubstituteDataAttribute : AutoDataAttribute
	{
		public AutoNSubstituteDataAttribute() : base(() => new Fixture().Customize(new HSCompositeCustomization()))
		{
		}
	}

	public class HSCompositeCustomization : CompositeCustomization
	{
		public HSCompositeCustomization() : base(new OrderCloudModelsCustomization(), new AutoNSubstituteCustomization())
		{
		}
	}


	// Autofixture can't handle ordercloud models that use custom xp
	// due to a bug in the ordercloud sdk  https://github.com/ordercloud-api/ordercloud-dotnet-sdk/issues/60
	// So as a workaround we are manually providing specific overrides for those types of models here
	// if this ever does get fixed in the sdk we can delete this whole customization
	public class OrderCloudModelsCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Customize<HsLocationUserGroup>(c => c
				.With(x => x.xp, fixture.Create<HsLocationUserGroupXp>()));

			fixture.Customize<HsBuyer>(c => c
				.With(x => x.xp, fixture.Create<BuyerXp>()));

			fixture.Customize<HsShipMethod>(c => c
				.With(x => x.xp, fixture.Create<ShipMethodXp>()));

			fixture.Customize<HsShipEstimate>(c => c
				.With(x => x.xp, fixture.Create<ShipEstimateXp>())
				.With(x => x.ShipMethods, fixture.Create<List<HsShipMethod>>()));

			fixture.Customize<HsLineItemProduct>(c => c
				.With(x => x.xp, fixture.Create<ProductXp>()));

			fixture.Customize<HsOrderCalculateResponse>(c => c
				.With(x => x.xp, fixture.Create<OrderCalculateResponseXp>()));

			fixture.Customize<HsShipEstimateResponse>(c => c
				.With(x => x.xp, fixture.Create<ShipEstimateResponseXp>())
				.With(x => x.ShipEstimates, fixture.Create<List<HsShipEstimate>>()));

			fixture.Customize<HsLineItem>(c => c
				.With(x => x.xp, fixture.Create<LineItemXp>())
				.With(x => x.Product, fixture.Create<HsLineItemProduct>())
				.With(x => x.ShippingAddress, fixture.Create<HsAddressBuyer>())
				.With(x => x.ShipFromAddress, fixture.Create<HsAddressSupplier>()));

			fixture.Customize<HsAddressBuyer>(c => c
				.With(x => x.xp, fixture.Create<BuyerAddressXP>()));

			fixture.Customize<HsUser>(c => c
				.With(x => x.xp, fixture.Create<UserXp>()));

			fixture.Customize<HsOrder>(c => c
				.With(x => x.xp, fixture.Create<OrderXp>())
				.With(x => x.FromUser, fixture.Create<HsUser>())
				.With(x => x.BillingAddress, fixture.Create<HsAddressBuyer>()));

			fixture.Customize<HsOrderWorksheet>(c => c
				.With(x => x.Order, fixture.Create<HsOrder>())
				.With(x => x.LineItems, fixture.Create<List<HsLineItem>>())
				.With(x => x.ShipEstimateResponse, fixture.Create<HsShipEstimateResponse>())
				.With(x => x.OrderCalculateResponse, fixture.Create<HsOrderCalculateResponse>()));
		}
	}
}
