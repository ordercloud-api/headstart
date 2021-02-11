using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Helpers;
using Headstart.Models;
using Headstart.Models.Misc;
using Headstart.Models.Headstart;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.IO;
using Newtonsoft.Json.Linq;
using Headstart.Common.Services.CMS;
using Headstart.Common.Services.CMS.Models;
using ordercloud.integrations.library.helpers;
using Headstart.Common.Services;
using Headstart.Common;

namespace Headstart.API.Commands
{
	public interface IEnvironmentSeedCommand
	{
		Task<string> Seed(EnvironmentSeed seed);
		Task PostStagingRestore();
	}
	public class EnvironmentSeedCommand : IEnvironmentSeedCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly AppSettings _settings;
		private readonly IPortalService _portal;
		private readonly IHeadstartSupplierCommand _supplierCommand;
		private readonly IHSBuyerCommand _buyerCommand;
		private readonly ICMSClient _cms;

		private readonly string _buyerApiClientName = "Default HeadStart Buyer UI";
		private readonly string _buyerLocalApiClientName = "Default HeadStart Buyer UI LOCAL"; // used for pointing integration events to the ngrok url
		private readonly string _sellerApiClientName = "Default HeadStart Admin UI";
		private readonly string _integrationsApiClientName = "Middleware Integrations";
		private readonly string _sellerUserName = "Default_Admin";
		private readonly string _fullAccessSecurityProfile = "DefaultContext";

		public EnvironmentSeedCommand(
			AppSettings settings,
			IPortalService portal,
			IHeadstartSupplierCommand supplierCommand,
			IHSBuyerCommand buyerCommand,
			ICMSClient cms,
			IOrderCloudClient oc
		)
		{
			_settings = settings;
			_portal = portal;
			_supplierCommand = supplierCommand;
			_buyerCommand = buyerCommand;
			_cms = cms;
			_oc = oc;
		}

		public async Task<string> Seed(EnvironmentSeed seed)
		{
			if (string.IsNullOrEmpty(_settings.OrderCloudSettings.ApiUrl))
			{
				throw new Exception("Missing required app setting OrderCloudSettings:ApiUrl");
			}
			if (string.IsNullOrEmpty(_settings.OrderCloudSettings.WebhookHashKey))
			{
				throw new Exception("Missing required app setting OrderCloudSettings:WebhookHashKey");
			}
			if (string.IsNullOrEmpty(_settings.EnvironmentSettings.MiddlewareBaseUrl))
			{
				throw new Exception("Missing required app setting EnvironmentSettings:MiddlewareBaseUrl");
			}

			var portalUserToken = await _portal.Login(seed.PortalUsername, seed.PortalPassword);
			await VerifyOrgExists(seed.SellerOrgID, portalUserToken);
			var orgToken = await _portal.GetOrgToken(seed.SellerOrgID, portalUserToken);

			await CreateDefaultSellerUser(orgToken);
			await CreateApiClients(orgToken);
			await CreateSecurityProfiles(seed, orgToken);
			await AssignSecurityProfiles(seed, orgToken);

			var apiClients = await GetApiClients(orgToken);
			await CreateWebhooks(new string[] { apiClients.BuyerUiApiClient.ID }, apiClients.AdminUiApiClient.ID, orgToken);
			await CreateMessageSenders(orgToken);
			await CreateIncrementors(orgToken); // must be before CreateBuyers

			var userContext = await GetVerifiedUserContext(apiClients.MiddlewareApiClient);
			await CreateBuyers(seed, userContext);
			await CreateXPIndices(orgToken);
			await CreateAndAssignIntegrationEvents(new string[] { apiClients.BuyerUiApiClient.ID }, apiClients.BuyerLocalUiApiClient.ID, orgToken);
			await CreateSuppliers(userContext, seed, orgToken);
			await CreateContentDocSchemas(userContext);
			await CreateDefaultContentDocs(userContext);

			return orgToken;

		}

		public async Task VerifyOrgExists(string orgID, string devToken)
		{
			try
			{
				await _portal.GetOrganization(orgID, devToken);
			}
			catch
			{
				// the portal API no longer allows us to create an organization outside of portal
				// so we need to create the org first before seeding it
				throw new Exception("Failed to retrieve seller organization with SellerOrgID. The organization must exist before it can be seeded");
			}
		}

