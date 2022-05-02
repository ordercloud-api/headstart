using System;
using OrderCloud.SDK;
using Headstart.Common.Helpers;
using System.Collections.Generic;
using Headstart.Common.Models.Misc;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.exchangerates;

namespace Headstart.API.Commands.EnvironmentSeed
{
	public static class SeedConstants
	{
		public const string BuyerApiClientName = "Default Buyer Storefront";
		public const string BuyerLocalApiClientName = "Default HeadStart Buyer UI LOCAL"; // Used for pointing integration events to the ngrok url
		public const string SellerApiClientName = "Default HeadStart Admin UI";
		public const string IntegrationsApiClientName = "Middleware Integrations";
		public const string SellerUserName = "Default_Admin";
		public const string FullAccessSecurityProfile = "DefaultContext";
		public const string DefaultBuyerName = "Default Headstart Buyer";
		public const string DefaultBuyerId = "0001";
		public const string DefaultLocationId = "default-buyerLocation";
		public const string AllowedSecretChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		/// <summary>
		/// Public re-usable AnonymousBuyerUser method
		/// </summary>
		/// <returns>The HsUser response object for an Anonymous Buyer User</returns>
		public static HsUser AnonymousBuyerUser()
		{
			return new HsUser()
			{
				Id = "default-buyer-user",
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

		/// <summary>
		/// Public re-usable MiddlewareIntegrationsUser method
		/// </summary>
		/// <returns>The User response object for a Middleware Integrations User</returns>
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

		/// <summary>
		/// Public re-usable MiddlewareIntegrationsUser method
		/// </summary>
		/// <returns>The HSBuyer response object for an Default Buyer</returns>
		public static HsBuyer DefaultBuyer()
		{
			return new HsBuyer
			{
				Id = DefaultBuyerId,
				Name = DefaultBuyerName,
				Active = true,
				xp = new BuyerXp
				{
					MarkupPercent = 0
				}
			};
		}

		/// <summary>
		/// Public read-only DefaultIndices object
		/// </summary>
		public static readonly List<XpIndex> DefaultIndices = new List<XpIndex>() {
			new XpIndex
			{
				ThingType = XpThingType.UserGroup, 
				Key = @"Type"
			},
			new XpIndex
			{
				ThingType = XpThingType.UserGroup, 
				Key = @"Role"
			},
			new XpIndex
			{
				ThingType = XpThingType.UserGroup, 
				Key = @"Country"
			},
			new XpIndex
			{
				ThingType = XpThingType.UserGroup, 
				Key = @"CatalogAssignments"
			},
			new XpIndex
			{
				ThingType = XpThingType.Company, 
				Key = @"Data.ServiceCategory"
			},
			new XpIndex
			{
				ThingType = XpThingType.Company, 
				Key = @"Data.VendorLevel"
			},
			new XpIndex
			{
				ThingType = XpThingType.Company, 
				Key = @"SyncFreightPop"
			},
			new XpIndex
			{
				ThingType = XpThingType.Company,
				Key = @"BuyersServicing"
			},
			new XpIndex
			{
				ThingType = XpThingType.Company,
				Key = @"CountriesServicing"
			},
			new XpIndex
			{
				ThingType = XpThingType.Order, 
				Key = @"NeedsAttention"
			},
			new XpIndex
			{
				ThingType = XpThingType.Order, 
				Key = @"StopShipSync"
			},
			new XpIndex
			{
				ThingType = XpThingType.Order, 
				Key = @"OrderType"
			},
			new XpIndex
			{
				ThingType = XpThingType.Order, 
				Key = @"LocationID"
			},
			new XpIndex
			{
				ThingType = XpThingType.Order, 
				Key = @"SubmittedOrderStatus"
			},
			new XpIndex
			{
				ThingType = XpThingType.Order, 
				Key = @"IsResubmitting"
			},
			new XpIndex
			{
				ThingType = XpThingType.Order, 
				Key = @"SupplierIDs"
			},
			new XpIndex
			{
				ThingType = XpThingType.Order, 
				Key = @"QuoteStatus"
			},
			new XpIndex
			{
				ThingType = XpThingType.Order, 
				Key = @"QuoteSupplierID"
			},
			new XpIndex
			{
				ThingType = XpThingType.User, 
				Key = @"UserGroupID"
			},
			new XpIndex
			{
				ThingType = XpThingType.User, 
				Key = @"RequestInfoEmails"
			},
			new XpIndex
			{
				ThingType = XpThingType.Promotion, Key = "Automatic"
			},
			new XpIndex
			{
				ThingType = XpThingType.Promotion, Key = "AppliesTo"
			},
		};

		/// <summary>
		/// Public read-only DefaultIncrementors object
		/// </summary>
		public static readonly List<Incrementor> DefaultIncrementors = new List<Incrementor>() {
			new Incrementor
			{
				ID = @"orderIncrementor", 
				Name = @"Order Incrementor", 
				LastNumber = 0, LeftPaddingCount = 6
			},
			new Incrementor
			{
				ID = @"supplierIncrementor", 
				Name = @"Supplier Incrementor", 
				LastNumber = 0, LeftPaddingCount = 3
			},
			new Incrementor
			{
				ID = @"buyerIncrementor", 
				Name = @"Buyer Incrementor", 
				LastNumber = 0, LeftPaddingCount = 4
			},
			new Incrementor
			{
				ID = @"sellerLocationIncrementor", 
				Name = @"Seller Location Incrementor",
				LastNumber = 0, LeftPaddingCount = 4
			}
		};

		#region API CLIENTS
		/// <summary>
		/// Public re-usable IntegrationsClient method
		/// </summary>
		/// <returns>The ApiClient response object for an Integrations Client</returns>
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

		/// <summary>
		/// Public re-usable SellerClient method
		/// </summary>
		/// <returns>The ApiClient response object for a Seller Client</returns>
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

		/// <summary>
		/// Public re-usable BuyerClient method
		/// </summary>
		/// <param name="seed"></param>
		/// <returns>The HSApiClient response object for a Buyer Client</returns>
		public static HSApiClient BuyerClient(Common.Models.Misc.EnvironmentSeed seed)
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

		/// <summary>
		/// Public re-usable BuyerLocalClient method
		/// </summary>
		/// <param name="seed"></param>
		/// <returns>The ApiClient response object for a Buyer Local Client</returns>
		public static ApiClient BuyerLocalClient(Common.Models.Misc.EnvironmentSeed seed)
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
		/// <summary>
		/// Public re-usable CheckoutEvent method
		/// </summary>
		/// <param name="middlewareBaseUrl"></param>
		/// <param name="webhookHashKey"></param>
		/// <returns>The IntegrationEvent response object for the Checkout Event process</returns>
		public static IntegrationEvent CheckoutEvent(string middlewareBaseUrl, string webhookHashKey)
		{
			return new IntegrationEvent()
			{
				ElevatedRoles = new[] { ApiRole.FullAccess },
				ID = @"HeadStartCheckout",
				EventType = IntegrationEventType.OrderCheckout,
				Name = @"HeadStart Checkout",
				CustomImplementationUrl = middlewareBaseUrl,
				HashKey = webhookHashKey,
				ConfigData = new
				{
					ExcludePOProductsFromShipping = false,
					ExcludePOProductsFromTax = true,
				}
			};
		}

		/// <summary>
		/// Public re-usable LocalCheckoutEvent method
		/// </summary>
		/// <param name="webhookHashKey"></param>
		/// <returns>The IntegrationEvent response object for the Local Checkout Event process</returns>
		public static IntegrationEvent LocalCheckoutEvent(string webhookHashKey)
		{
			return new IntegrationEvent()
			{
				ElevatedRoles = new[] { ApiRole.FullAccess },
				ID = @"HeadStartCheckoutLOCAL",
				EventType = IntegrationEventType.OrderCheckout,
				CustomImplementationUrl = "https://changethisurl.ngrok.io", // Local webhook url
				Name = @"HeadStart Checkout LOCAL",
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
		/// <summary>
		/// Public re-usable BuyerEmails method
		/// </summary>
		/// <param name="seed"></param>
		/// <returns>The MessageSender response object for the BuyerEmails process</returns>
		public static MessageSender BuyerEmails(Common.Models.Misc.EnvironmentSeed seed)
		{
			return new MessageSender()
			{
				ID = @"BuyerEmails",
				Name = @"Buyer Emails",
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
				URL = seed.MiddlewareBaseUrl + @"/messagesenders/{messagetype}",
				SharedKey = seed.OrderCloudSeedSettings.WebhookHashKey
			};
		}

		/// <summary>
		/// Public re-usable SellerEmails method
		/// </summary>
		/// <param name="seed"></param>
		/// <returns>The MessageSender response object for the SellerEmails process</returns>
		public static MessageSender SellerEmails(Common.Models.Misc.EnvironmentSeed seed)
		{
			return new MessageSender()
			{
				ID = @"SellerEmails",
				Name = @"Seller Emails",
				MessageTypes = new[] {
					MessageType.ForgottenPassword,
				},
				URL = seed.MiddlewareBaseUrl + @"/messagesenders/{messagetype}",
				SharedKey = seed.OrderCloudSeedSettings.WebhookHashKey
			};
		}

		/// <summary>
		/// Public re-usable SuplierEmails method
		/// </summary>
		/// <param name="seed"></param>
		/// <returns>The MessageSender response object for the SuplierEmails process</returns>
		public static MessageSender SupplierEmails(Common.Models.Misc.EnvironmentSeed seed)
		{
			return new MessageSender()
			{
				ID = "SupplierEmails",
				Name = "Supplier Emails",
				MessageTypes = new[] {
					MessageType.ForgottenPassword,
				},
				URL = seed.MiddlewareBaseUrl + @"/messagesenders/{messagetype}",
				SharedKey = seed.OrderCloudSeedSettings.WebhookHashKey
			};
		}
		#endregion

		#region Permissions
		/// <summary>
		/// Public read-only DefaultSecurityProfiles object
		/// </summary>
		public static readonly List<HsSecurityProfile> DefaultSecurityProfiles = new List<HsSecurityProfile>() 
		{
			// Seller/supplier
			new HsSecurityProfile() {Id = CustomRole.HSBuyerAdmin, CustomRoles = new CustomRole[] { CustomRole.HSBuyerAdmin }, Roles = new ApiRole[] { ApiRole.AddressAdmin, ApiRole.ApprovalRuleAdmin, ApiRole.BuyerAdmin, ApiRole.BuyerUserAdmin, ApiRole.CreditCardAdmin, ApiRole.UserGroupAdmin } },
			new HsSecurityProfile() {Id = CustomRole.HSBuyerImpersonator, CustomRoles = new CustomRole[] { CustomRole.HSBuyerImpersonator }, Roles = new ApiRole[] { ApiRole.BuyerImpersonation } },
			new HsSecurityProfile() {Id = CustomRole.HSCategoryAdmin, CustomRoles = new CustomRole[] { CustomRole.HSCategoryAdmin }, Roles = new ApiRole[] { ApiRole.CategoryAdmin } },
			new HsSecurityProfile() {Id = CustomRole.HSContentAdmin, CustomRoles = new CustomRole[] { CustomRole.AssetAdmin, CustomRole.DocumentAdmin, CustomRole.SchemaAdmin }, Roles = new ApiRole[] { ApiRole.ApiClientAdmin } },
			new HsSecurityProfile() {Id = CustomRole.HSMeAdmin, CustomRoles = new CustomRole[] { CustomRole.HSMeAdmin }, Roles = new ApiRole[] { ApiRole.MeAdmin, ApiRole.MeXpAdmin } },
			new HsSecurityProfile() {Id = CustomRole.HSMeProductAdmin, CustomRoles = new CustomRole[] { CustomRole.HSMeProductAdmin }, Roles = new ApiRole[] { ApiRole.InventoryAdmin, ApiRole.PriceScheduleAdmin, ApiRole.ProductAdmin, ApiRole.ProductFacetReader, ApiRole.SupplierAddressReader } },
			new HsSecurityProfile() {Id = CustomRole.HSMeSupplierAddressAdmin, CustomRoles = new CustomRole[] { CustomRole.HSMeSupplierAddressAdmin }, Roles = new ApiRole[] { ApiRole.SupplierAddressAdmin, ApiRole.SupplierReader } },
			new HsSecurityProfile() {Id = CustomRole.HSMeSupplierAdmin, CustomRoles = new CustomRole[] { CustomRole.AssetAdmin, CustomRole.HSMeSupplierAdmin }, Roles = new ApiRole[] { ApiRole.SupplierAdmin, ApiRole.SupplierReader } },
			new HsSecurityProfile() {Id = CustomRole.HSMeSupplierUserAdmin, CustomRoles = new CustomRole[] { CustomRole.HSMeSupplierUserAdmin }, Roles = new ApiRole[] { ApiRole.SupplierReader, ApiRole.SupplierUserAdmin } },
			new HsSecurityProfile() {Id = CustomRole.HSOrderAdmin, CustomRoles = new CustomRole[] { CustomRole.HSOrderAdmin }, Roles = new ApiRole[] { ApiRole.AddressReader, ApiRole.OrderAdmin, ApiRole.ShipmentReader } },
			new HsSecurityProfile() {Id = CustomRole.HSProductAdmin, CustomRoles = new CustomRole[] { CustomRole.HSProductAdmin }, Roles = new ApiRole[] { ApiRole.AdminAddressAdmin, ApiRole.AdminAddressReader, ApiRole.CatalogAdmin, ApiRole.PriceScheduleAdmin, ApiRole.ProductAdmin, ApiRole.ProductAssignmentAdmin, ApiRole.ProductFacetAdmin, ApiRole.SupplierAddressReader } },
			new HsSecurityProfile() {Id = CustomRole.HSPromotionAdmin, CustomRoles = new CustomRole[] { CustomRole.HSPromotionAdmin }, Roles = new ApiRole[] { ApiRole.PromotionAdmin } },
			new HsSecurityProfile() {Id = CustomRole.HSReportAdmin, CustomRoles = new CustomRole[] { CustomRole.HSReportAdmin }, Roles = new ApiRole[] { } },
			new HsSecurityProfile() {Id = CustomRole.HSReportReader, CustomRoles = new CustomRole[] { CustomRole.HSReportReader }, Roles = new ApiRole[] { } },
			new HsSecurityProfile() {Id = CustomRole.HSSellerAdmin, CustomRoles = new CustomRole[] { CustomRole.HSSellerAdmin }, Roles = new ApiRole[] { ApiRole.AdminUserAdmin } },
			new HsSecurityProfile() {Id = CustomRole.HSShipmentAdmin, CustomRoles = new CustomRole[] { CustomRole.HSShipmentAdmin }, Roles = new ApiRole[] { ApiRole.AddressReader, ApiRole.OrderReader, ApiRole.ShipmentAdmin } },
			new HsSecurityProfile() {Id = CustomRole.HSStorefrontAdmin, CustomRoles = new CustomRole[] { CustomRole.HSStorefrontAdmin }, Roles = new ApiRole[] { ApiRole.ProductFacetAdmin, ApiRole.ProductFacetReader } },
			new HsSecurityProfile() {Id = CustomRole.HSSupplierAdmin, CustomRoles = new CustomRole[] { CustomRole.HSSupplierAdmin }, Roles = new ApiRole[] { ApiRole.SupplierAddressAdmin, ApiRole.SupplierAdmin, ApiRole.SupplierUserAdmin } },
			new HsSecurityProfile() {Id = CustomRole.HSSupplierUserGroupAdmin, CustomRoles = new CustomRole[] { CustomRole.HSSupplierUserGroupAdmin }, Roles = new ApiRole[] { ApiRole.SupplierReader, ApiRole.SupplierUserGroupAdmin } },
			
			// Buyer - this is the only role needed for a buyer user to successfully check out
			new HsSecurityProfile() {Id = CustomRole.HSBaseBuyer, CustomRoles = new CustomRole[] { CustomRole.HSBaseBuyer }, Roles = new ApiRole[] { ApiRole.MeAddressAdmin, ApiRole.MeAdmin, ApiRole.MeCreditCardAdmin, ApiRole.MeXpAdmin, ApiRole.ProductFacetReader, ApiRole.Shopper, ApiRole.SupplierAddressReader, ApiRole.SupplierReader } },

			/* These roles don't do much, access to changing location information will be done through middleware calls that
			*  confirm the user is in the location specific access user group. These roles will be assigned to the location 
			*  specific user group and allow us to determine if a user has an admin role for at least one location through 
			*  the users JWT
			*/
			new HsSecurityProfile() {Id = CustomRole.HSLocationOrderApprover, CustomRoles = new CustomRole[] { CustomRole.HSLocationOrderApprover }, Roles = new ApiRole[] { } },
			new HsSecurityProfile() {Id = CustomRole.HSLocationViewAllOrders, CustomRoles = new CustomRole[] { CustomRole.HSLocationViewAllOrders }, Roles = new ApiRole[] { } },
			new HsSecurityProfile() {Id = CustomRole.HSLocationAddressAdmin, CustomRoles = new CustomRole[] { CustomRole.HSLocationAddressAdmin }, Roles = new ApiRole[] { } },
			new HsSecurityProfile() {Id = CustomRole.HSLocationPermissionAdmin, CustomRoles = new CustomRole[] { CustomRole.HSLocationPermissionAdmin }, Roles = new ApiRole[] { } },
			new HsSecurityProfile() {Id = CustomRole.HSLocationNeedsApproval, CustomRoles = new CustomRole[] { CustomRole.HSLocationNeedsApproval }, Roles = new ApiRole[] { } },
			new HsSecurityProfile() {Id = CustomRole.HSLocationCreditCardAdmin, CustomRoles = new CustomRole[] { CustomRole.HSLocationCreditCardAdmin }, Roles = new ApiRole[] { } },
		};

		/// <summary>
		/// Public read-only SellerHsRoles object
		/// </summary>
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
		/// <summary>
		/// Public re-usable DefaultBuyerLocation method
		/// </summary>
		/// <returns>The HsBuyerLocation response object for the Default Buyer Location</returns>
		public static HsBuyerLocation DefaultBuyerLocation()
		{
			return new HsBuyerLocation()
			{
				UserGroup = new HsLocationUserGroup()
				{
					Name = "Default Headstart Location",
					xp = new HsLocationUserGroupXp()
					{
						Type = "BuyerLocation",
						Currency = CurrencySymbol.USD,
						Country = "US"
					}
				},
				Address = new HsAddressBuyer()
				{
					AddressName = "Default Headstart Address",
					Street1 = "110 5th St",
					City = "Minneapolis",
					Zip = "55403",
					State = "Minnesota",
					Country = "US"
				}
			};
		}
		#endregion

		#region Product Facets
		/// <summary>
		/// Public re-usable DefaultProductFacet method
		/// </summary>
		/// <returns>The HsProductFacet response object for the Default Product Facet</returns>
		public static HsProductFacet DefaultProductFacet()
		{
			return new HsProductFacet()
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

		#region Azure Regions
		public static Region UsEast = new Region()
		{
			AzureRegion = "eastus",
			Id = "est",
			Name = "US-East"
		};

		public static Region AustraliaEast = new Region()
		{
			AzureRegion = "australiaeast",
			Id = "aus",
			Name = "Australia-East"
		};

		public static Region EuropeWest = new Region()
		{
			AzureRegion = "westeurope",
			Id = "eur",
			Name = "Europe-West"
		};

		public static Region JapanEast = new Region()
		{
			AzureRegion = "japaneast",
			Id = "jpn",
			Name = "Japan-East"
		};

		public static Region UsWest = new Region()
		{
			AzureRegion = "westus",
			Id = "usw",
			Name = "US-West"
		};

		public static readonly List<Region> Regions = new List<Region>()
		{
			UsEast,
			AustraliaEast,
			EuropeWest,
			JapanEast,
			UsWest
		};
		#endregion
	}
}