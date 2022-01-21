using System;
using System.Collections.Generic;
using Headstart.Common.Helpers;
using Headstart.Models;
using Headstart.Models.Misc;
using OrderCloud.SDK;
using Headstart.Common;
using ordercloud.integrations.exchangerates;
using Headstart.Common.Models;

namespace Headstart.API.Commands
{
    public class SeedConstants
    {
        public static string BuyerApiClientName = "Default Buyer Storefront";
        public static string BuyerLocalApiClientName = "Default HeadStart Buyer UI LOCAL"; // used for pointing integration events to the ngrok url
        public static string SellerApiClientName = "Default HeadStart Admin UI";
        public static string IntegrationsApiClientName = "Middleware Integrations";
        public static string SellerUserName = "Default_Admin";
        public static string FullAccessSecurityProfile = "DefaultContext";
        public static string DefaultBuyerName = "Default Headstart Buyer";
        public static string DefaultBuyerID = "0001";
        public static string DefaultLocationID = "default-buyerLocation";
        public static string AllowedSecretChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static HSUser AnonymousBuyerUser()
        {
            return new HSUser()
            {
                ID = "default-buyer-user",
                Username = "default-buyer-user",
                FirstName = "Default",
                LastName = "Buyer",
                Email = "default-buyer-user@test.com",
                Active = true,
                DateCreated = DateTime.Now,
                xp = new UserXp()
                {
                    Country = "US"
                }
            };
        }

        public static User MiddlewareIntegrationsUser()
        {
            return new User()
            {
                ID = "MiddlewareIntegrationsUser",
                Username = SellerUserName,
                Email = "test@test.com",
                Active = true,
                FirstName = "Default",
                LastName = "User"
            };
        }

        public static HSBuyer DefaultBuyer()
        {
            return new HSBuyer
            {
                ID = DefaultBuyerID,
                Name = DefaultBuyerName,
                Active = true,
                xp = new BuyerXp
                {
                    MarkupPercent = 0
                }
            };
        }

        public static readonly List<XpIndex> DefaultIndices = new List<XpIndex>() {
            new XpIndex { ThingType = XpThingType.UserGroup, Key = "Type" },
            new XpIndex { ThingType = XpThingType.UserGroup, Key = "Role" },
            new XpIndex { ThingType = XpThingType.UserGroup, Key = "Country" },
            new XpIndex { ThingType = XpThingType.UserGroup, Key = "CatalogAssignments" },
            new XpIndex { ThingType = XpThingType.Company, Key = "Data.ServiceCategory" },
            new XpIndex { ThingType = XpThingType.Company, Key = "Data.VendorLevel" },
            new XpIndex { ThingType = XpThingType.Company, Key = "SyncFreightPop" },
            new XpIndex { ThingType = XpThingType.Company, Key = "BuyersServicing" },
            new XpIndex { ThingType = XpThingType.Company, Key = "CountriesServicing" },
            new XpIndex { ThingType = XpThingType.Order, Key = "NeedsAttention" },
            new XpIndex { ThingType = XpThingType.Order, Key = "StopShipSync" },
            new XpIndex { ThingType = XpThingType.Order, Key = "OrderType" },
            new XpIndex { ThingType = XpThingType.Order, Key = "LocationID" },
            new XpIndex { ThingType = XpThingType.Order, Key = "SubmittedOrderStatus" },
            new XpIndex { ThingType = XpThingType.Order, Key = "IsResubmitting" },
            new XpIndex { ThingType = XpThingType.Order, Key = "SupplierIDs" },
            new XpIndex { ThingType = XpThingType.Order, Key = "QuoteStatus" },
            new XpIndex { ThingType = XpThingType.Order, Key = "QuoteSupplierID" },
            new XpIndex { ThingType = XpThingType.User, Key = "UserGroupID" },
            new XpIndex { ThingType = XpThingType.User, Key = "RequestInfoEmails" },
            new XpIndex { ThingType = XpThingType.Promotion, Key = "Automatic" },
            new XpIndex { ThingType = XpThingType.Promotion, Key = "AppliesTo" },
        };