		public async Task PostStagingRestore()
		{
			var token = (await _oc.AuthenticateAsync()).AccessToken;
			var apiClients = await GetApiClients(token);
			var storefrontClientIDs = await GetStoreFrontClientIDs(token);

			var deleteMS = DeleteAllMessageSenders(token);
			var deleteWH = DeleteAllWebhooks(token);
			var deleteIE = DeleteAllIntegrationEvents(token);
			await Task.WhenAll(deleteMS, deleteWH, deleteIE);

			// recreate with environment specific data
			var createMS = CreateMessageSenders(token);
			var createWH = CreateWebhooks(storefrontClientIDs, apiClients.AdminUiApiClient.ID, token);
			var createIE = CreateAndAssignIntegrationEvents(storefrontClientIDs, apiClients.BuyerLocalUiApiClient.ID, token);
			var shutOffSupplierEmails = ShutOffSupplierEmailsAsync(token); // shut off email notifications for all suppliers

			await Task.WhenAll(createMS, createWH, createIE, shutOffSupplierEmails);
		}


		private async Task AssignSecurityProfiles(EnvironmentSeed seed, string orgToken)
		{
			// assign buyer security profiles
			var buyerSecurityProfileAssignmentRequests = seed.Buyers.Select(b =>
			{
				return _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment
				{
					BuyerID = b.ID,
					SecurityProfileID = CustomRole.MPBaseBuyer.ToString()
				}, orgToken);
			});
			await Task.WhenAll(buyerSecurityProfileAssignmentRequests);

