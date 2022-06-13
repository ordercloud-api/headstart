using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Constants;
using Headstart.Common.Models;
using Headstart.Common.Settings;
using Microsoft.WindowsAzure.Storage.Blob;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.AzureStorage;
using OrderCloud.Integrations.EnvironmentSeed.Helpers;
using OrderCloud.Integrations.EnvironmentSeed.Models;
using OrderCloud.Integrations.Portal;
using OrderCloud.Integrations.Portal.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.EnvironmentSeed.Commands
{
    public interface IEnvironmentSeedCommand
    {
        Task<EnvironmentSeedResponse> Seed(EnvironmentSeedRequest seed);

        Task UpdateTranslations(string connectionString, string containerName);

        Task PostStagingRestore();
    }

    public class EnvironmentSeedCommand : IEnvironmentSeedCommand
    {
        private readonly EnvironmentSettings environmentSettings;
        private readonly OrderCloudSettings orderCloudSettings;
        private readonly IPortalService portal;
        private readonly ISupplierCommand supplierCommand;
        private readonly IBuyerCommand buyerCommand;
        private readonly IHSBuyerLocationCommand buyerLocationCommand;
        private IOrderCloudClient oc;

        public EnvironmentSeedCommand(
            EnvironmentSettings environmentSettings,
            OrderCloudSettings orderCloudSettings,
            IPortalService portal,
            ISupplierCommand supplierCommand,
            IBuyerCommand buyerCommand,
            IHSBuyerLocationCommand buyerLocationCommand,
            IOrderCloudClient oc)
        {
            this.portal = portal;
            this.supplierCommand = supplierCommand;
            this.buyerCommand = buyerCommand;
            this.buyerLocationCommand = buyerLocationCommand;
            this.oc = oc;
            this.environmentSettings = environmentSettings;
            this.orderCloudSettings = orderCloudSettings;
        }

        /// <summary>
        /// This seeding function can be used to initially seed an marketplace
        /// it is also meant to be safe to call after an marketplace has been seeded (by including seed.MarketplaceID)
        /// If a method starts with CreateOrUpdate it will update the resource every time its called based on what has been defined in SeedConstants.cs
        /// If a method starts with CreateOnlyOnce it will only create the resource once and then ignore thereafter
        /// The CreateOnlyOnce resources are likely to change after initial creation so we ignore to avoid overwriting desired changes that happen outside of seeding.
        /// </summary>
        public async Task<EnvironmentSeedResponse> Seed(EnvironmentSeedRequest seed)
        {
            try
            {
                if (seed.Marketplace.Environment == OrderCloudEnvironment.Production && seed.Marketplace.ID == null)
                {
                    return new EnvironmentSeedResponse
                    {
                        Comments = "Cannot create a production marketplace via the environment seed endpoint. Please contact an OrderCloud Developer to create a production marketplace.",
                        Success = false,
                    };
                }

                // lets us handle requests to multiple api environments
                var apiUrl = MarketplaceHelper.GetApiUrl(seed.Marketplace.Environment, seed.Marketplace.Region);
                oc = new OrderCloudClient(new OrderCloudClientConfig
                {
                    ApiUrl = apiUrl,
                    AuthUrl = apiUrl,
                });

                var portalUserToken = await portal.Login(seed.Portal.Username, seed.Portal.Password);
                var marketplace = await GetOrCreateMarketplace(portalUserToken, seed.Marketplace);
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
                    OrderCloudEnvironment = seed.Marketplace.Environment.ToString(),
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
            catch (CatalystBaseException ex)
            {
                return new EnvironmentSeedResponse
                {
                    Comments = $"Error! Environment seeding failed. The marketplace may be partially seeded with incomplete data. Please see the following exception message for details.\n{ex.Errors[0].Message}",
                    Success = false,
                };
            }
            catch (Exception ex)
            {
                return new EnvironmentSeedResponse
                {
                    Comments = $"Error! Environment seeding failed. The marketplace may be partially seeded with incomplete data. Please see the following exception message for details.\n{ex.Message}",
                    Success = false,
                };
            }
        }

        public async Task UpdateTranslations(string connectionString, string containerName)
        {
            var englishTranslationsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Assets", "english-translations.json"));
            var translationsConfig = new CloudBlobServiceConfig()
            {
                ConnectionString = connectionString,
                Container = containerName,
                AccessType = BlobContainerPublicAccessType.Container,
            };
            var translationsBlob = new CloudBlobService(translationsConfig);
            await translationsBlob.Save("i18n/en.json", File.ReadAllText(englishTranslationsPath));
        }

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

        public async Task<Marketplace> GetOrCreateMarketplace(string token, MarketplaceSettings marketplaceSettings)
        {
            if (marketplaceSettings.ID != null)
            {
                var existingMarketplace = await VerifyMarketplaceExists(marketplaceSettings.ID, token);
                return existingMarketplace;
            }
            else
            {
                var marketPlaceToCreate = ConstructMarketplaceFromSeed(marketplaceSettings);
                try
                {
                    await portal.GetMarketplace(marketPlaceToCreate.Id, token);
                    return await GetOrCreateMarketplace(token, marketplaceSettings);
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

        public async Task CreateOrUpdateXPIndices(string token)
        {
            var defaultIndicies = SeedData.XpIndices.DefaultIndices();
            foreach (var index in defaultIndicies)
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

        public async Task CreateOnlyOnceIncrementors(string token)
        {
            var defaultIncrementors = SeedData.Incrementors.DefaultIncrementors();
            foreach (var incrementor in defaultIncrementors)
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

        public async Task CreateOrUpdateSecurityProfiles(string accessToken)
        {
            var profiles = SeedData.SecurityProfiles.DefaultSecurityProfiles().Select(p =>
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
                Name = SeedData.Constants.FullAccessSecurityProfile,
                ID = SeedData.Constants.FullAccessSecurityProfile,
            });

            var profileCreateRequests = profiles.Select(p => oc.SecurityProfiles.SaveAsync(p.ID, p, accessToken));
            await Task.WhenAll(profileCreateRequests);
        }

        public async Task DeleteAllMessageSenders(string token)
        {
            var messageSenders = await oc.MessageSenders.ListAllAsync(accessToken: token);
            await Throttler.RunAsync(messageSenders, 500, 20, messageSender =>
                oc.MessageSenders.DeleteAsync(messageSender.ID, accessToken: token));
        }

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

        private static Marketplace ConstructMarketplaceFromSeed(MarketplaceSettings marketplaceSettings)
        {
            return new Marketplace()
            {
                Id = Guid.NewGuid().ToString(),
                Environment = marketplaceSettings.Environment.ToString(),
                Name = marketplaceSettings.Name ?? "My Headstart Marketplace",
                Region = MarketplaceHelper.GetRegion(marketplaceSettings.Region),
            };
        }

        private async Task CreateOnlyOnceApiClients(EnvironmentSeedRequest seed, string token)
        {
            var existingClients = await oc.ApiClients.ListAllAsync(accessToken: token);

            await CreateOrGetBuyerClient(existingClients, SeedData.ApiClients.BuyerClient(seed.Marketplace.EnableAnonymousShopping), seed, token);
            await CreateOrGetApiClient(existingClients, SeedData.ApiClients.IntegrationsClient(), token);
            await CreateOrGetApiClient(existingClients, SeedData.ApiClients.SellerClient(), token);
            await CreateOrGetApiClient(existingClients, SeedData.ApiClients.BuyerLocalClient(seed.Marketplace.EnableAnonymousShopping), token);
        }

        private async Task CreateOrUpdateSecurityProfileAssignments(EnvironmentSeedRequest seed, string marketplaceToken)
        {
            // assign buyer security profiles
            var buyerSecurityProfileAssignmentRequests = seed.Marketplace.Buyers.Select(b =>
            {
                return oc.SecurityProfiles.SaveAssignmentAsync(
                    new SecurityProfileAssignment
                    {
                        BuyerID = b.ID,
                        SecurityProfileID = CustomRole.HSBaseBuyer.ToString(),
                    }, marketplaceToken);
            });
            await Task.WhenAll(buyerSecurityProfileAssignmentRequests);

            // assign seller security profiles to seller marketplace
            var sellerSecurityProfileAssignmentRequests = SeedData.CustomRoles.SellerHsRoles().Select(role =>
            {
                return oc.SecurityProfiles.SaveAssignmentAsync(
                    new SecurityProfileAssignment()
                    {
                        SecurityProfileID = role.ToString(),
                    }, marketplaceToken);
            });
            await Task.WhenAll(sellerSecurityProfileAssignmentRequests);

            // assign full access security profile to default admin user
            var adminUsersList = await oc.AdminUsers.ListAsync(filters: new { Username = SeedData.Constants.SellerUserName }, accessToken: marketplaceToken);
            var defaultAdminUser = adminUsersList.Items.FirstOrDefault();
            if (defaultAdminUser == null)
            {
                throw new Exception($"Unable to find default admin user (username: {SeedData.Constants.SellerUserName}");
            }

            await oc.SecurityProfiles.SaveAssignmentAsync(
                new SecurityProfileAssignment()
                {
                    SecurityProfileID = SeedData.Constants.FullAccessSecurityProfile,
                    UserID = defaultAdminUser.ID,
                }, marketplaceToken);
        }

        private async Task CreateOnlyOnceBuyers(EnvironmentSeedRequest seed, string token)
        {
            // create default buyer if it does not exist
            // default buyer will have a well-known ID we can use to query with
            var defaultBuyer = await GetBuyerByID(SeedData.Constants.DefaultBuyerID, token);
            if (defaultBuyer == null)
            {
                var superBuyer = new SuperHSBuyer()
                {
                    Buyer = SeedData.Buyers.DefaultBuyer(),
                };
                await buyerCommand.Create(superBuyer, token, oc);
            }

            // create seed buyers if they don't exist
            // seed buyers may not have ID defined, we are relying on Name instead
            foreach (var buyer in seed.Marketplace.Buyers)
            {
                var seedBuyer = await GetBuyerByName(buyer.Name, token);
                if (seedBuyer == null)
                {
                    var superBuyer = new SuperHSBuyer()
                    {
                        Buyer = buyer,
                    };
                    await buyerCommand.Create(superBuyer, token, oc);
                }
            }
        }

        private async Task CreateOnlyOnceAnonBuyerConfig(EnvironmentSeedRequest seed, string token)
        {
            // validate AnonymousShoppingBuyerID or provide fallback if none is defined
            var allBuyers = await oc.Buyers.ListAllAsync(accessToken: token);
            if (!string.IsNullOrWhiteSpace(seed.Marketplace.AnonymousShoppingBuyerID))
            {
                if (!allBuyers.Select(b => b.ID).Contains(seed.Marketplace.AnonymousShoppingBuyerID))
                {
                    throw new Exception("The buyer defined by AnonymousShoppingBuyerID does not exist");
                }
            }
            else
            {
                seed.Marketplace.AnonymousShoppingBuyerID = SeedData.Constants.DefaultBuyerID;
            }

            // create and assign initial buyer location
            await buyerLocationCommand.Save(
                seed.Marketplace.AnonymousShoppingBuyerID,
                $"{seed.Marketplace.AnonymousShoppingBuyerID}-{SeedData.Constants.DefaultLocationID}",
                SeedData.BuyerLocations.DefaultBuyerLocation(),
                token,
                oc);

            // create user
            var anonBuyerUser = await oc.Users.SaveAsync(seed.Marketplace.AnonymousShoppingBuyerID, SeedData.Constants.AnonymousBuyerUserID, SeedData.Users.AnonymousBuyerUser(), token);

            // save assignment between user and buyergroup (location)
            var assignment = new UserGroupAssignment()
            {
                UserGroupID = $"{seed.Marketplace.AnonymousShoppingBuyerID}-{SeedData.Constants.DefaultLocationID}",
                UserID = anonBuyerUser.ID,
            };
            await oc.UserGroups.SaveUserAssignmentAsync(seed.Marketplace.AnonymousShoppingBuyerID, assignment, accessToken: token);
        }

        private async Task<HSBuyer> GetBuyerByName(string buyerName, string token)
        {
            var list = await oc.Buyers.ListAsync<HSBuyer>(filters: new { Name = buyerName }, accessToken: token);
            return list.Items.ToList().FirstOrDefault();
        }

        private async Task<HSBuyer> GetBuyerByID(string buyerID, string token)
        {
            var list = await oc.Buyers.ListAsync<HSBuyer>(filters: new { ID = buyerID }, accessToken: token);
            return list.Items.ToList().FirstOrDefault();
        }

        private async Task CreateOrUpdateSuppliers(EnvironmentSeedRequest seed, string token)
        {
            // Create Suppliers and necessary user groups and security profile assignments
            foreach (HSSupplier supplier in seed.Marketplace.Suppliers)
            {
                var exists = await SupplierExistsAsync(supplier.Name, token);
                if (!exists)
                {
                    await supplierCommand.Create(supplier, token, isSeedingEnvironment: true);
                }
            }
        }

        private async Task CreateOrUpdateProductFacets(string token)
        {
            var defaultFacet = SeedData.ProductFacets.DefaultProductFacet();
            await oc.ProductFacets.SaveAsync<HSProductFacet>(defaultFacet.ID, defaultFacet, token);
        }

        private async Task<bool> SupplierExistsAsync(string supplierName, string token)
        {
            var list = await oc.Suppliers.ListAsync(filters: new { Name = supplierName }, accessToken: token);
            return list.Items.Any();
        }

        private async Task CreateOrUpdateDefaultSellerUser(EnvironmentSeedRequest seed, string token)
        {
            // the middleware api client will use this user as the default context user
            var middlewareIntegrationsUser = SeedData.Users.MiddlewareIntegrationsUser();
            await oc.AdminUsers.SaveAsync(middlewareIntegrationsUser.ID, middlewareIntegrationsUser, token);

            // used to log in immediately after seeding the marketplace
            var initialAdminUser = SeedData.Users.IntialAdminUser(seed.Marketplace.InitialAdmin.Username, seed.Marketplace.InitialAdmin.Password);
            await oc.AdminUsers.SaveAsync(initialAdminUser.ID, initialAdminUser, token);
        }

        private async Task<SeededApiClients> GetApiClients(string token)
        {
            var list = await oc.ApiClients.ListAllAsync<HSApiClient>(accessToken: token);
            var appNames = list.Select(x => x.AppName);
            var adminUIApiClient = list.First(a => a.AppName == SeedData.Constants.SellerApiClientName);
            var buyerUIApiClient = list.First(a => a?.xp?.IsStorefront == true);
            var buyerLocalUIApiClient = list.First(a => a.AppName == SeedData.Constants.BuyerLocalApiClientName);
            var middlewareApiClient = list.First(a => a.AppName == SeedData.Constants.IntegrationsApiClientName);
            return new SeededApiClients()
            {
                AdminUiApiClient = adminUIApiClient,
                BuyerUiApiClient = buyerUIApiClient,
                BuyerLocalUiApiClient = buyerLocalUIApiClient,
                MiddlewareApiClient = middlewareApiClient,
            };
        }

        private async Task<string[]> GetStoreFrontClientIDs(string token)
        {
            var list = await oc.ApiClients.ListAllAsync<HSApiClient>(accessToken: token);
            return list
                .Where(client => client?.xp?.IsStorefront == true) // can't index ApiClients so we need to filter client-side
                .Select(client => client.ID).ToArray();
        }

        private async Task<ApiClient> CreateOrGetBuyerClient(List<ApiClient> existingClients, ApiClient client, EnvironmentSeedRequest seed, string token)
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

        private async Task CreateOrUpdateMessageSendersAndAssignments(EnvironmentSeedRequest seed, string accessToken)
        {
            var defaultMessageSenders = new List<MessageSender>()
            {
                SeedData.MessageSenders.BuyerEmails(seed.Marketplace.MiddlewareBaseUrl, seed.Marketplace.WebhookHashKey),
                SeedData.MessageSenders.SellerEmails(seed.Marketplace.MiddlewareBaseUrl, seed.Marketplace.WebhookHashKey),
                SeedData.MessageSenders.SuplierEmails(seed.Marketplace.MiddlewareBaseUrl, seed.Marketplace.WebhookHashKey),
            };
            var existingMessageSenders = await oc.MessageSenders.ListAllAsync(accessToken: accessToken);
            foreach (var sender in defaultMessageSenders)
            {
                var messageSender = await GetOrCreateMessageSender(existingMessageSenders, sender, accessToken);
                if (messageSender.ID == MessageSenderConstants.BuyerEmails)
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
                else if (messageSender.ID == MessageSenderConstants.SellerEmails)
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
                else if (messageSender.ID == MessageSenderConstants.SupplierEmails)
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

        private async Task<MessageSender> GetOrCreateMessageSender(List<MessageSender> existingMessageSenders, MessageSender messageSender, string accessToken)
        {
            var match = existingMessageSenders.Find(c => c.ID == messageSender.ID);
            if (match == null)
            {
                return await oc.MessageSenders.CreateAsync(messageSender, accessToken);
            }

            return match;
        }

        private async Task CreateOrUpdateAndAssignIntegrationEvents(string token, EnvironmentSeedRequest seed = null)
        {
            var storefrontApiClientIDs = await GetStoreFrontClientIDs(token);
            var apiClients = await GetApiClients(token);
            var localBuyerClientID = apiClients.BuyerLocalUiApiClient.ID;

            // this gets called by both the /seed command and the post-staging restore so we need to handle getting settings from two sources
            var middlewareBaseUrl = seed != null ? seed.Marketplace.MiddlewareBaseUrl : environmentSettings.MiddlewareBaseUrl;
            var webhookHashKey = seed != null ? seed.Marketplace.WebhookHashKey : orderCloudSettings.WebhookHashKey;
            var checkoutEvent = SeedData.IntegrationEvents.CheckoutEvent(middlewareBaseUrl, webhookHashKey);
            await oc.IntegrationEvents.SaveAsync(checkoutEvent.ID, checkoutEvent, token);
            var localCheckoutEvent = SeedData.IntegrationEvents.LocalCheckoutEvent(webhookHashKey);
            await oc.IntegrationEvents.SaveAsync(localCheckoutEvent.ID, localCheckoutEvent, token);

            await oc.ApiClients.PatchAsync(localBuyerClientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckoutLOCAL" }, token);
            await Throttler.RunAsync(storefrontApiClientIDs, 500, 20, clientID =>
                oc.ApiClients.PatchAsync(clientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckout" }, token));
        }

        private async Task ShutOffSupplierEmailsAsync(string token)
        {
            var allSuppliers = await oc.Suppliers.ListAllAsync(accessToken: token);
            await Throttler.RunAsync(allSuppliers, 500, 20, supplier =>
                oc.Suppliers.PatchAsync(supplier.ID, new PartialSupplier { xp = new { NotificationRcpts = new string[] { } } }, token));
        }
    }
}