        public static readonly List<Incrementor> DefaultIncrementors = new List<Incrementor>() {
            new Incrementor { ID = "orderIncrementor", Name = "Order Incrementor", LastNumber = 0, LeftPaddingCount = 6 },
            new Incrementor { ID = "supplierIncrementor", Name = "Supplier Incrementor", LastNumber = 0, LeftPaddingCount = 3 },
            new Incrementor { ID = "buyerIncrementor", Name = "Buyer Incrementor", LastNumber = 0, LeftPaddingCount = 4 },
            new Incrementor { ID = "sellerLocationIncrementor", Name = "Seller Location Incrementor", LastNumber = 0, LeftPaddingCount = 4 }
        };

        #region API CLIENTS
        public static ApiClient IntegrationsClient()
        {
            return new ApiClient()
            {
                AppName = IntegrationsApiClientName,
                Active = true,
                AllowAnyBuyer = false,
                AllowAnySupplier = false,
                AllowSeller = true,
                AccessTokenDuration = 600,
                RefreshTokenDuration = 43200,
                DefaultContextUserName = SellerUserName,
                ClientSecret = RandomGen.GetString(AllowedSecretChars, 60)
            };
        }
        public static ApiClient SellerClient()
        {
            return new ApiClient()
            {
                AppName = SellerApiClientName,
                Active = true,
                AllowAnyBuyer = false,
                AllowAnySupplier = true,
                AllowSeller = true,
                AccessTokenDuration = 600,
                RefreshTokenDuration = 43200
            };
        }

        public static HSApiClient BuyerClient(EnvironmentSeed seed)
        {
            return new HSApiClient()
            {
                AppName = BuyerApiClientName,
                Active = true,
                AllowAnyBuyer = true,
                AllowAnySupplier = false,
                AllowSeller = false,
                AccessTokenDuration = 600,
                RefreshTokenDuration = 43200,
                DefaultContextUserName = seed.EnableAnonymousShopping ? AnonymousBuyerUser().ID : null,
                IsAnonBuyer = seed.EnableAnonymousShopping,
                xp = new ApiClientXP
                {
                    IsStorefront = true
                }
            };
        }

        public static ApiClient BuyerLocalClient(EnvironmentSeed seed)
        {
            return new ApiClient()
            {
                AppName = BuyerLocalApiClientName,
                Active = true,
                AllowAnyBuyer = true,
                AllowAnySupplier = false,
                AllowSeller = false,
                AccessTokenDuration = 600,
                RefreshTokenDuration = 43200,
                DefaultContextUserName = seed.EnableAnonymousShopping ? AnonymousBuyerUser().ID : null,
                IsAnonBuyer = seed.EnableAnonymousShopping
            };
        }
        #endregion

        #region IntegrationEvents
        public static IntegrationEvent CheckoutEvent(string middlewareBaseUrl, string webhookHashKey)
        {
            return new IntegrationEvent()
            {
                ElevatedRoles = new[] { ApiRole.FullAccess },
                ID = "HeadStartCheckout",
                EventType = IntegrationEventType.OrderCheckout,
                Name = "HeadStart Checkout",
                CustomImplementationUrl = middlewareBaseUrl,
                HashKey = webhookHashKey,
                ConfigData = new
                {
                    ExcludePOProductsFromShipping = false,
                    ExcludePOProductsFromTax = true,
                }
            };
        }

        public static IntegrationEvent LocalCheckoutEvent(string webhookHashKey)
        {
            return new IntegrationEvent()
            {
                ElevatedRoles = new[] { ApiRole.FullAccess },
                ID = "HeadStartCheckoutLOCAL",
                EventType = IntegrationEventType.OrderCheckout,
                CustomImplementationUrl = "https://changethisurl.ngrok.io", // local webhook url
                Name = "HeadStart Checkout LOCAL",
                HashKey = webhookHashKey,
                ConfigData = new
                {
                    ExcludePOProductsFromShipping = false,
                    ExcludePOProductsFromTax = true,
                }
            };
        }
        #endregion

