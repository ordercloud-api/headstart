using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture.AutoNSubstitute;
using AutoFixture;
using AutoFixture.NUnit3;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using OrderCloud.SDK;
using Headstart.Models.Headstart;

namespace Headstart.Tests
{
    public class AutoNSubstituteDataAttribute : AutoDataAttribute
    {
        public AutoNSubstituteDataAttribute()
            : base(() => new Fixture().Customize(new HSCompositeCustomization()))
        {

        }
    }

    public class HSCompositeCustomization : CompositeCustomization
    {
        public HSCompositeCustomization()
            : base(
                new OrderCloudModelsCustomization(),
                new AutoNSubstituteCustomization()

            )
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

            fixture.Customize<HSLocationUserGroup>(c => c
                .With(x => x.xp, fixture.Create<HSLocationUserGroupXp>()));

            fixture.Customize<HSBuyer>(c => c
                    .With(x => x.xp, fixture.Create<BuyerXp>()));

            fixture.Customize<HSShipMethod>(c => c
                    .With(x => x.xp, fixture.Create<ShipMethodXP>()));

            fixture.Customize<HSShipEstimate>(c => c
                    .With(x => x.xp, fixture.Create<ShipEstimateXP>())
                    .With(x => x.ShipMethods, fixture.Create<List<HSShipMethod>>()));

            fixture.Customize<HSLineItemProduct>(c => c
                    .With(x => x.xp, fixture.Create<ProductXp>()));

            fixture.Customize<HSOrderCalculateResponse>(c => c
                    .With(x => x.xp, fixture.Create<OrderCalculateResponseXp>()));

            fixture.Customize<HSShipEstimateResponse>(c => c
                    .With(x => x.xp, fixture.Create<ShipEstimateResponseXP>())
                    .With(x => x.ShipEstimates, fixture.Create<List<HSShipEstimate>>()));

            fixture.Customize<HSLineItem>(c => c
                    .With(x => x.xp, fixture.Create<LineItemXp>())
                    .With(x => x.Product, fixture.Create<HSLineItemProduct>())
                    .With(x => x.ShippingAddress, fixture.Create<HSAddressBuyer>())
                    .With(x => x.ShipFromAddress, fixture.Create<HSAddressSupplier>()));

            fixture.Customize<HSAddressBuyer>(c => c
                    .With(x => x.xp, fixture.Create<BuyerAddressXP>()));

            fixture.Customize<HSUser>(c => c
                    .With(x => x.xp, fixture.Create<UserXp>()));

            fixture.Customize<HSOrder>(c => c
                    .With(x => x.xp, fixture.Create<OrderXp>())
                    .With(x => x.FromUser, fixture.Create<HSUser>())
                    .With(x => x.BillingAddress, fixture.Create<HSAddressBuyer>()));

            fixture.Customize<HSOrderWorksheet>(c => c
                    .With(x => x.Order, fixture.Create<HSOrder>())
                    .With(x => x.LineItems, fixture.Create<List<HSLineItem>>())
                    .With(x => x.ShipEstimateResponse, fixture.Create<HSShipEstimateResponse>())
                    .With(x => x.OrderCalculateResponse, fixture.Create<HSOrderCalculateResponse>()));
        }
    }
}
