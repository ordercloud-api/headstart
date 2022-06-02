using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Headstart.Common.Services;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Common.Settings;
using Headstart.Models;
using Headstart.Models.Headstart;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using OrderCloud.Integrations.Library.Models;
using OrderCloud.Integrations.Taxation.Interfaces;
using OrderCloud.SDK;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Headstart.Tests
{
    public class SendgridTests
    {
        private IOrderCloudClient oc;
        private SendgridSettings sendgridSettings;
        private UI uiSettings;
        private ISendGridClient sendGridClient;
        private ISendgridService command;

        [SetUp]
        public void Setup()
        {
            oc = Substitute.For<IOrderCloudClient>();
            sendgridSettings = Substitute.For<SendgridSettings>();
            uiSettings = Substitute.For<UI>();
            sendGridClient = Substitute.For<ISendGridClient>();

            command = new SendgridService(sendgridSettings, uiSettings, oc, sendGridClient);
        }

        [Test]
        public async Task SendOrderSubmitEmail_WithValidWorksheet_SendsEmailNotifications()
        {
            // Arrange
            var orderWorksheet = GetOrderWorksheet();
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, $"{TestConstants.OrderID}-{TestConstants.Supplier1ID}").Returns(GetSupplierWorksheet(TestConstants.Supplier1ID, TestConstants.LineItem1ID, TestConstants.LineItem1Total));
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, $"{TestConstants.OrderID}-{TestConstants.Supplier2ID}").Returns(GetSupplierWorksheet(TestConstants.Supplier2ID, TestConstants.LineItem2ID, TestConstants.LineItem2Total));
            oc.Suppliers.ListAsync<HSSupplier>(Arg.Any<string>()).ReturnsForAnyArgs(Task.FromResult(GetSupplierList()));
            oc.AdminUsers.ListAsync<HSSellerUser>().ReturnsForAnyArgs(Task.FromResult(GetSellerUserList()));
            var commandSub = Substitute.ForPartsOf<SendgridService>(sendgridSettings, oc, sendGridClient);
            commandSub.Configure().WhenForAnyArgs(x => x.SendSingleTemplateEmailMultipleRcpts(default, default, default, default)).DoNotCallBase();
            commandSub.Configure().WhenForAnyArgs(x => x.SendSingleTemplateEmail(default, default, default, default)).DoNotCallBase();

            var expectedSellerEmailList = new List<EmailAddress>()
            {
                new EmailAddress() { Email = TestConstants.SellerUser1email },
                new EmailAddress() { Email = TestConstants.SellerUser1AdditionalRcpts[0] },
            };
            var expectedSupplier1EmailList = new List<EmailAddress>()
            {
                new EmailAddress() { Email = TestConstants.Supplier1NotificationRcpts[0] },
                new EmailAddress() { Email = TestConstants.Supplier1NotificationRcpts[1] },
            };
            var expectedSupplier2EmailList = new List<EmailAddress>()
            {
                new EmailAddress() { Email = TestConstants.Supplier2NotificationRcpts[0] },
            };

            // Act
            await commandSub.SendOrderSubmitEmail(orderWorksheet);

            // Assert
            // Confirm emails sent to buyer, seller users, supplier 1 notification recipients, supplier 2 notification recipients
            await commandSub.Configure().Received().SendSingleTemplateEmail(Arg.Any<string>(), TestConstants.BuyerEmail, Arg.Any<string>(), Arg.Any<object>());
            await commandSub.Configure().Received().SendSingleTemplateEmailMultipleRcpts(Arg.Any<string>(), Arg.Is<List<EmailAddress>>(x => EqualEmailLists(x, expectedSellerEmailList)), Arg.Any<string>(), Arg.Any<object>());
            await commandSub.Configure().Received().SendSingleTemplateEmailMultipleRcpts(Arg.Any<string>(), Arg.Is<List<EmailAddress>>(x => EqualEmailLists(x, expectedSupplier1EmailList)), Arg.Any<string>(), Arg.Any<object>());
            await commandSub.Configure().Received().SendSingleTemplateEmailMultipleRcpts(Arg.Any<string>(), Arg.Is<List<EmailAddress>>(x => EqualEmailLists(x, expectedSupplier2EmailList)), Arg.Any<string>(), Arg.Any<object>());
        }

        private bool EqualEmailLists(List<EmailAddress> list1, List<EmailAddress> list2)
        {
            if (list1.Count() != list2.Count())
            {
                return false;
            }
            else
            {
                var isEqual = true;
                var list2Emails = list2.Select(item => item.Email);
                var list1Emails = list1.Select(item => item.Email);
                foreach (var item in list1)
                {
                    if (!list2Emails.Contains(item.Email))
                    {
                        isEqual = false;
                    }
                }

                foreach (var item in list2)
                {
                    if (!list1Emails.Contains(item.Email))
                    {
                        isEqual = false;
                    }
                }

                return isEqual;
            }
        }

        private HSOrderWorksheet GetOrderWorksheet()
        {
            Fixture fixture = new Fixture();

            dynamic shipEstimatexp1 = new ShipEstimateXP();
            dynamic shipEstimatexp2 = new ShipEstimateXP();
            shipEstimatexp1.SupplierID = TestConstants.Supplier1ID;
            shipEstimatexp2.SupplierID = TestConstants.Supplier2ID;

            return new HSOrderWorksheet()
            {
                Order = new HSOrder()
                {
                    ID = TestConstants.OrderID,
                    FromUser = new HSUser()
                    {
                        FirstName = "john",
                        LastName = "johnson",
                        Email = TestConstants.BuyerEmail,
                    },
                    BillingAddressID = "testbillingaddressid",
                    BillingAddress = fixture.Create<HSAddressBuyer>(),
                    xp = new OrderXp()
                    {
                        OrderType = OrderType.Standard,
                        SupplierIDs = new List<string>()
                        {
                            TestConstants.Supplier1ID,
                            TestConstants.Supplier2ID,
                        },
                        Currency = CurrencyCode.USD,
                    },
                    DateSubmitted = new DateTimeOffset(),
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem()
                    {
                        ID = TestConstants.LineItem1ID,
                        ProductID = TestConstants.Product1ID,
                        Quantity = 1,
                        LineTotal = TestConstants.LineItem1Total,
                        Product = new HSLineItemProduct()
                        {
                            Name = TestConstants.Product1Name,
                        },
                        ShippingAddress = fixture.Create<HSAddressBuyer>(),
                        xp = fixture.Create<LineItemXp>(),
                    },
                    new HSLineItem()
                    {
                        ID = TestConstants.LineItem2ID,
                        ProductID = TestConstants.Product2ID,
                        Quantity = 1,
                        LineTotal = TestConstants.LineItem2Total,
                        Product = new HSLineItemProduct()
                        {
                            Name = TestConstants.Product2Name,
                        },
                        ShippingAddress = fixture.Create<HSAddressBuyer>(),
                        xp = fixture.Create<LineItemXp>(),
                    },
                },
                ShipEstimateResponse = new HSShipEstimateResponse()
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate()
                        {
                            SelectedShipMethodID = TestConstants.SelectedShipEstimate1ID,
                            xp = shipEstimatexp1,
                            ShipMethods = new List<HSShipMethod>()
                            {
                                new HSShipMethod()
                                {
                                    ID = TestConstants.SelectedShipEstimate1ID,
                                    Cost = TestConstants.SelectedShipEstimate1Cost,
                                },
                                fixture.Create<HSShipMethod>(),
                            },
                        },
                        new HSShipEstimate()
                        {
                            SelectedShipMethodID = TestConstants.SelectedShipEstimate2ID,
                            xp = shipEstimatexp2,
                            ShipMethods = new List<HSShipMethod>()
                            {
                                new HSShipMethod()
                                {
                                    ID = TestConstants.SelectedShipEstimate2ID,
                                    Cost = TestConstants.SelectedShipEstimate2Cost,
                                },
                                fixture.Create<HSShipMethod>(),
                            },
                        },
                    },
                },
                OrderCalculateResponse = new HSOrderCalculateResponse()
                {
                    xp = new OrderCalculateResponseXp()
                    {
                        TaxCalculation = new OrderTaxCalculation()
                        {
                            OrderLevelTaxes = new List<TaxDetails>
                            {
                                new TaxDetails()
                                {
                                    Tax = TestConstants.LineItem1ShipmentTax,
                                    ShipEstimateID = TestConstants.SelectedShipEstimate1ID,
                                },
                                new TaxDetails()
                                {
                                    Tax = TestConstants.LineItem2ShipmentTax,
                                    ShipEstimateID = TestConstants.SelectedShipEstimate2ID,
                                },
                            },
                            LineItems = new List<LineItemTaxCalculation>()
                            {
                                new LineItemTaxCalculation()
                                {
                                    LineItemID = TestConstants.LineItem1ID,
                                    LineItemTotalTax = TestConstants.LineItem1Tax,
                                },
                                new LineItemTaxCalculation()
                                {
                                    LineItemID = TestConstants.LineItem2ID,
                                    LineItemTotalTax = TestConstants.LineItem2Tax,
                                },
                            },
                        },
                    },
                },
            };
        }

        private HSOrderWorksheet GetSupplierWorksheet(string supplierID, string lineItemID, decimal total)
        {
            Fixture fixture = new Fixture();
            return new HSOrderWorksheet()
            {
                Order = new HSOrder()
                {
                    ID = $"{TestConstants.OrderID}-{supplierID}",
                    Total = total,
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem()
                    {
                        ID = lineItemID,
                        Quantity = 1,
                        LineTotal = total,
                        ProductID = lineItemID == TestConstants.LineItem1ID ? TestConstants.Product1ID : TestConstants.Product2ID,
                        Product = new HSLineItemProduct()
                        {
                            Name = lineItemID == TestConstants.LineItem1ID ? TestConstants.Product1Name : TestConstants.Product2Name,
                        },
                        xp = fixture.Create<LineItemXp>(),
                        ShippingAddress = fixture.Create<HSAddressBuyer>(),
                    },
                },
            };
        }

        private ListPage<HSSupplier> GetSupplierList()
        {
            return new ListPage<HSSupplier>()
            {
                Items = new List<HSSupplier>()
                {
                    new HSSupplier()
                    {
                        ID = TestConstants.Supplier1ID,
                        xp = new SupplierXp()
                        {
                            NotificationRcpts = TestConstants.Supplier1NotificationRcpts.ToList(),
                        },
                    },
                    new HSSupplier()
                    {
                        ID = TestConstants.Supplier2ID,
                        xp = new SupplierXp()
                        {
                            NotificationRcpts = TestConstants.Supplier2NotificationRcpts.ToList(),
                        },
                    },
                },
            };
        }

        private ListPage<HSSellerUser> GetSellerUserList()
        {
            return new ListPage<HSSellerUser>()
            {
                Items = new List<HSSellerUser>()
                {
                    new HSSellerUser()
                    {
                        ID = "selleruser1",
                        Email = TestConstants.SellerUser1email,
                        xp = new SellerUserXp()
                        {
                            OrderEmails = true,
                            AddtlRcpts = TestConstants.SellerUser1AdditionalRcpts.ToList(),
                        },
                    },
                    new HSSellerUser()
                    {
                        ID = "selleruser1",
                        Email = TestConstants.Selleruser2email,
                        xp = new SellerUserXp()
                        {
                            OrderEmails = false,
                        },
                    },
                },
            };
        }

        public class TestConstants
        {
            public const string OrderID = "testorder";
            public const string BuyerEmail = "buyer@test.com";
            public const string LineItem1ID = "testlineitem1";
            public const string LineItem2ID = "testlineitem2";
            public const decimal LineItem1Total = 15;
            public const decimal LineItem2Total = 10;
            public const string Product1ID = "testproduct1";
            public const string Product1Name = "shirt";
            public const string Product2ID = "testproduct2";
            public const string Product2Name = "pants";
            public const string Supplier1ID = "001";
            public const string Supplier2ID = "002";
            public const string SelectedShipEstimate1ID = "shipEstimate001";
            public const string SelectedShipEstimate2ID = "shipEstimate002";
            public const decimal SelectedShipEstimate1Cost = 10;
            public const decimal SelectedShipEstimate2Cost = 15;
            public const string SellerUser1email = "selleruser1@test.com";
            public const string Selleruser2email = "selleruser2@test.com";
            public const decimal LineItem1Tax = 5;
            public const decimal LineItem2Tax = 7;
            public const decimal LineItem1ShipmentTax = 2;
            public const decimal LineItem2ShipmentTax = 2;
            public static readonly string[] Supplier1NotificationRcpts = { "001user@test.com", "001user2@test.com" };
            public static readonly string[] Supplier2NotificationRcpts = { "002user@test.com" };
            public static readonly string[] SellerUser1AdditionalRcpts = { "additionalrecipient1@test.com" };
        }
    }
}