        #region MessageSenders
        public static MessageSender BuyerEmails(EnvironmentSeed seed)
        {
            return new MessageSender()
            {
                ID = "BuyerEmails",
                Name = "Buyer Emails",
                MessageTypes = new[] {
                        MessageType.ForgottenPassword,
                        MessageType.NewUserInvitation,
                        MessageType.OrderApproved,
                        MessageType.OrderDeclined,
                        // MessageType.OrderSubmitted, this is currently being handled in PostOrderSubmitCommand, possibly move to message senders
                        MessageType.OrderSubmittedForApproval,
                        // MessageType.OrderSubmittedForYourApprovalHasBeenApproved, // too noisy
                        // MessageType.OrderSubmittedForYourApprovalHasBeenDeclined, // too noisy
                        // MessageType.ShipmentCreated this is currently being triggered in-app possibly move to message senders
                    },
                URL = seed.MiddlewareBaseUrl + "/messagesenders/{messagetype}",
                SharedKey = seed.OrderCloudSettings.WebhookHashKey
            };
        }

        public static MessageSender SellerEmails(EnvironmentSeed seed)
        {
            return new MessageSender()
            {
                ID = "SellerEmails",
                Name = "Seller Emails",
                MessageTypes = new[] {
                        MessageType.ForgottenPassword,
                    },
                URL = seed.MiddlewareBaseUrl + "/messagesenders/{messagetype}",
                SharedKey = seed.OrderCloudSettings.WebhookHashKey
            };
        }

        public static MessageSender SuplierEmails(EnvironmentSeed seed)
        {
            return new MessageSender()
            {
                ID = "SupplierEmails",
                Name = "Supplier Emails",
                MessageTypes = new[] {
                        MessageType.ForgottenPassword,
                    },
                URL = seed.MiddlewareBaseUrl + "/messagesenders/{messagetype}",
                SharedKey = seed.OrderCloudSettings.WebhookHashKey
            };
        }
        #endregion

