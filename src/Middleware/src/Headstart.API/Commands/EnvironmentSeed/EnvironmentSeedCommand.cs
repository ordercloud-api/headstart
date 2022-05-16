using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Headstart.Common.Services.Portal.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using Headstart.Models.Misc;
using Microsoft.WindowsAzure.Storage.Blob;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands
{
	public interface IEnvironmentSeedCommand
	{
		Task<EnvironmentSeedResponse> Seed(EnvironmentSeed seed);
		Task UpdateTranslations(string connectionString, string containerName);
		Task PostStagingRestore();
	}

	public class EnvironmentSeedCommand : IEnvironmentSeedCommand
	{
		private readonly AppSettings settings;
		private readonly IPortalService portal;
		private readonly IHSSupplierCommand supplierCommand;
		private readonly IHSBuyerCommand buyerCommand;
		private readonly IHSBuyerLocationCommand buyerLocationCommand;
		private IOrderCloudClient oc;
		
		/// <summary>
		/// The IOC based constructor method for the EnvironmentSeedCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="portal"></param>
		/// <param name="supplierCommand"></param>
		/// <param name="buyerCommand"></param>
		/// <param name="buyerLocationCommand"></param>
		/// <param name="oc"></param>
		public EnvironmentSeedCommand(AppSettings settings, IPortalService portal, IHSSupplierCommand supplierCommand, IHSBuyerCommand buyerCommand, IHSBuyerLocationCommand buyerLocationCommand, IOrderCloudClient oc)
		{
			try
			{
				this.settings = settings;
				this.portal = portal;
				this.supplierCommand = supplierCommand;
				this.buyerCommand = buyerCommand;
				this.buyerLocationCommand = buyerLocationCommand;
				this.oc = oc;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// This seeding function can be used to initially seed an marketplace
		/// it is also meant to be safe to call after an marketplace has been seeded (by including seed.MarketplaceID)
		/// If a method starts with CreateOrUpdate it will update the resource every time its called based on what has been defined in SeedConstants.cs
		/// If a method starts with CreateOnlyOnce it will only create the resource once and then ignore thereafter
		/// The CreateOnlyOnce resources are likely to change after initial creation so we ignore to avoid overwriting desired changes that happen outside of seeding.
		/// </summary>
		/// <param name="seed"></param>
		/// <returns>The EnvironmentSeedResponse object from the Seed process</returns>
		public async Task<EnvironmentSeedResponse> Seed(EnvironmentSeed seed)
		{
			var requestedEnv = SeedConstants.OrderCloudEnvironment(seed.OrderCloudSeedSettings);
			if (requestedEnv.EnvironmentName.Equals(SeedConstants.Environments.Production, StringComparison.OrdinalIgnoreCase) && seed.MarketplaceID == null)
			{
				throw new Exception("Cannot create a production environment via the environment seed endpoint. Please contact an OrderCloud Developer to create a production marketplace.");
			}
			// lets us handle requests to multiple api environments
			oc = new OrderCloudClient(new OrderCloudClientConfig
			{
				ApiUrl = requestedEnv.ApiUrl,
				AuthUrl = requestedEnv.ApiUrl,
			});

			var portalUserToken = await portal.Login(seed.PortalUsername, seed.PortalPassword);
			var marketplace = await GetOrCreateMarketplace(portalUserToken, seed, requestedEnv);
			var marketplaceToken = await portal.GetMarketplaceToken(marketplace.Id, portalUserToken);

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
			return new EnvironmentSeedResponse
			{
				Comments = "Success! Your environment is now seeded. The following clientIDs & secrets should be used to finalize the configuration of your application. The initial admin username and password can be used to sign into your admin application",
				MarketplaceName = marketplace.Name,
				MarketplaceID = marketplace.Id,
				OrderCloudEnvironment = requestedEnv.EnvironmentName,
				ApiClients = new Dictionary<string, dynamic>
				{
					["Middleware"] = new
					{
						ClientID = apiClients.MiddlewareApiClient.ID,
						ClientSecret = apiClients.MiddlewareApiClient.ClientSecret,
					},
					["Seller"] = new
					{
						ClientID = apiClients.AdminUiApiClient.ID,
					},
					["Buyer"] = new
					{
						ClientID = apiClients.BuyerUiApiClient.ID,
					},
				},
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
			var englishTranslationsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Assets", "english-translations.json"));
			var translationsConfig = new BlobServiceConfig()
			{
				ConnectionString = connectionString,
				Container = containerName,
				AccessType = BlobContainerPublicAccessType.Container,
			};
			var translationsBlob = new OrderCloudIntegrationsBlobService(translationsConfig);
			await translationsBlob.Save("i18n/en.json", File.ReadAllText(englishTranslationsPath));
		}

		/// <summary>
		/// Public re-usable VerifyMarketplaceExists task method
		/// </summary>
		/// <param name="marketplaceID"></param>
		/// <param name="devToken"></param>
		/// <returns>The Marketplace object</returns>
		public async Task<Marketplace> VerifyMarketplaceExists(string marketplaceID, string devToken)
		{
			try
			{
				return await portal.GetMarketplace(marketplaceID, devToken);
			}
			catch (Exception e)
			{
				// the portal API no longer allows us to create a production marketplace outside of portal
				// though its possible to create on sandbox - for consistency sake we'll require its created before seeding
				Console.WriteLine(e.Message);
				throw new Exception("Failed to retrieve marketplace with MarketplaceID. The marketplace must exist before it can be seeded");
			}
		}

		/// <summary>
		/// Public re-usable GetOrCreateMarketplace task method
		/// </summary>
		/// <param name="token"></param>
		/// <param name="seed"></param>
		/// <param name="env"></param>
		/// <returns>The Marketplace object</returns>
		public async Task<Marketplace> GetOrCreateMarketplace(string token, EnvironmentSeed seed, OcEnv env)
		{
			if (seed.MarketplaceID != null)
			{
				var existingMarketplace = await VerifyMarketplaceExists(seed.MarketplaceID, token);
				return existingMarketplace;
			}
			else
			{
				var marketPlaceToCreate = ConstructMarketplaceFromSeed(seed, env);
				try
				{
					await portal.GetMarketplace(marketPlaceToCreate.Id, token);
					return await GetOrCreateMarketplace(token, seed, env);
				}
				catch
				{
					await portal.CreateMarketplace(marketPlaceToCreate, token);
					return await portal.GetMarketplace(marketPlaceToCreate.Id, token);
				}
			}
		}

		/// <summary>
		/// The staging environment gets restored weekly from production
		/// during that restore things like webhooks, message senders, and integration events are shut off (so you don't for example email production customers)
		/// this process restores integration events which are required for checkout (with environment specific settings).
		/// </summary>
		/// <returns></returns>
		public async Task PostStagingRestore()
		{
			var token = (await oc.AuthenticateAsync()).AccessToken;

			var deleteIE = DeleteAllIntegrationEvents(token);
			await Task.WhenAll(deleteIE);

			// recreate with environment specific data
			var createIE = CreateOrUpdateAndAssignIntegrationEvents(token);
			var shutOffSupplierEmails = ShutOffSupplierEmailsAsync(token); // shut off email notifications for all suppliers
			await Task.WhenAll(createIE, shutOffSupplierEmails);
		}
		
		/// <summary>
		/// Public re-usable CreateOrUpdateXPIndices task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task CreateOrUpdateXPIndices(string token)
		{
			foreach (var index in SeedConstants.DefaultIndices)
			{
				try
				{
					await oc.XpIndices.PutAsync(index, token);
				}
				catch (OrderCloudException ex)
				{
					// this is a bug in the api PUTs should never return 409s so ignore those errors
					// https://four51.atlassian.net/browse/EX-2210
					if (ex.HttpStatus != HttpStatusCode.Conflict)
					{
						throw ex;
					}
				}
			}
		}
		
		/// <summary>
		/// Public re-usable CreateOnlyOnceIncrementors task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task CreateOnlyOnceIncrementors(string token)
		{
			foreach (var incrementor in SeedConstants.DefaultIncrementors)
			{
				var exists = await oc.Incrementors.ListAsync(pageSize: 1, filters: new { ID = incrementor.ID }, accessToken: token);

				// only create the incrementor if it doesn't already exist otherwise the count will be reset and it may cause 409 conflict errors
				// when it tries to create an entity with an ID that has already been created
				if (!exists.Items.Any())
				{
					await oc.Incrementors.CreateAsync(incrementor, token);
				}
			}
		}
		
		/// <summary>
		/// Public re-usable CreateOrUpdateSecurityProfiles task method
		/// </summary>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		public async Task CreateOrUpdateSecurityProfiles(string accessToken)
		{
			var profiles = SeedConstants.DefaultSecurityProfiles.Select(p =>
				new SecurityProfile()
				{
					Name = p.ID.ToString(),
					ID = p.ID.ToString(),
					CustomRoles = p.CustomRoles.Select(r => r.ToString()).ToList(),
					Roles = p.Roles,
				}).ToList();

			profiles.Add(new SecurityProfile()
			{
				Roles = new List<ApiRole> { ApiRole.FullAccess },
				Name = SeedConstants.FullAccessSecurityProfile,
				ID = SeedConstants.FullAccessSecurityProfile,
			});

			var profileCreateRequests = profiles.Select(p => oc.SecurityProfiles.SaveAsync(p.ID, p, accessToken));
			await Task.WhenAll(profileCreateRequests);
		}

		/// <summary>
		/// Public re-usable DeleteAllMessageSenders task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task DeleteAllMessageSenders(string token)
		{
			var messageSenders = await oc.MessageSenders.ListAllAsync(accessToken: token);
			await Throttler.RunAsync(messageSenders, 500, 20, messageSender =>
				oc.MessageSenders.DeleteAsync(messageSender.ID, accessToken: token));
		}

		/// <summary>
		/// Public re-usable DeleteAllIntegrationEvents task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task DeleteAllIntegrationEvents(string token)
		{
			// can't delete integration event if its referenced by an api client so first patch it to null
			var apiClientsWithIntegrationEvent = await oc.IntegrationEvents.ListAllAsync(filters: new { OrderCheckoutIntegrationEventID = "*" }, accessToken: token);
			await Throttler.RunAsync(apiClientsWithIntegrationEvent, 500, 20, apiClient =>
				oc.ApiClients.PatchAsync(apiClient.ID, new PartialApiClient { OrderCheckoutIntegrationEventID = null }, accessToken: token));

			var integrationEvents = await oc.IntegrationEvents.ListAllAsync(accessToken: token);
			await Throttler.RunAsync(integrationEvents, 500, 20, integrationEvent =>
				oc.IntegrationEvents.DeleteAsync(integrationEvent.ID, accessToken: token));
		}
		
		/// <summary>
		/// Private re-usable ConstructMarketplaceFromSeed task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="requestedEnv"></param>
		/// <returns>The Marketplace object</returns>
		private static Marketplace ConstructMarketplaceFromSeed(EnvironmentSeed seed, OcEnv requestedEnv)
		{
			return new Marketplace()
			{
				Id = Guid.NewGuid().ToString(),
				Environment = requestedEnv.EnvironmentName,
				Name = seed.MarketplaceName == null ? "My Headstart Marketplace" : seed.MarketplaceName,
				Region = requestedEnv.Region,
			};
		}
		
		/// <summary>
		/// Private re-usable CreateOnlyOnceApiClients task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOnlyOnceApiClients(EnvironmentSeed seed, string token)
		{
			var existingClients = await oc.ApiClients.ListAllAsync(accessToken: token);

			await CreateOrGetBuyerClient(existingClients, SeedConstants.BuyerClient(seed), seed, token);
			await CreateOrGetApiClient(existingClients, SeedConstants.IntegrationsClient(), token);
			await CreateOrGetApiClient(existingClients, SeedConstants.SellerClient(), token);
			await CreateOrGetApiClient(existingClients, SeedConstants.BuyerLocalClient(seed), token);
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateSecurityProfileAssignments task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="marketplaceToken"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateSecurityProfileAssignments(EnvironmentSeed seed, string marketplaceToken)
		{
			// assign buyer security profiles
			var buyerSecurityProfileAssignmentRequests = seed.Buyers.Select(b =>
			{
				return oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment
				{
					BuyerID = b.ID,
					SecurityProfileID = CustomRole.HSBaseBuyer.ToString(),
				}, marketplaceToken);
			});
			await Task.WhenAll(buyerSecurityProfileAssignmentRequests);

			// assign seller security profiles to seller marketplace
			var sellerSecurityProfileAssignmentRequests = SeedConstants.SellerHsRoles.Select(role =>
			{
				return oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
				{
					SecurityProfileID = role.ToString(),
				}, marketplaceToken);
			});
			await Task.WhenAll(sellerSecurityProfileAssignmentRequests);

			// assign full access security profile to default admin user
			var adminUsersList = await oc.AdminUsers.ListAsync(filters: new { Username = SeedConstants.SellerUserName }, accessToken: marketplaceToken);
			var defaultAdminUser = adminUsersList.Items.FirstOrDefault();
			if (defaultAdminUser == null)
			{
				throw new Exception($"Unable to find default admin user (username: {SeedConstants.SellerUserName}");
			}
			await oc.SecurityProfiles.SaveAssignmentAsync(
                new SecurityProfileAssignment()
                {
                    SecurityProfileID = SeedConstants.FullAccessSecurityProfile,
                    UserID = defaultAdminUser.ID,
                }, marketplaceToken);
		}

		/// <summary>
		/// Private re-usable CreateOnlyOnceBuyers task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOnlyOnceBuyers(EnvironmentSeed seed, string token)
		{
			// create default buyer if it does not exist
			// default buyer will have a well-known ID we can use to query with
			var defaultBuyer = await GetBuyerByID(SeedConstants.DefaultBuyerID, token);
			if (defaultBuyer == null)
			{
				var superBuyer = new SuperHSBuyer()
				{
					Buyer = SeedConstants.DefaultBuyer(),
                    Markup = new BuyerMarkup() { Percent = 0 },
				};
                await buyerCommand.Create(superBuyer, token, oc);
			}

			// create seed buyers if they don't exist
			// seed buyers may not have ID defined, we are relying on Name instead
			foreach (var buyer in seed.Buyers)
			{
				var seedBuyer = await GetBuyerByName(buyer.Name, token);
				if (seedBuyer == null)
				{
					var superBuyer = new SuperHSBuyer()
					{
						Buyer = buyer,
                        Markup = new BuyerMarkup() { Percent = 0 },
					};
                    await buyerCommand.Create(superBuyer, token, oc);
				}
			}
		}

		/// <summary>
		/// Private re-usable CreateOnlyOnceAnonBuyerConfig task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOnlyOnceAnonBuyerConfig(EnvironmentSeed seed, string token)
		{
			// validate AnonymousShoppingBuyerID or provide fallback if none is defined
            var allBuyers = await oc.Buyers.ListAllAsync(accessToken: token);
			if (!string.IsNullOrWhiteSpace(seed.AnonymousShoppingBuyerID))
			{
				if (!allBuyers.Select(b => b.ID).Contains(seed.AnonymousShoppingBuyerID))
				{
					throw new Exception("The buyer defined by AnonymousShoppingBuyerID does not exist");
				}
			}
			else
			{
				seed.AnonymousShoppingBuyerID = SeedConstants.DefaultBuyerID;
			}

			//create and assign initial buyer location
            await buyerLocationCommand.Save(
                seed.AnonymousShoppingBuyerID,
				$"{seed.AnonymousShoppingBuyerID}-{SeedConstants.DefaultLocationID}",
                SeedConstants.DefaultBuyerLocation(),
                token,
                oc);

			// create user
            var anonBuyerUser = await oc.Users.SaveAsync(seed.AnonymousShoppingBuyerID, SeedConstants.AnonymousBuyerUser().ID, SeedConstants.AnonymousBuyerUser(), token);

			// save assignment between user and buyergroup (location)
			var assignment = new UserGroupAssignment()
			{
				UserGroupID = $"{seed.AnonymousShoppingBuyerID}-{SeedConstants.DefaultLocationID}",
                UserID = anonBuyerUser.ID,
			};
            await oc.UserGroups.SaveUserAssignmentAsync(seed.AnonymousShoppingBuyerID, assignment, accessToken: token);
		}

		/// <summary>
		/// Private re-usable CreateOnlyOnceBuyers task method
		/// </summary>
		/// <param name="buyerName"></param>
		/// <param name="token"></param>
		/// <returns>The HSBuyer object by the buyerName</returns>
		private async Task<HSBuyer> GetBuyerByName(string buyerName, string token)
		{
            var list = await oc.Buyers.ListAsync<HSBuyer>(filters: new { Name = buyerName }, accessToken: token);
			return list.Items.ToList().FirstOrDefault();
		}

		/// <summary>
		/// Private re-usable GetBuyerByID task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="token"></param>
		/// <returns>The HSBuyer object by the buyerID</returns>
		private async Task<HSBuyer> GetBuyerByID(string buyerID, string token)
		{
            var list = await oc.Buyers.ListAsync<HSBuyer>(filters: new { ID = buyerID }, accessToken: token);
			return list.Items.ToList().FirstOrDefault();
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateSuppliers task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateSuppliers(EnvironmentSeed seed, string token)
		{
			// Create Suppliers and necessary user groups and security profile assignments
			foreach (HSSupplier supplier in seed.Suppliers)
			{
				var exists = await SupplierExistsAsync(supplier.Name, token);
				if (!exists)
				{
                    await supplierCommand.Create(supplier, token, isSeedingEnvironment: true);
				}
			}
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateProductFacets task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateProductFacets(string token)
		{
			var defaultFacet = SeedConstants.DefaultProductFacet();
            await oc.ProductFacets.SaveAsync<HSProductFacet>(defaultFacet.ID, defaultFacet, token);
		}

		/// <summary>
		/// Private re-usable SupplierExistsAsync task method
		/// </summary>
		/// <param name="supplierName"></param>
		/// <param name="token"></param>
		/// <returns>The boolean status for whether the Supplier exists or not</returns>
		private async Task<bool> SupplierExistsAsync(string supplierName, string token)
		{
            var list = await oc.Suppliers.ListAsync(filters: new { Name = supplierName }, accessToken: token);
			return list.Items.Any();
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateDefaultSellerUser task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateDefaultSellerUser(EnvironmentSeed seed, string token)
		{
			// the middleware api client will use this user as the default context user
			var middlewareIntegrationsUser = SeedConstants.MiddlewareIntegrationsUser();

            await oc.AdminUsers.SaveAsync(middlewareIntegrationsUser.ID, middlewareIntegrationsUser, token);

			// used to log in immediately after seeding the marketplace
			var initialAdminUser = new User
			{
				ID = "InitialAdminUser",
				Username = seed.InitialAdminUsername,
				Password = seed.InitialAdminPassword,
				Email = "test@test.com",
				Active = true,
				FirstName = "Initial",
                LastName = "User",
			};
            await oc.AdminUsers.SaveAsync(initialAdminUser.ID, initialAdminUser, token);
		}

		/// <summary>
		/// Private re-usable GetApiClients task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns>The ApiClients object</returns>
		private async Task<ApiClients> GetApiClients(string token)
		{
            var list = await oc.ApiClients.ListAllAsync<HSApiClient>(accessToken: token);
			var appNames = list.Select(x => x.AppName);
			var adminUIApiClient = list.First(a => a.AppName == SeedConstants.SellerApiClientName);
			var buyerUIApiClient = list.First(a => a?.xp?.IsStorefront == true);
			var buyerLocalUIApiClient = list.First(a => a.AppName == SeedConstants.BuyerLocalApiClientName);
			var middlewareApiClient = list.First(a => a.AppName == SeedConstants.IntegrationsApiClientName);
			return new ApiClients()
			{
				AdminUiApiClient = adminUIApiClient,
				BuyerUiApiClient = buyerUIApiClient,
				BuyerLocalUiApiClient = buyerLocalUIApiClient,
                MiddlewareApiClient = middlewareApiClient,
			};
		}

		/// <summary>
		/// Private re-usable GetStoreFrontClientIDs task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns>The ApiClients object</returns>
		private async Task<string[]> GetStoreFrontClientIDs(string token)
		{
            var list = await oc.ApiClients.ListAllAsync<HSApiClient>(accessToken: token);
			return list
				.Where(client => client?.xp?.IsStorefront == true) // can't index ApiClients so we need to filter client-side
				.Select(client => client.ID).ToArray();
		}

		/// <summary>
		/// Private re-usable CreateOrGetBuyerClient task method
		/// </summary>
		/// <param name="existingClients"></param>
		/// <param name="client"></param>
		/// <param name="seed"></param>
		/// <param name="token"></param>
		/// <returns>The ApiClient object</returns>
		private async Task<ApiClient> CreateOrGetBuyerClient(List<ApiClient> existingClients, ApiClient client, EnvironmentSeed seed, string token)
		{
			var match = existingClients.FirstOrDefault(c => c.AppName == client.AppName);
			if (match == null)
			{
				await CreateOnlyOnceAnonBuyerConfig(seed, token);
                var apiClient = await oc.ApiClients.CreateAsync(client, token);
				return apiClient;
			}
			return match;
		}

		private async Task<ApiClient> CreateOrGetApiClient(List<ApiClient> existingClients, ApiClient client, string token)
		{
			var match = existingClients.FirstOrDefault(c => c.AppName == client.AppName);
			if (match == null)
			{
                return await oc.ApiClients.CreateAsync(client, token);
			}
			return match;
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateMessageSendersAndAssignments task method
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateMessageSendersAndAssignments(EnvironmentSeed seed, string accessToken)
		{
			var defaultMessageSenders = new List<MessageSender>()
			{
				SeedConstants.BuyerEmails(seed),
				SeedConstants.SellerEmails(seed),
                SeedConstants.SuplierEmails(seed),
			};
            var existingMessageSenders = await oc.MessageSenders.ListAllAsync(accessToken: accessToken);
			foreach (var sender in defaultMessageSenders)
			{
				var messageSender = await GetOrCreateMessageSender(existingMessageSenders, sender, accessToken);
				if (messageSender.ID == "BuyerEmails")
				{
                    var allBuyers = await oc.Buyers.ListAllAsync(accessToken: accessToken);
					foreach (var buyer in allBuyers)
					{
						try
						{
                            await oc.MessageSenders.SaveAssignmentAsync(
                                new MessageSenderAssignment
							{
								MessageSenderID = messageSender.ID,
                                    BuyerID = buyer.ID,
							}, accessToken);
						}
						catch (OrderCloudException ex)
						{
							// this is a bug in the api PUTs should never return 409s so ignore those errors
							// https://four51.atlassian.net/browse/EX-2210
							if (ex.HttpStatus != HttpStatusCode.Conflict)
							{
								throw ex;
							}
						}
					}
				}
				else if (messageSender.ID == "SellerEmails")
				{
					try
					{
                        await oc.MessageSenders.SaveAssignmentAsync(
                            new MessageSenderAssignment
						{
                                MessageSenderID = messageSender.ID,
						}, accessToken);
					}
					catch (OrderCloudException ex)
					{
						// this is a bug in the api PUTs should never return 409s so ignore those errors
						// https://four51.atlassian.net/browse/EX-2210
						if (ex.HttpStatus != HttpStatusCode.Conflict)
						{
							throw ex;
						}
					}
				}
				else if (messageSender.ID == "SupplierEmails")
				{
                    var allSuppliers = await oc.Suppliers.ListAllAsync(accessToken: accessToken);
					foreach (var supplier in allSuppliers)
					{
						try
						{
                            await oc.MessageSenders.SaveAssignmentAsync(
                                new MessageSenderAssignment
							{
								MessageSenderID = messageSender.ID,
                                    SupplierID = supplier.ID,
							}, accessToken);
						}
						catch (OrderCloudException ex)
						{
							// this is a bug in the api PUTs should never return 409s so ignore those errors
							// https://four51.atlassian.net/browse/EX-2210
							if (ex.HttpStatus != HttpStatusCode.Conflict)
							{
								throw ex;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Private re-usable GetOrCreateMessageSender task method
		/// </summary>
		/// <param name="existingMessageSenders"></param>
		/// <param name="messageSender"></param>
		/// <param name="accessToken"></param>
		/// <returns>The MessageSender object</returns>
		private async Task<MessageSender> GetOrCreateMessageSender(List<MessageSender> existingMessageSenders, MessageSender messageSender, string accessToken)
		{
			var match = existingMessageSenders.Find(c => c.ID == messageSender.ID);
			if (match == null)
			{
                return await oc.MessageSenders.CreateAsync(messageSender, accessToken);
			}
			return match;
		}

		/// <summary>
		/// Private re-usable CreateOrUpdateAndAssignIntegrationEvents task method
		/// </summary>
		/// <param name="token"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		private async Task CreateOrUpdateAndAssignIntegrationEvents(string token, EnvironmentSeed seed = null)
		{
			var storefrontApiClientIDs = await GetStoreFrontClientIDs(token);
			var apiClients = await GetApiClients(token);
			var localBuyerClientID = apiClients.BuyerLocalUiApiClient.ID;

			// this gets called by both the /seed command and the post-staging restore so we need to handle getting settings from two sources
            var middlewareBaseUrl = seed != null ? seed.MiddlewareBaseUrl : settings.EnvironmentSettings.MiddlewareBaseUrl;
            var webhookHashKey = seed != null ? seed.OrderCloudSeedSettings.WebhookHashKey : settings.OrderCloudSettings.WebhookHashKey;
			var checkoutEvent = SeedConstants.CheckoutEvent(middlewareBaseUrl, webhookHashKey);
            await oc.IntegrationEvents.SaveAsync(checkoutEvent.ID, checkoutEvent, token);
			var localCheckoutEvent = SeedConstants.LocalCheckoutEvent(webhookHashKey);
            await oc.IntegrationEvents.SaveAsync(localCheckoutEvent.ID, localCheckoutEvent, token);

            await oc.ApiClients.PatchAsync(localBuyerClientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckoutLOCAL" }, token);
			await Throttler.RunAsync(storefrontApiClientIDs, 500, 20, clientID =>
                oc.ApiClients.PatchAsync(clientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckout" }, token));
		}

		/// <summary>
		/// Private re-usable ShutOffSupplierEmailsAsync task method
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task ShutOffSupplierEmailsAsync(string token)
		{
            var allSuppliers = await oc.Suppliers.ListAllAsync(accessToken: token);
			await Throttler.RunAsync(allSuppliers, 500, 20, supplier =>
                oc.Suppliers.PatchAsync(supplier.ID, new PartialSupplier { xp = new { NotificationRcpts = new string[] { } } }, token));
        }

        public class ApiClients
        {
            public ApiClient AdminUiApiClient { get; set; }

            public ApiClient BuyerUiApiClient { get; set; }

            public ApiClient BuyerLocalUiApiClient { get; set; }

            public ApiClient MiddlewareApiClient { get; set; }
        }
    }
}
