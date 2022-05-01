using System;
using System.IO;
using System.Net;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Common.Models.Misc;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Services.Portal;
using Microsoft.WindowsAzure.Storage.Blob;
using Headstart.Common.Services.Portal.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands.EnvironmentSeed
{
	public interface IEnvironmentSeedCommand
	{
		Task<EnvironmentSeedResponse> Seed(Common.Models.Misc.EnvironmentSeed seed);
		Task UpdateTranslations(string connectionString, string containerName);
		Task PostStagingRestore();
	}

	public class EnvironmentSeedCommand : IEnvironmentSeedCommand
	{
		private IOrderCloudClient _oc;
		private readonly AppSettings _settings;
		private readonly IPortalService _portal;
		private readonly IHsSupplierCommand _supplierCommand;
		private readonly IHsBuyerCommand _buyerCommand;
		private readonly IHsBuyerLocationCommand _buyerLocationCommand;

		/// <summary>
		/// The IOC based constructor method for the EnvironmentSeedCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="portal"></param>
		/// <param name="supplierCommand"></param>
		/// <param name="buyerCommand"></param>
		/// <param name="buyerLocationCommand"></param>
		/// <param name="oc"></param>
		public EnvironmentSeedCommand(AppSettings settings, IPortalService portal, IHsSupplierCommand supplierCommand, IHsBuyerCommand buyerCommand, IHsBuyerLocationCommand buyerLocationCommand, IOrderCloudClient oc)
		{
			try
			{
				_portal = portal;
				_supplierCommand = supplierCommand;
				_buyerCommand = buyerCommand;
				_buyerLocationCommand = buyerLocationCommand;
				_oc = oc;
				_settings = settings;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// This seeding function can be used to initially seed an marketplace
		/// it is also meant to be safe to call after an marketplace has been seeded (by including seed.MarketplaceID)
		/// If a method starts with CreateOrUpdate it will update the resource every time its called based on what has been defined in SeedConstants.cs
		/// If a method starts with CreateOnlyOnce it will only create the resource once and then ignore thereafter
		/// The CreateOnlyOnce resources are likely to change after initial creation so we ignore to avoid overwriting desired changes that happen outside of seeding
		/// </summary>
		/// <param name="seed"></param>
		/// <returns>The EnvironmentSeedResponse response object from the Seed process</returns>
		public async Task<EnvironmentSeedResponse> Seed(Common.Models.Misc.EnvironmentSeed seed)
		{
			var resp = new EnvironmentSeedResponse();
			try 
			{
				var requestedEnv = ValidateEnvironment(seed.OrderCloudSeedSettings.Environment);
				if (requestedEnv.EnvironmentName == OrderCloudEnvironments.Production.EnvironmentName && seed.MarketplaceId == null)
				{
					var exception = $@"Cannot create a production environment via the environment seed endpoint. Please contact an OrderCloud Developer to create a production marketplace.";
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", exception, "", this, true);
					throw new Exception(exception);
				}
				// lets us handle requests to multiple api environments
				_oc = new OrderCloudClient(new OrderCloudClientConfig
				{
					ApiUrl = requestedEnv.ApiUrl,
					AuthUrl = requestedEnv.ApiUrl
				});

				var portalUserToken = await _portal.Login(seed.PortalUsername, seed.PortalPassword);
				var marketplace = await GetOrCreateMarketplace(portalUserToken, seed, requestedEnv);
				var marketplaceToken = await _portal.GetMarketplaceToken(marketplace.Id, portalUserToken);
				await CreateOrUpdateDefaultSellerUser(seed, marketplaceToken);
				await CreateOnlyOnceIncrementors(marketplaceToken); // must be before CreateBuyers
				await CreateOrUpdateMessageSendersAndAssignments(seed, marketplaceToken); // must be before CreateBuyers and CreateSuppliers
				await CreateOrUpdateSecurityProfiles(marketplaceToken);
				await CreateOnlyOnceBuyers(seed, marketplaceToken);
				await CreateOnlyOnceApiClients(seed, marketplaceToken);
				await CreateOrUpdateSecurityProfileAssignments(seed, marketplaceToken);
				await CreateOrUpdateXPIndices(marketplaceToken);
				await CreateOrUpdateAndAssignIntegrationEvents(marketplaceToken, seed);
				await CreateOrUpdateSuppliers(seed, marketplaceToken);
				await CreateOrUpdateProductFacets(marketplaceToken);

				if (seed?.StorageAccountSettings?.ConnectionString != null && seed?.StorageAccountSettings?.ContainerNameTranslations != null)
				{
					await UpdateTranslations(seed.StorageAccountSettings.ConnectionString, seed.StorageAccountSettings.ContainerNameTranslations);
				}
				var apiClients = await GetApiClients(marketplaceToken);
				resp = new EnvironmentSeedResponse
				{
					Comments = "Success! Your environment is now seeded. The following clientIDs & secrets should be used to finalize the configuration of your application. The initial admin username and password can be used to sign into your admin application",
					MarketplaceName = marketplace.Name,
					MarketplaceId = marketplace.Id,
					OrderCloudEnvironment = requestedEnv.EnvironmentName,
					ApiClients = new Dictionary<string, dynamic>
					{
						["Middleware"] = new
						{
							ClientID = apiClients.MiddlewareApiClient.ID,
							ClientSecret = apiClients.MiddlewareApiClient.ClientSecret
						},
						["Seller"] = new
						{
							ClientID = apiClients.AdminUiApiClient.ID
						},
						["Buyer"] = new
						{
							ClientID = apiClients.BuyerUiApiClient.ID
						}
					}
				};
			}
			catch(Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ConstructMarketplaceFromSeed task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="requestedEnv"></param>
		/// <returns>The Marketplace response object</returns>
		private static Marketplace ConstructMarketplaceFromSeed(Common.Models.Misc.EnvironmentSeed seed, OcEnv requestedEnv)
		{
			var region = !string.IsNullOrWhiteSpace(seed.Region)
				? SeedConstants.Regions.Find(r => r.Name.Equals(seed.Region, StringComparison.OrdinalIgnoreCase))
				: SeedConstants.UsWest;
			return new Marketplace()
			{
				Id = Guid.NewGuid().ToString(),
				Environment = requestedEnv.EnvironmentName,
				Name = string.IsNullOrEmpty(seed.MarketplaceName) 
					? @"My Headstart Marketplace" 
					: seed.MarketplaceName,
				Region = region
			};
		}

		/// <summary>
		/// Public re-usable UpdateTranslations task method
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="containerName"></param>
		/// <returns></returns>
		public async Task UpdateTranslations(string connectionString, string containerName)
		{
			try
			{
				var englishTranslationsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Assets", "english-translations.json"));
				var translationsConfig = new BlobServiceConfig()
				{
					ConnectionString = connectionString,
					Container = containerName,
					AccessType = BlobContainerPublicAccessType.Container
				};
				var translationsBlob = new OrderCloudIntegrationsBlobService(translationsConfig);
				await translationsBlob.Save("i18n/en.json", File.ReadAllText(englishTranslationsPath));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable ValidateEnvironment method
		/// </summary>
		/// <param name="environment"></param>
		/// <returns>The OcEnv response object</returns>
		private OcEnv ValidateEnvironment(string environment)
		{
			OcEnv resp = null;
			try
			{
				if (environment.Trim().Equals(@"production", StringComparison.OrdinalIgnoreCase))
				{
					return OrderCloudEnvironments.Production;
				}
				else if (environment.Trim().Equals(@"sandbox", StringComparison.OrdinalIgnoreCase))
				{
					return OrderCloudEnvironments.Sandbox;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable VerifyMarketplaceExists task method
		/// </summary>
		/// <param name="marketplaceId"></param>
		/// <param name="devToken"></param>
		/// <returns>The Marketplace response object</returns>
		public async Task<Marketplace> VerifyMarketplaceExists(string marketplaceId, string devToken)
		{
			var marketplace = new Marketplace();
			try
			{
				marketplace = await _portal.GetMarketplace(marketplaceId, devToken);
			}
			catch (Exception ex)
			{
				// The portal API no longer allows us to create a production marketplace outside of portal
				// though its possible to create on sandbox - for consistency sake we'll require its created before seeding
				var exception = $@"Failed to retrieve marketplace with MarketplaceID. The marketplace must exist before it can be seeded. {ex.Message}.";
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", exception, ex.StackTrace, this, true);
				throw new Exception(exception);
			}
			return marketplace;
		}

		/// <summary>
		/// Public re-usable GetOrCreateMarketplace task method
		/// </summary>
		/// <param name="token"></param>
		/// <param name="seed"></param>
		/// <param name="env"></param>
		/// <returns>The Marketplace response object</returns>
		public async Task<Marketplace> GetOrCreateMarketplace(string token, Common.Models.Misc.EnvironmentSeed seed, OcEnv env)
		{
			if (!string.IsNullOrEmpty(seed.MarketplaceId))
			{
				var existingMarketplace = await VerifyMarketplaceExists(seed.MarketplaceId, token);
				return existingMarketplace;
			}
			else
			{
				var marketPlaceToCreate = ConstructMarketplaceFromSeed(seed, env);
				try
				{
					await _portal.GetMarketplace(marketPlaceToCreate.Id, token);
					return await GetOrCreateMarketplace(token, seed, env);
				}
				catch (Exception ex)
				{
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					await _portal.CreateMarketplace(marketPlaceToCreate, token);
					return await _portal.GetMarketplace(marketPlaceToCreate.Id, token);
				}
			}
		}

		/// <summary>
		/// The staging environment gets restored weekly from production
		/// during that restore things like webhooks, message senders, and integration events are shut off (so you don't for example email production customers)
		/// this process restores integration events which are required for checkout (with environment specific settings)
		/// </summary>
		/// <returns></returns>
		public async Task PostStagingRestore()
		{
			try
			{
				var token = (await _oc.AuthenticateAsync()).AccessToken;
				var deleteIE = DeleteAllIntegrationEvents(token);
				await Task.WhenAll(deleteIE);

				// recreate with environment specific data
				var createIE = CreateOrUpdateAndAssignIntegrationEvents(token);
				var shutOffSupplierEmails = ShutOffSupplierEmailsAsync(token); // shut off email notifications for all suppliers
				await Task.WhenAll(createIE, shutOffSupplierEmails);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateSecurityProfileAssignments task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="marketplaceToken"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateSecurityProfileAssignments(Common.Models.Misc.EnvironmentSeed seed, string marketplaceToken)
		{
			try
			{
				// assign buyer security profiles
				var buyerSecurityProfileAssignmentRequests = seed.Buyers.Select(b =>
				{
					return _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment
					{
						BuyerID = b.ID,
						SecurityProfileID = CustomRole.HsBaseBuyer.ToString()
					}, marketplaceToken);
				});
				await Task.WhenAll(buyerSecurityProfileAssignmentRequests);

				// assign seller security profiles to seller marketplace
				var sellerSecurityProfileAssignmentRequests = SeedConstants.SellerHsRoles.Select(role =>
				{
					return _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
					{
						SecurityProfileID = role.ToString()
					}, marketplaceToken);
				});
				await Task.WhenAll(sellerSecurityProfileAssignmentRequests);

				// assign full access security profile to default admin user
				var adminUsersList = await _oc.AdminUsers.ListAsync(filters: new { Username = SeedConstants.SellerUserName }, accessToken: marketplaceToken);
				var defaultAdminUser = adminUsersList.Items.FirstOrDefault();
				if (defaultAdminUser == null)
				{
					var exception = $@"Unable to find default admin user (username: {SeedConstants.SellerUserName}.";
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", exception, "", this, true);
					throw new Exception(exception);
				}
				await _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
				{
					SecurityProfileID = SeedConstants.FullAccessSecurityProfile,
					UserID = defaultAdminUser.ID
				}, marketplaceToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable CreateOnlyOnceBuyers task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOnlyOnceBuyers(Common.Models.Misc.EnvironmentSeed seed, string token)
		{
			try
			{
				// create default buyer if it does not exist
				// default buyer will have a well-known ID we can use to query with
				var defaultBuyer = await GetBuyerByID(SeedConstants.DefaultBuyerId, token);
				if (defaultBuyer == null)
				{
					var superBuyer = new SuperHsBuyer()
					{
						Buyer = SeedConstants.DefaultBuyer(),
						Markup = new BuyerMarkup() { Percent = 0 }
					};
					await _buyerCommand.Create(superBuyer, token, _oc);
				}

				// create seed buyers if they don't exist
				// seed buyers may not have ID defined, we are relying on Name instead
				foreach (var buyer in seed.Buyers)
				{
					var seedBuyer = await GetBuyerByName(buyer.Name, token);
					if (seedBuyer == null)
					{
						var superBuyer = new SuperHsBuyer()
						{
							Buyer = buyer,
							Markup = new BuyerMarkup() { Percent = 0 }
						};
						await _buyerCommand.Create(superBuyer, token, _oc);
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable CreateOnlyOnceAnonBuyerConfig task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOnlyOnceAnonBuyerConfig(Common.Models.Misc.EnvironmentSeed seed, string token)
		{
			try
			{
				// validate AnonymousShoppingBuyerID or provide fallback if none is defined
				var allBuyers = await _oc.Buyers.ListAllAsync(accessToken: token);
				if (seed.AnonymousShoppingBuyerId != null)
				{
					if (!allBuyers.Select(b => b.ID).Contains(seed.AnonymousShoppingBuyerId))
					{
						var exception = $@"The buyer defined by AnonymousShoppingBuyerID does not exist.";
						LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", exception, "", this, true);
						throw new Exception(exception);
					}
				}
				else
				{
					seed.AnonymousShoppingBuyerId = SeedConstants.DefaultBuyerId;
				}

				//create and assign initial buyer location
				await _buyerLocationCommand.Save(seed.AnonymousShoppingBuyerId, $@"{seed.AnonymousShoppingBuyerId}-{SeedConstants.DefaultLocationId}", SeedConstants.DefaultBuyerLocation(), token, _oc);

				// create user
				var anonBuyerUser = await _oc.Users.SaveAsync(seed.AnonymousShoppingBuyerId, SeedConstants.AnonymousBuyerUser().ID, SeedConstants.AnonymousBuyerUser(), token);

				// save assignment between user and buyergroup (location)
				var assignment = new UserGroupAssignment()
				{
					UserGroupID = $"{seed.AnonymousShoppingBuyerId}-{SeedConstants.DefaultLocationId}",
					UserID = anonBuyerUser.ID
				};
				await _oc.UserGroups.SaveUserAssignmentAsync(seed.AnonymousShoppingBuyerId, assignment, accessToken: token);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable CreateOnlyOnceBuyers task method
		/// </summary>
		/// <param name="buyerName"></param>
		/// <param name="token"></param>
		/// <returns>The HSBuyer response object by the buyerName</returns>
		private async Task<HsBuyer> GetBuyerByName(string buyerName, string token)
		{
			ListPage<HsBuyer> list = new ListPage<HsBuyer>();
			try
			{
				list = await _oc.Buyers.ListAsync<HsBuyer>(filters: new { Name = buyerName }, accessToken: token);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return list.Items.ToList().FirstOrDefault();
		}

		/// <summary>
		/// Private re-usable GetBuyerByID task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="token"></param>
		/// <returns>The HSBuyer response object by the buyerID</returns>
		private async Task<HsBuyer> GetBuyerByID(string buyerID, string token)
		{
			ListPage<HsBuyer> list = new ListPage<HsBuyer>();
			try
			{
				list = await _oc.Buyers.ListAsync<HsBuyer>(filters: new { ID = buyerID }, accessToken: token);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return list.Items.ToList().FirstOrDefault();
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateSuppliers task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateSuppliers(Common.Models.Misc.EnvironmentSeed seed, string token)
		{
			try
			{
				// create suppliers and necessary user groups and security profile assignments
				foreach (var supplier in seed.Suppliers)
				{
					var exists = await SupplierExistsAsync(supplier.Name, token);
					if (!exists)
					{
						await _supplierCommand.Create(supplier, token, isSeedingEnvironment: true);
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateProductFacets task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateProductFacets(string token)
		{
			try
			{
				var defaultFacet = SeedConstants.DefaultProductFacet();
				await _oc.ProductFacets.SaveAsync<HsProductFacet>(defaultFacet.ID, defaultFacet, token);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable SupplierExistsAsync task method
		/// </summary>
		/// <param name="supplierName"></param>
		/// <param name="token"></param>
		/// <returns>The boolean status for whether the Supplier exists or not</returns>
		private async Task<bool> SupplierExistsAsync(string supplierName, string token)
		{
			ListPage<Supplier> list = new ListPage<Supplier>();
			try
			{
				list = await _oc.Suppliers.ListAsync(filters: new { Name = supplierName }, accessToken: token);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return list.Items.Any();
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateDefaultSellerUser task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateDefaultSellerUser(Common.Models.Misc.EnvironmentSeed seed, string token)
		{
			try
			{
				// the middleware api client will use this user as the default context user
				var middlewareIntegrationsUser = SeedConstants.MiddlewareIntegrationsUser();
				await _oc.AdminUsers.SaveAsync(middlewareIntegrationsUser.ID, middlewareIntegrationsUser, token);

				// used to log in immediately after seeding the marketplace
				var initialAdminUser = new User
				{
					ID = "InitialAdminUser",
					Username = seed.InitialAdminUsername,
					Password = seed.InitialAdminPassword,
					Email = "test@test.com",
					Active = true,
					FirstName = "Initial",
					LastName = "User"
				};
				await _oc.AdminUsers.SaveAsync(initialAdminUser.ID, initialAdminUser, token);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable CreateOrUpdateXPIndices task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task CreateOrUpdateXPIndices(string token)
		{
			try
			{
				foreach (var index in SeedConstants.DefaultIndices)
				{
					try
					{
						await _oc.XpIndices.PutAsync(index, token);
					}
					catch (OrderCloudException ex)
					{
						// this is a bug in the api PUTs should never return 409s so ignore those errors
						// https://four51.atlassian.net/browse/EX-2210
						if (ex.HttpStatus != HttpStatusCode.Conflict)
						{
							LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
							throw ex;
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable CreateOnlyOnceIncrementors task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task CreateOnlyOnceIncrementors(string token)
		{
			try
			{
				foreach (var incrementor in SeedConstants.DefaultIncrementors)
				{
					var exists = await _oc.Incrementors.ListAsync(pageSize: 1, filters: new { ID = incrementor.ID }, accessToken: token);

					// only create the incrementor if it doesn't already exist otherwise the count will be reset and it may cause 409 conflict errors
					// when it tries to create an entity with an ID that has already been created
					if (!exists.Items.Any())
					{
						await _oc.Incrementors.CreateAsync(incrementor, token);
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable GetApiClients task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns>The ApiClients response object</returns>
		private async Task<ApiClients> GetApiClients(string token)
		{
			var resp = new ApiClients();
			try
			{
				var list = await _oc.ApiClients.ListAllAsync<HSApiClient>(accessToken: token);
				var appNames = list.Select(x => x.AppName);
				var adminUIApiClient = list.First(a => a.AppName == SeedConstants.SellerApiClientName);
				var buyerUIApiClient = list.First(a => a?.xp?.IsStorefront == true);
				var buyerLocalUIApiClient = list.First(a => a.AppName == SeedConstants.BuyerLocalApiClientName);
				var middlewareApiClient = list.First(a => a.AppName == SeedConstants.IntegrationsApiClientName);
				resp = new ApiClients()
				{
					AdminUiApiClient = adminUIApiClient,
					BuyerUiApiClient = buyerUIApiClient,
					BuyerLocalUiApiClient = buyerLocalUIApiClient,
					MiddlewareApiClient = middlewareApiClient
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable GetStoreFrontClientIDs task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns>The ApiClients response object</returns>
		private async Task<string[]> GetStoreFrontClientIDs(string token)
		{
			var list = new List<HSApiClient>();
			try
			{
				list = await _oc.ApiClients.ListAllAsync<HSApiClient>(accessToken: token);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return list.Where(client => client?.xp?.IsStorefront == true).Select(client => client.ID).ToArray();
		}

		public class ApiClients
		{
			public ApiClient AdminUiApiClient { get; set; }
			public ApiClient BuyerUiApiClient { get; set; }
			public ApiClient BuyerLocalUiApiClient { get; set; }
			public ApiClient MiddlewareApiClient { get; set; }
		}

		/// <summary>
		/// Private re-usable CreateOnlyOnceApiClients task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOnlyOnceApiClients(Common.Models.Misc.EnvironmentSeed seed, string token)
		{
			var existingClients = await _oc.ApiClients.ListAllAsync(accessToken: token);

			await CreateOrGetBuyerClient(existingClients, SeedConstants.BuyerClient(seed), seed, token);
			await CreateOrGetApiClient(existingClients, SeedConstants.IntegrationsClient(), token);
			await CreateOrGetApiClient(existingClients, SeedConstants.SellerClient(), token);
			await CreateOrGetApiClient(existingClients, SeedConstants.BuyerLocalClient(seed), token);
		}

		/// <summary>
		/// Private re-usable CreateOrGetBuyerClient task method
		/// </summary>
		/// <param name="existingClients"></param>
		/// <param name="client"></param>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns>The ApiClient response object</returns>
		private async Task<ApiClient> CreateOrGetBuyerClient(List<ApiClient> existingClients, ApiClient client, Common.Models.Misc.EnvironmentSeed seed, string token)
		{
			var match = existingClients.FirstOrDefault(c => c.AppName == client.AppName);
			try
			{
				if (match == null)
				{
					await CreateOnlyOnceAnonBuyerConfig(seed, token);
					var apiClient = await _oc.ApiClients.CreateAsync(client, token);
					return apiClient;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return match;
		}

		/// <summary>
		/// Private re-usable CreateOrGetApiClient task method
		/// </summary>
		/// <param name="existingClients"></param>
		/// <param name="client"></param>
		/// <param name="token"></param>
		/// <returns>The ApiClient response object</returns>
		private async Task<ApiClient> CreateOrGetApiClient(List<ApiClient> existingClients, ApiClient client, string token)
		{
			var match = existingClients.FirstOrDefault(c => c.AppName == client.AppName);
			try
			{
				if (match == null)
				{
					match = await _oc.ApiClients.CreateAsync(client, token);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return match;
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateMessageSendersAndAssignments task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateMessageSendersAndAssignments(Common.Models.Misc.EnvironmentSeed seed, string accessToken)
		{
			try
			{
				var defaultMessageSenders = new List<MessageSender>()
				{
					SeedConstants.BuyerEmails(seed),
					SeedConstants.SellerEmails(seed),
					SeedConstants.SupplierEmails(seed)
				};

				var existingMessageSenders = await _oc.MessageSenders.ListAllAsync(accessToken: accessToken);
				foreach (var sender in defaultMessageSenders)
				{
					var messageSender = await GetOrCreateMessageSender(existingMessageSenders, sender, accessToken);
					if (messageSender.ID.Trim().Equals($@"BuyerEmails", StringComparison.OrdinalIgnoreCase))
					{
						var allBuyers = await _oc.Buyers.ListAllAsync(accessToken: accessToken);
						foreach (var buyer in allBuyers)
						{
							try
							{
								await _oc.MessageSenders.SaveAssignmentAsync(new MessageSenderAssignment
								{
									MessageSenderID = messageSender.ID,
									BuyerID = buyer.ID
								}, accessToken);
							}
							catch (OrderCloudException ex)
							{
								// this is a bug in the api PUTs should never return 409s so ignore those errors
								// https://four51.atlassian.net/browse/EX-2210
								if (ex.HttpStatus != HttpStatusCode.Conflict)
								{
									LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
									throw ex;
								}
							}
						}
					}
					else if (messageSender.ID.Trim().Equals($@"SellerEmails", StringComparison.OrdinalIgnoreCase))
					{
						try
						{
							await _oc.MessageSenders.SaveAssignmentAsync(new MessageSenderAssignment
							{
								MessageSenderID = messageSender.ID
							}, accessToken);
						}
						catch (OrderCloudException ex)
						{
							// this is a bug in the api PUTs should never return 409s so ignore those errors
							// https://four51.atlassian.net/browse/EX-2210
							if (ex.HttpStatus != HttpStatusCode.Conflict)
							{
								LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
								throw ex;
							}
						}
					}
					else if (messageSender.ID.Trim().Equals($@"SupplierEmails", StringComparison.OrdinalIgnoreCase))
					{
						var allSuppliers = await _oc.Suppliers.ListAllAsync(accessToken: accessToken);
						foreach (var supplier in allSuppliers)
						{
							try
							{
								await _oc.MessageSenders.SaveAssignmentAsync(new MessageSenderAssignment
								{
									MessageSenderID = messageSender.ID,
									SupplierID = supplier.ID
								}, accessToken);
							}
							catch (OrderCloudException ex)
							{
								// this is a bug in the api PUTs should never return 409s so ignore those errors
								// https://four51.atlassian.net/browse/EX-2210
								if (ex.HttpStatus != HttpStatusCode.Conflict)
								{
									LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
									throw ex;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable GetOrCreateMessageSender task method
		/// </summary>
		/// <param name="existingMessageSenders"></param>
		/// <param name="messageSender"></param>
		/// <param name="accessToken"></param>
		/// <returns>The MessageSender response object</returns>
		private async Task<MessageSender> GetOrCreateMessageSender(List<MessageSender> existingMessageSenders, MessageSender messageSender, string accessToken)
		{
			var match = existingMessageSenders.Find(c => c.ID == messageSender.ID);
			try
			{
				if (match == null)
				{
					match = await _oc.MessageSenders.CreateAsync(messageSender, accessToken);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return match;
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateAndAssignIntegrationEvents task method
		/// </summary>
		/// <param name="token"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateAndAssignIntegrationEvents(string token, Common.Models.Misc.EnvironmentSeed seed = null)
		{
			try
			{
				var storefrontApiClientIDs = await GetStoreFrontClientIDs(token);
				var apiClients = await GetApiClients(token);
				var localBuyerClientID = apiClients.BuyerLocalUiApiClient.ID;

				// this gets called by both the /seed command and the post-staging restore so we need to handle getting settings from two sources
				var middlewareBaseUrl = seed != null ? seed.MiddlewareBaseUrl : _settings.EnvironmentSettings.MiddlewareBaseUrl;
				var webhookHashKey = seed != null ? seed.OrderCloudSeedSettings.WebhookHashKey : _settings.OrderCloudSettings.WebhookHashKey;
				var checkoutEvent = SeedConstants.CheckoutEvent(middlewareBaseUrl, webhookHashKey);
				await _oc.IntegrationEvents.SaveAsync(checkoutEvent.ID, checkoutEvent, token);
				var localCheckoutEvent = SeedConstants.LocalCheckoutEvent(webhookHashKey);
				await _oc.IntegrationEvents.SaveAsync(localCheckoutEvent.ID, localCheckoutEvent, token);

				await _oc.ApiClients.PatchAsync(localBuyerClientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckoutLOCAL" }, token);
				await Throttler.RunAsync(storefrontApiClientIDs, 500, 20, clientID =>
					_oc.ApiClients.PatchAsync(clientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckout" }, token));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable ShutOffSupplierEmailsAsync task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task ShutOffSupplierEmailsAsync(string token)
		{
			try
			{
				var allSuppliers = await _oc.Suppliers.ListAllAsync(accessToken: token);
				await Throttler.RunAsync(allSuppliers, 500, 20, supplier =>
					_oc.Suppliers.PatchAsync(supplier.ID, new PartialSupplier { xp = new { NotificationRcpts = new string[] { } } }, token));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable CreateOrUpdateSecurityProfiles task method
		/// </summary>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		public async Task CreateOrUpdateSecurityProfiles(string accessToken)
		{
			try
			{
				var profiles = SeedConstants.DefaultSecurityProfiles.Select(p =>
					new SecurityProfile()
					{
						Name = p.Id.ToString(),
						ID = p.Id.ToString(),
						CustomRoles = p.CustomRoles.Select(r => r.ToString()).ToList(),
						Roles = p.Roles
					}).ToList();

				profiles.Add(new SecurityProfile()
				{
					Roles = new List<ApiRole> { ApiRole.FullAccess },
					Name = SeedConstants.FullAccessSecurityProfile,
					ID = SeedConstants.FullAccessSecurityProfile
				});

				var profileCreateRequests = profiles.Select(p => _oc.SecurityProfiles.SaveAsync(p.ID, p, accessToken));
				await Task.WhenAll(profileCreateRequests);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable DeleteAllMessageSenders task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task DeleteAllMessageSenders(string token)
		{
			try
			{
				var messageSenders = await _oc.MessageSenders.ListAllAsync(accessToken: token);
				await Throttler.RunAsync(messageSenders, 500, 20, messageSender =>
					_oc.MessageSenders.DeleteAsync(messageSender.ID, accessToken: token));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable DeleteAllIntegrationEvents task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task DeleteAllIntegrationEvents(string token)
		{
			try
			{
				// can't delete integration event if its referenced by an api client so first patch it to null
				var apiClientsWithIntegrationEvent = await _oc.IntegrationEvents.ListAllAsync(filters: new { OrderCheckoutIntegrationEventID = "*" }, accessToken: token);
				await Throttler.RunAsync(apiClientsWithIntegrationEvent, 500, 20, apiClient =>
					_oc.ApiClients.PatchAsync(apiClient.ID, new PartialApiClient { OrderCheckoutIntegrationEventID = null }, accessToken: token));

				var integrationEvents = await _oc.IntegrationEvents.ListAllAsync(accessToken: token);
				await Throttler.RunAsync(integrationEvents, 500, 20, integrationEvent =>
					_oc.IntegrationEvents.DeleteAsync(integrationEvent.ID, accessToken: token));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}
	}
}