        #region Permissions
        public static readonly List<HSSecurityProfile> DefaultSecurityProfiles = new List<HSSecurityProfile>() {
			
			// seller/supplier
			new HSSecurityProfile() { ID = CustomRole.HSBuyerAdmin, CustomRoles = new CustomRole[] { CustomRole.HSBuyerAdmin }, Roles = new ApiRole[] { ApiRole.AddressAdmin, ApiRole.ApprovalRuleAdmin, ApiRole.BuyerAdmin, ApiRole.BuyerUserAdmin, ApiRole.CreditCardAdmin, ApiRole.UserGroupAdmin } },
            new HSSecurityProfile() { ID = CustomRole.HSBuyerImpersonator, CustomRoles = new CustomRole[] { CustomRole.HSBuyerImpersonator }, Roles = new ApiRole[] { ApiRole.BuyerImpersonation } },
            new HSSecurityProfile() { ID = CustomRole.HSCategoryAdmin, CustomRoles = new CustomRole[] { CustomRole.HSCategoryAdmin }, Roles = new ApiRole[] { ApiRole.CategoryAdmin } },
            new HSSecurityProfile() { ID = CustomRole.HSContentAdmin, CustomRoles = new CustomRole[] { CustomRole.AssetAdmin, CustomRole.DocumentAdmin, CustomRole.SchemaAdmin }, Roles = new ApiRole[] { ApiRole.ApiClientAdmin } },
            new HSSecurityProfile() { ID = CustomRole.HSMeAdmin, CustomRoles = new CustomRole[] { CustomRole.HSMeAdmin }, Roles = new ApiRole[] { ApiRole.MeAdmin, ApiRole.MeXpAdmin } },
            new HSSecurityProfile() { ID = CustomRole.HSMeProductAdmin, CustomRoles = new CustomRole[] { CustomRole.HSMeProductAdmin }, Roles = new ApiRole[] { ApiRole.InventoryAdmin, ApiRole.PriceScheduleAdmin, ApiRole.ProductAdmin, ApiRole.ProductFacetReader, ApiRole.SupplierAddressReader } },
            new HSSecurityProfile() { ID = CustomRole.HSMeSupplierAddressAdmin, CustomRoles = new CustomRole[] { CustomRole.HSMeSupplierAddressAdmin }, Roles = new ApiRole[] { ApiRole.SupplierAddressAdmin, ApiRole.SupplierReader } },
            new HSSecurityProfile() { ID = CustomRole.HSMeSupplierAdmin, CustomRoles = new CustomRole[] { CustomRole.AssetAdmin, CustomRole.HSMeSupplierAdmin }, Roles = new ApiRole[] { ApiRole.SupplierAdmin, ApiRole.SupplierReader } },
            new HSSecurityProfile() { ID = CustomRole.HSMeSupplierUserAdmin, CustomRoles = new CustomRole[] { CustomRole.HSMeSupplierUserAdmin }, Roles = new ApiRole[] { ApiRole.SupplierReader, ApiRole.SupplierUserAdmin } },
            new HSSecurityProfile() { ID = CustomRole.HSOrderAdmin, CustomRoles = new CustomRole[] { CustomRole.HSOrderAdmin }, Roles = new ApiRole[] { ApiRole.AddressReader, ApiRole.OrderAdmin, ApiRole.ShipmentReader } },
            new HSSecurityProfile() { ID = CustomRole.HSProductAdmin, CustomRoles = new CustomRole[] { CustomRole.HSProductAdmin }, Roles = new ApiRole[] { ApiRole.AdminAddressAdmin, ApiRole.AdminAddressReader, ApiRole.CatalogAdmin, ApiRole.PriceScheduleAdmin, ApiRole.ProductAdmin, ApiRole.ProductAssignmentAdmin, ApiRole.ProductFacetAdmin, ApiRole.SupplierAddressReader } },
            new HSSecurityProfile() { ID = CustomRole.HSPromotionAdmin, CustomRoles = new CustomRole[] { CustomRole.HSPromotionAdmin }, Roles = new ApiRole[] { ApiRole.PromotionAdmin } },
            new HSSecurityProfile() { ID = CustomRole.HSReportAdmin, CustomRoles = new CustomRole[] { CustomRole.HSReportAdmin }, Roles = new ApiRole[] { } },
            new HSSecurityProfile() { ID = CustomRole.HSReportReader, CustomRoles = new CustomRole[] { CustomRole.HSReportReader }, Roles = new ApiRole[] { } },
            new HSSecurityProfile() { ID = CustomRole.HSSellerAdmin, CustomRoles = new CustomRole[] { CustomRole.HSSellerAdmin }, Roles = new ApiRole[] { ApiRole.AdminUserAdmin } },
            new HSSecurityProfile() { ID = CustomRole.HSShipmentAdmin, CustomRoles = new CustomRole[] { CustomRole.HSShipmentAdmin }, Roles = new ApiRole[] { ApiRole.AddressReader, ApiRole.OrderReader, ApiRole.ShipmentAdmin } },
            new HSSecurityProfile() { ID = CustomRole.HSStorefrontAdmin, CustomRoles = new CustomRole[] { CustomRole.HSStorefrontAdmin }, Roles = new ApiRole[] { ApiRole.ProductFacetAdmin, ApiRole.ProductFacetReader } },
            new HSSecurityProfile() { ID = CustomRole.HSSupplierAdmin, CustomRoles = new CustomRole[] { CustomRole.HSSupplierAdmin }, Roles = new ApiRole[] { ApiRole.SupplierAddressAdmin, ApiRole.SupplierAdmin, ApiRole.SupplierUserAdmin } },
            new HSSecurityProfile() { ID = CustomRole.HSSupplierUserGroupAdmin, CustomRoles = new CustomRole[] { CustomRole.HSSupplierUserGroupAdmin }, Roles = new ApiRole[] { ApiRole.SupplierReader, ApiRole.SupplierUserGroupAdmin } },
			
			// buyer - this is the only role needed for a buyer user to successfully check out
			new HSSecurityProfile() { ID = CustomRole.HSBaseBuyer, CustomRoles = new CustomRole[] { CustomRole.HSBaseBuyer }, Roles = new ApiRole[] { ApiRole.MeAddressAdmin, ApiRole.MeAdmin, ApiRole.MeCreditCardAdmin, ApiRole.MeXpAdmin, ApiRole.ProductFacetReader, ApiRole.Shopper, ApiRole.SupplierAddressReader, ApiRole.SupplierReader } },

			/* these roles don't do much, access to changing location information will be done through middleware calls that
			*  confirm the user is in the location specific access user group. These roles will be assigned to the location 
			*  specific user group and allow us to determine if a user has an admin role for at least one location through 
			*  the users JWT
			*/
			new HSSecurityProfile() { ID = CustomRole.HSLocationOrderApprover, CustomRoles = new CustomRole[] { CustomRole.HSLocationOrderApprover }, Roles = new ApiRole[] { } },
            new HSSecurityProfile() { ID = CustomRole.HSLocationViewAllOrders, CustomRoles = new CustomRole[] { CustomRole.HSLocationViewAllOrders }, Roles = new ApiRole[] { } },
            new HSSecurityProfile() { ID = CustomRole.HSLocationAddressAdmin, CustomRoles = new CustomRole[] { CustomRole.HSLocationAddressAdmin }, Roles = new ApiRole[] { } },
            new HSSecurityProfile() { ID = CustomRole.HSLocationPermissionAdmin, CustomRoles = new CustomRole[] { CustomRole.HSLocationPermissionAdmin }, Roles = new ApiRole[] { } },
            new HSSecurityProfile() { ID = CustomRole.HSLocationNeedsApproval, CustomRoles = new CustomRole[] { CustomRole.HSLocationNeedsApproval }, Roles = new ApiRole[] { } },
            new HSSecurityProfile() { ID = CustomRole.HSLocationCreditCardAdmin, CustomRoles = new CustomRole[] { CustomRole.HSLocationCreditCardAdmin }, Roles = new ApiRole[] { } },

        };