			// assign seller security profiles to seller org
			var sellerSecurityProfileAssignmentRequests = SellerHsRoles.Select(role =>
			{
				return _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
				{
					SecurityProfileID = role.ToString()
				}, orgToken);
			});
			await Task.WhenAll(sellerSecurityProfileAssignmentRequests);

			// assign full access security profile to default admin user
			var defaultAdminUser = (await _oc.AdminUsers.ListAsync(accessToken: orgToken)).Items.First(u => u.Username == _sellerUserName);
			await _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
			{
				SecurityProfileID = _fullAccessSecurityProfile,
				UserID = defaultAdminUser.ID
			}, orgToken);
		}

		private async Task CreateBuyers(EnvironmentSeed seed, VerifiedUserContext user)
		{
			seed.Buyers.Add(new HSBuyer
			{
				ID = "Default_HeadStart_Buyer",
				Name = "Default HeadStart Buyer",
				Active = true,
				xp = new BuyerXp
				{
					MarkupPercent = 0
				}
			});
			foreach (var buyer in seed.Buyers)
			{
				var superBuyer = new SuperHSBuyer()
				{
					Buyer = buyer,
					Markup = new BuyerMarkup() { Percent = 0 }
				};
				await _buyerCommand.Create(superBuyer, user, isSeedingEnvironment: true);
			}
		}

		private async Task CreateSuppliers(VerifiedUserContext user, EnvironmentSeed seed, string token)
		{
			// Create Suppliers and necessary user groups and security profile assignments
			foreach (HSSupplier supplier in seed.Suppliers)
			{
				await _supplierCommand.Create(supplier, user, isSeedingEnvironment: true);
			}
		}

		private async Task<VerifiedUserContext> GetVerifiedUserContext(ApiClient middlewareApiClient)
		{
			// some endpoints such as documents and documentschemas require a verified user context for a user in the seller org
			// however the context that we get when calling this endpoint is for the dev user so we need to create a user context
			// with the seller user
			var ocConfig = new OrderCloudClientConfig
			{
				ApiUrl = _settings.OrderCloudSettings.ApiUrl,
				AuthUrl = _settings.OrderCloudSettings.ApiUrl,
				ClientId = middlewareApiClient.ID,
				ClientSecret = middlewareApiClient.ClientSecret,
				GrantType = GrantType.ClientCredentials,
				Roles = new[]
				{
					ApiRole.FullAccess
				}
			};
			return await new VerifiedUserContext(_oc).Define(ocConfig);
		}

		private async Task CreateContentDocSchemas(VerifiedUserContext userContext)
		{
			var kitSchema = new DocSchema
			{
				ID = "HSKitProductAssignment",
				RestrictedAssignmentTypes = new List<ResourceType> { },
				Schema = JObject.Parse(File.ReadAllText("../Headstart.Common/Assets/ContentDocSchemas/kitproduct.json"))
			};

			var supplierFilterConfigSchema = new DocSchema
			{
				ID = "SupplierFilterConfig",
				RestrictedAssignmentTypes = new List<ResourceType> { },
				Schema = JObject.Parse(File.ReadAllText("../Headstart.Common/Assets/ContentDocSchemas/supplierfilterconfig.json"))
			};

			await Task.WhenAll(
				_cms.Schemas.Create(kitSchema, userContext.AccessToken),
				_cms.Schemas.Create(supplierFilterConfigSchema, userContext.AccessToken)
			);
		}

		private async Task CreateDefaultContentDocs(VerifiedUserContext userContext)
		{
			// any default created docs should be generic enough to be used by all orgs
			await Task.WhenAll(
				_cms.Documents.Create("SupplierFilterConfig", GetCountriesServicingDoc(), userContext.AccessToken),
				_cms.Documents.Create("SupplierFilterConfig", GetServiceCategoryDoc(), userContext.AccessToken),
				_cms.Documents.Create("SupplierFilterConfig", GetVendorLevelDoc(), userContext.AccessToken)
			);
		}

		private Document<SupplierFilterConfig> GetCountriesServicingDoc()
		{
			return new Document<SupplierFilterConfig>
			{
				ID = "CountriesServicing",
				Doc = new SupplierFilterConfig
				{
					Display = "Countries Servicing",
					Path = "xp.CountriesServicing",
					Items = new List<Filter>
					{
						new Filter
						{
							Text = "UnitedStates",
							Value = "US"
						}
					},
					AllowSellerEdit = true,
					AllowSupplierEdit = true,
					BuyerAppFilterType = "NonUI"
				}
			};
		}

		private dynamic GetServiceCategoryDoc()
		{
			return new Document<SupplierFilterConfig>
			{
				ID = "ServiceCategory",
				Doc = new SupplierFilterConfig
				{
					Display = "Service Category",
					Path = "xp.Categories.ServiceCategory",
					AllowSupplierEdit = false,
					AllowSellerEdit = true,
					BuyerAppFilterType = "SelectOption",
					Items = new List<Filter>
					{

					}
				}
			};
		}

		private Document<SupplierFilterConfig> GetVendorLevelDoc()
		{
			return new Document<SupplierFilterConfig>
			{
				ID = "VendorLevel",
				Doc = new SupplierFilterConfig
				{
					Display = "Vendor Level",
					Path = "xp.Categories.VendorLevel",
					AllowSupplierEdit = true,
					AllowSellerEdit = true,
					BuyerAppFilterType = "SelectOption",
					Items = new List<Filter>
					{

					}
				}
			};
		}

		private async Task CreateDefaultSellerUser(string token)
		{
			var defaultSellerUser = new User
			{
				ID = "Default_Admin",
				Username = _sellerUserName,
				Email = "test@test.com",
				Active = true,
				FirstName = "Default",
				LastName = "User"
			};
			await _oc.AdminUsers.CreateAsync(defaultSellerUser, token);
		}

		static readonly List<XpIndex> DefaultIndices = new List<XpIndex>() {
			new XpIndex { ThingType = XpThingType.UserGroup, Key = "Type" },
			new XpIndex { ThingType = XpThingType.UserGroup, Key = "Role" },
			new XpIndex { ThingType = XpThingType.UserGroup, Key = "Country" },
			new XpIndex { ThingType = XpThingType.Company, Key = "Data.ServiceCategory" },
			new XpIndex { ThingType = XpThingType.Company, Key = "Data.VendorLevel" },
			new XpIndex { ThingType = XpThingType.Company, Key = "SyncFreightPop" },
			new XpIndex { ThingType = XpThingType.Company, Key = "CountriesServicing" },
			new XpIndex { ThingType = XpThingType.Order, Key = "NeedsAttention" },
			new XpIndex { ThingType = XpThingType.Order, Key = "StopShipSync" },
			new XpIndex { ThingType = XpThingType.Order, Key = "OrderType" },
			new XpIndex { ThingType = XpThingType.Order, Key = "LocationID" },
			new XpIndex { ThingType = XpThingType.Order, Key = "SubmittedOrderStatus" },
			new XpIndex { ThingType = XpThingType.Order, Key = "IsResubmitting" },
			new XpIndex { ThingType = XpThingType.Order, Key = "SupplierIDs" },
			new XpIndex { ThingType = XpThingType.User, Key = "UserGroupID" },
			new XpIndex { ThingType = XpThingType.User, Key = "RequestInfoEmails" },
		};

		public async Task CreateXPIndices(string token)
		{
			foreach (var index in DefaultIndices)
			{
				await _oc.XpIndices.PutAsync(index, token);
			}
		}

		static readonly List<Incrementor> DefaultIncrementors = new List<Incrementor>() {
			new Incrementor { ID = "orderIncrementor", Name = "Order Incrementor", LastNumber = 1, LeftPaddingCount = 6 },
			new Incrementor { ID = "supplierIncrementor", Name = "Supplier Incrementor", LastNumber = 1, LeftPaddingCount = 3 },
			new Incrementor { ID = "buyerIncrementor", Name = "Buyer Incrementor", LastNumber = 1, LeftPaddingCount = 4 }
		};

		public async Task CreateIncrementors(string token)
		{
			foreach (var incrementor in DefaultIncrementors)
			{
				await _oc.Incrementors.CreateAsync(incrementor, token);
			}
		}

		private async Task<ApiClientIDs> GetApiClients(string token)
		{
			var list = await ListAllAsync.List(page => _oc.ApiClients.ListAsync(page: page, pageSize: 100, accessToken: token));
			var appNames = list.Select(x => x.AppName);
			var adminUIApiClient = list.First(a => a.AppName == _sellerApiClientName);
			var buyerUIApiClient = list.First(a => a.AppName == _buyerApiClientName);
			var buyerLocalUIApiClient = list.First(a => a.AppName == _buyerLocalApiClientName);
			var middlewareApiClient = list.First(a => a.AppName == _integrationsApiClientName);
			return new ApiClientIDs()
			{
				AdminUiApiClient = adminUIApiClient,
				BuyerUiApiClient = buyerUIApiClient,
				BuyerLocalUiApiClient = buyerLocalUIApiClient,
				MiddlewareApiClient = middlewareApiClient
			};
		}

		private async Task<string[]> GetStoreFrontClientIDs(string token)
		{
			var list = await ListAllAsync.List(page => _oc.ApiClients.ListAsync(page: page, pageSize: 100, filters: new { AppName = "Storefront - *" }, accessToken: token));
			return list.Select(client => client.ID).ToArray();
		}

		public class ApiClientIDs
		{
			public ApiClient AdminUiApiClient { get; set; }
			public ApiClient BuyerUiApiClient { get; set; }
			public ApiClient BuyerLocalUiApiClient { get; set; }
			public ApiClient MiddlewareApiClient { get; set; }
		}

		private async Task CreateApiClients(string token)
		{
			var allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var integrationsClient = new ApiClient()
			{
				AppName = _integrationsApiClientName,
				Active = true,
				AllowAnyBuyer = false,
				AllowAnySupplier = false,
				AllowSeller = true,
				AccessTokenDuration = 600,
				RefreshTokenDuration = 43200,
				DefaultContextUserName = _sellerUserName,
				ClientSecret = RandomGen.GetString(allowedChars, 60)
			};
			var sellerClient = new ApiClient()
			{
				AppName = _sellerApiClientName,
				Active = true,
				AllowAnyBuyer = false,
				AllowAnySupplier = true,
				AllowSeller = true,
				AccessTokenDuration = 600,
				RefreshTokenDuration = 43200
			};
			var buyerClient = new ApiClient()
			{
				AppName = _buyerApiClientName,
				Active = true,
				AllowAnyBuyer = true,
				AllowAnySupplier = false,
				AllowSeller = false,
				AccessTokenDuration = 600,
				RefreshTokenDuration = 43200
			};
			var buyerLocalClient = new ApiClient()
			{
				AppName = _buyerLocalApiClientName,
				Active = true,
				AllowAnyBuyer = true,
				AllowAnySupplier = false,
				AllowSeller = false,
				AccessTokenDuration = 600,
				RefreshTokenDuration = 43200
			};

			var integrationsClientRequest = _oc.ApiClients.CreateAsync(integrationsClient, token);
			var sellerClientRequest = _oc.ApiClients.CreateAsync(sellerClient, token);
			var buyerClientRequest = _oc.ApiClients.CreateAsync(buyerClient, token);
			var buyerLocalClientRequest = _oc.ApiClients.CreateAsync(buyerLocalClient, token);

			await Task.WhenAll(integrationsClientRequest, sellerClientRequest, buyerClientRequest, buyerLocalClientRequest);
		}

		private async Task CreateMessageSenders(string accessToken)
		{
			foreach (var messageSender in DefaultMessageSenders())
			{
				messageSender.URL = $"{_settings.EnvironmentSettings.MiddlewareBaseUrl}{messageSender.URL}";
				await _oc.MessageSenders.CreateAsync(messageSender, accessToken);
			}
		}
		private async Task CreateAndAssignIntegrationEvents(string[] buyerClientIDs, string localBuyerClientID, string token)
		{
			await _oc.IntegrationEvents.CreateAsync(new IntegrationEvent()
			{
				ElevatedRoles = new[] { ApiRole.FullAccess },
				ID = "HeadStartCheckout",
				EventType = IntegrationEventType.OrderCheckout,
				Name = "HeadStart Checkout",
				CustomImplementationUrl = _settings.EnvironmentSettings.MiddlewareBaseUrl,
				HashKey = _settings.OrderCloudSettings.WebhookHashKey,
				ConfigData = new
				{
					ExcludePOProductsFromShipping = false,
					ExcludePOProductsFromTax = true,
				}
			}, token);
			await _oc.IntegrationEvents.CreateAsync(new IntegrationEvent()
			{
				ElevatedRoles = new[] { ApiRole.FullAccess },
				ID = "HeadStartCheckoutLOCAL",
				EventType = IntegrationEventType.OrderCheckout,
				CustomImplementationUrl = "https://marketplaceteam.ngrok.io", // local webhook url
				Name = "HeadStart Checkout LOCAL",
				HashKey = _settings.OrderCloudSettings.WebhookHashKey,
				ConfigData = new
				{
					ExcludePOProductsFromShipping = false,
					ExcludePOProductsFromTax = true,
				}
			}, token);

			await _oc.ApiClients.PatchAsync(localBuyerClientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckoutLOCAL" }, token);
			await Throttler.RunAsync(buyerClientIDs, 500, 20, clientID =>
				_oc.ApiClients.PatchAsync(clientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckout" }, token));
		}

		private async Task ShutOffSupplierEmailsAsync(string token)
		{
			var allSuppliers = await ListAllAsync.List(page => _oc.Suppliers.ListAsync<HSSupplier>(page: page, pageSize: 100));
			await Throttler.RunAsync(allSuppliers, 500, 20, supplier =>
				_oc.Suppliers.PatchAsync(supplier.ID, new PartialSupplier { xp = new { NotificationRcpts = new string[] { } } }, token));
		}

		public async Task CreateSecurityProfiles(EnvironmentSeed seed, string accessToken)
		{
			var profiles = DefaultSecurityProfiles.Select(p =>
				new SecurityProfile()
				{
					Name = p.CustomRole.ToString(),
					ID = p.CustomRole.ToString(),
					CustomRoles = p.CustomRoles == null ? new List<string>() { p.CustomRole.ToString() } : p.CustomRoles.Append(p.CustomRole).Select(r => r.ToString()).ToList(),
					Roles = p.Roles
				}).ToList();

			profiles.Add(new SecurityProfile()
			{
				Roles = new List<ApiRole> { ApiRole.FullAccess },
				Name = _fullAccessSecurityProfile,
				ID = _fullAccessSecurityProfile
			});

			var profileCreateRequests = profiles.Select(p => _oc.SecurityProfiles.CreateAsync(p, accessToken));
			await Task.WhenAll(profileCreateRequests);
		}

		public async Task DeleteAllWebhooks(string token)
		{
			var webhooks = await ListAllAsync.List(page => _oc.Webhooks.ListAsync(page: page, pageSize: 100, accessToken: token));
			await Throttler.RunAsync(webhooks, 500, 20, webhook =>
				_oc.Webhooks.DeleteAsync(webhook.ID, accessToken: token));
		}

		public async Task DeleteAllMessageSenders(string token)
		{
			var messageSenders = await ListAllAsync.List(page => _oc.MessageSenders.ListAsync(page: page, pageSize: 100, accessToken: token));
			await Throttler.RunAsync(messageSenders, 500, 20, messageSender =>
				_oc.MessageSenders.DeleteAsync(messageSender.ID, accessToken: token));
		}

		public async Task DeleteAllIntegrationEvents(string token)
		{
			// can't delete integration event if its referenced by an api client so first patch it to null
			var apiClientsWithIntegrationEvent = await ListAllAsync.List(page => _oc.ApiClients.ListAsync(page: page, pageSize: 100, filters: new { OrderCheckoutIntegrationEventID = "*" }, accessToken: token));
			await Throttler.RunAsync(apiClientsWithIntegrationEvent, 500, 20, apiClient =>
				_oc.ApiClients.PatchAsync(apiClient.ID, new PartialApiClient { OrderCheckoutIntegrationEventID = null }, accessToken: token));

			var integrationEvents = await ListAllAsync.List(page => _oc.IntegrationEvents.ListAsync(page: page, pageSize: 100, accessToken: token));
			await Throttler.RunAsync(integrationEvents, 500, 20, integrationEvent =>
				_oc.IntegrationEvents.DeleteAsync(integrationEvent.ID, accessToken: token));
		}

		private async Task CreateWebhooks(string[] buyerClientIDs, string adminUiApiClientID, string token)
		{
			var DefaultWebhooks = new List<Webhook>() {
			new Webhook() {
			  Name = "Order Shipped",
			  Description = "Triggers email letting user know the order was shipped.",
			  Url = "/ordershipped",
			  ElevatedRoles =
				new List<ApiRole>
				{
					ApiRole.FullAccess
				},
			  BeforeProcessRequest = false,
			  WebhookRoutes = new List<WebhookRoute>
			  {
				new WebhookRoute() { Route = "v1/orders/{direction}/{orderID}/ship", Verb = "POST" }
			  },
			  ApiClientIDs = new [] { adminUiApiClientID }
			},
			new Webhook() {
			  Name = "Order Cancelled",
			  Description = "Triggers email letting user know the order has been cancelled.",
			  Url = "/ordercancelled",
			  ElevatedRoles =
				new List<ApiRole>
				{
					ApiRole.FullAccess
				},
			  BeforeProcessRequest = false,
			  WebhookRoutes = new List<WebhookRoute>
			  {
				new WebhookRoute() { Route = "v1/orders/{direction}/{orderID}/cancel", Verb = "POST" }
			  },
			  ApiClientIDs = new [] { adminUiApiClientID }
			},
			new Webhook() {
			  Name = "New User",
			  Description = "Triggers an email welcoming the buyer user.  Triggers an email letting admin know about the new buyer user.",
			  Url = "/newuser",
			  ElevatedRoles =
				new List<ApiRole>
				{
					ApiRole.FullAccess
				},
			  BeforeProcessRequest = false,
			  WebhookRoutes = new List<WebhookRoute>
			  {
				new WebhookRoute() { Route = "v1/buyers/{buyerID}/users", Verb = "POST" }
			  },
			  ApiClientIDs = new [] { adminUiApiClientID }.Concat(buyerClientIDs).ToArray()
			},
			new Webhook() {
			  Name = "Product Created",
			  Description = "Triggers email to user with details of newly created product.",
			  Url = "/productcreated",
			  ElevatedRoles =
				new List<ApiRole>
				{
					ApiRole.FullAccess
				},
			  BeforeProcessRequest = false,
			  WebhookRoutes = new List<WebhookRoute>
			  {
				new WebhookRoute() { Route = "v1/products", Verb = "POST" }
			  },
			  ApiClientIDs = new [] { adminUiApiClientID }
			},
			new Webhook() {
			  Name = "Product Update",
			  Description = "Triggers email to user indicating that a product has been updated.",
			  Url = "/productupdate",
			  ElevatedRoles =
				new List<ApiRole>
				{
					ApiRole.FullAccess
				},
			  BeforeProcessRequest = false,
			  WebhookRoutes = new List<WebhookRoute>
			  {
				new WebhookRoute() { Route = "v1/products/{productID}", Verb = "PATCH" }
			  },
			  ApiClientIDs = new [] { adminUiApiClientID }
			},
			new Webhook() {
			  Name = "Supplier Updated",
			  Description = "Triggers email letting user know the supplier has been updated.",
			  Url = "/supplierupdated",
			  ElevatedRoles =
				new List<ApiRole>
				{
					ApiRole.FullAccess
				},
			  BeforeProcessRequest = false,
			  WebhookRoutes = new List<WebhookRoute>
			  {
				new WebhookRoute() { Route = "v1/suppliers/{supplierID}", Verb = "PATCH" }
			  },
			 ApiClientIDs = new [] { adminUiApiClientID }
			},
		};
			foreach (Webhook webhook in DefaultWebhooks)
			{
				webhook.Url = $"{_settings.EnvironmentSettings.MiddlewareBaseUrl}{webhook.Url}";
				webhook.HashKey = _settings.OrderCloudSettings.WebhookHashKey;
				await _oc.Webhooks.CreateAsync(webhook, accessToken: token);
			}
		}

		private List<MessageSender> DefaultMessageSenders()
		{
			return new List<MessageSender>() {
				new MessageSender()
				{
					Name = "Password Reset",
					MessageTypes = new[] { MessageType.ForgottenPassword },
					URL = "/passwordreset",
					SharedKey = _settings.OrderCloudSettings.WebhookHashKey,
					xp = new {
							MessageTypeConfig = new {
								MessageType = "ForgottenPassword",
								FromEmail = "noreply@ordercloud.io",
								Subject = "Here is the link to reset your password",
								TemplateName = "ForgottenPassword",
								MainContent = "ForgottenPassword"
							}
					}
				},
				new MessageSender()
				{
					Name = "New User Registration",
					MessageTypes = new[] { MessageType.NewUserInvitation },
					URL = "/newuser",
					SharedKey = _settings.OrderCloudSettings.WebhookHashKey,
					xp = new {
							MessageTypeConfig = new {
								MessageType = "NewUserInvitation",
								FromEmail = "noreply@ordercloud.io",
								Subject = "New User Registration",
								TemplateName = "ForgottenPassword",
								MainContent = "NewUserInvitation"
							}
					}
				}
			};
		}

		static readonly List<HSSecurityProfile> DefaultSecurityProfiles = new List<HSSecurityProfile>() {
			
			// seller/supplier
			new HSSecurityProfile() { CustomRole = CustomRole.MPMeProductAdmin, Roles = new[] { ApiRole.ProductAdmin, ApiRole.PriceScheduleAdmin, ApiRole.InventoryAdmin, ApiRole.ProductFacetReader } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPMeProductReader, Roles = new[] { ApiRole.ProductReader, ApiRole.PriceScheduleReader, ApiRole.ProductFacetReader } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPProductAdmin, Roles = new[] { ApiRole.ProductAdmin, ApiRole.CatalogAdmin, ApiRole.ProductAssignmentAdmin, ApiRole.ProductFacetAdmin, ApiRole.AdminAddressReader, ApiRole.PriceScheduleAdmin  } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPProductReader, Roles = new[] { ApiRole.ProductReader, ApiRole.CatalogReader, ApiRole.ProductFacetReader} },
			new HSSecurityProfile() { CustomRole = CustomRole.MPPromotionAdmin, Roles = new[] { ApiRole.PromotionAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPContentAdmin, CustomRoles = new[] { CustomRole.AssetAdmin, CustomRole.SchemaAdmin, CustomRole.DocumentAdmin, } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPPromotionReader, Roles = new[] { ApiRole.PromotionReader } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPStoreFrontAdmin, Roles = new[] { ApiRole.ProductFacetAdmin, ApiRole.ProductFacetReader } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPCategoryAdmin, Roles = new[] { ApiRole.CategoryAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPCategoryReader, Roles = new[] { ApiRole.CategoryReader } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPOrderAdmin, Roles = new[] { ApiRole.OrderAdmin, ApiRole.ShipmentReader } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPOrderReader, Roles = new[] { ApiRole.OrderReader, ApiRole.ShipmentAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPShipmentAdmin, Roles = new[] { ApiRole.OrderReader, ApiRole.ShipmentAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPBuyerAdmin, Roles = new[] { ApiRole.BuyerAdmin, ApiRole.BuyerUserAdmin, ApiRole.UserGroupAdmin, ApiRole.AddressAdmin, ApiRole.CreditCardAdmin, ApiRole.ApprovalRuleAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPBuyerReader, Roles = new[] { ApiRole.BuyerReader, ApiRole.BuyerUserReader, ApiRole.UserGroupReader, ApiRole.AddressReader, ApiRole.CreditCardReader, ApiRole.ApprovalRuleReader } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPSellerAdmin, Roles = new[] { ApiRole.AdminUserAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPSupplierAdmin, Roles = new[] { ApiRole.SupplierAdmin, ApiRole.SupplierUserAdmin, ApiRole.SupplierAddressAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPMeSupplierAdmin, Roles = new[] {ApiRole.SupplierReader, ApiRole.SupplierAdmin }, CustomRoles = new[] { CustomRole.AssetAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPMeSupplierAddressAdmin, Roles = new[] { ApiRole.SupplierReader, ApiRole.SupplierAddressAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPMeSupplierUserAdmin, Roles = new[] { ApiRole.SupplierReader, ApiRole.SupplierUserAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPSupplierUserGroupAdmin, Roles = new[] { ApiRole.SupplierReader, ApiRole.SupplierUserGroupAdmin } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPReportReader, CustomRoles = new[] { CustomRole.MPReportReader } },
			new HSSecurityProfile() { CustomRole = CustomRole.MPReportAdmin, CustomRoles = new[] { CustomRole.MPReportAdmin } },
			
			// buyer
			new HSSecurityProfile() { CustomRole = CustomRole.MPBaseBuyer, Roles = new[] { ApiRole.MeAdmin, ApiRole.MeCreditCardAdmin, ApiRole.MeAddressAdmin, ApiRole.MeXpAdmin, ApiRole.ProductFacetReader, ApiRole.Shopper, ApiRole.SupplierAddressReader, ApiRole.SupplierReader } },
			
			// buyer impersonation - indicating the most roles a buyer user could have for impersonation purposes
			new HSSecurityProfile() { CustomRoles = new[] {CustomRole.MPBaseBuyer, CustomRole.MPLocationOrderApprover, CustomRole.MPLocationViewAllOrders }, Roles = new[] { ApiRole.MeAdmin, ApiRole.MeCreditCardAdmin, ApiRole.MeAddressAdmin, ApiRole.MeXpAdmin, ApiRole.ProductFacetReader, ApiRole.Shopper, ApiRole.SupplierAddressReader, ApiRole.SupplierReader } },

			/* these roles don't do much, access to changing location information will be done through middleware calls that
			*  confirm the user is in the location specific access user group. These roles will be assigned to the location 
			*  specific user group and allow us to determine if a user has an admin role for at least one location through 
			*  the users JWT
			*/
			new HSSecurityProfile() { CustomRole = CustomRole.MPLocationPermissionAdmin },
			new HSSecurityProfile() { CustomRole = CustomRole.MPLocationOrderApprover },
			new HSSecurityProfile() { CustomRole = CustomRole.MPLocationNeedsApproval },
			new HSSecurityProfile() { CustomRole = CustomRole.MPLocationViewAllOrders },
			new HSSecurityProfile() { CustomRole = CustomRole.MPLocationCreditCardAdmin },
			new HSSecurityProfile() { CustomRole = CustomRole.MPLocationAddressAdmin },
			new HSSecurityProfile() { CustomRole = CustomRole.MPLocationResaleCertAdmin }
		};

		static readonly List<CustomRole> SellerHsRoles = new List<CustomRole>() {
			CustomRole.MPProductAdmin,
			CustomRole.MPPromotionAdmin,
			CustomRole.MPStoreFrontAdmin,
			CustomRole.MPCategoryAdmin,
			CustomRole.MPOrderAdmin,
			CustomRole.MPShipmentAdmin,
			CustomRole.MPBuyerAdmin,
			CustomRole.MPSellerAdmin,
			CustomRole.MPSupplierAdmin,
			CustomRole.MPSupplierUserGroupAdmin,
			CustomRole.MPReportReader,
			CustomRole.MPReportAdmin,
			CustomRole.MPContentAdmin
		};
	}
}