        public static readonly List<CustomRole> SellerHsRoles = new List<CustomRole>() {
            CustomRole.HSBuyerAdmin,
            CustomRole.HSBuyerImpersonator,
            CustomRole.HSCategoryAdmin,
            CustomRole.HSContentAdmin,
            CustomRole.HSMeAdmin,
            CustomRole.HSOrderAdmin,
            CustomRole.HSProductAdmin,
            CustomRole.HSPromotionAdmin,
            CustomRole.HSReportAdmin,
            CustomRole.HSReportReader,
            CustomRole.HSSellerAdmin,
            CustomRole.HSShipmentAdmin,
            CustomRole.HSStorefrontAdmin,
            CustomRole.HSSupplierAdmin,
            CustomRole.HSSupplierUserGroupAdmin
        };
        #endregion

        #region Buyer Location 
        public static HSBuyerLocation DefaultBuyerLocation()
        {
            return new HSBuyerLocation()
            {
                UserGroup = new HSLocationUserGroup()
                {
                    Name = "Default Headstart Location",
                    xp = new HSLocationUserGroupXp()
                    {
                        Type = "BuyerLocation",
                        Currency = CurrencySymbol.USD,
                        Country = "US"
                    }
                },
                Address = new HSAddressBuyer()
                {
                    AddressName = "Default Headstart Address",
                    Street1 = "110 5th St",
                    City = "Minneaplis",
                    Zip = "55403",
                    State = "Minnesota",
                    Country = "US"
                }
            };
        }

        #endregion

        #region Product Facets

        public static HSProductFacet DefaultProductFacet()
        {
            return new HSProductFacet()
            {
                ID = "supplier",
                Name = "Supplier",
                XpPath = "Facets.supplier",
                ListOrder = 1,
                MinCount = 1,
                xp = null
            };
        }

        #endregion

    }
}
