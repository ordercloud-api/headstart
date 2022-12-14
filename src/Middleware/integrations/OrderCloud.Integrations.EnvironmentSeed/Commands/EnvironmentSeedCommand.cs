using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;
using Headstart.Common.Commands;
using Headstart.Common.Constants;
using Headstart.Common.Models;
using Headstart.Common.Settings;
using OrderCloud.Catalyst;
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

        Task PostStagingRestore();
    }

    public class EnvironmentSeedCommand : IEnvironmentSeedCommand
    {
        private readonly EnvironmentSettings environmentSettings;
        private readonly OrderCloudSettings orderCloudSettings;
        private readonly IPortalService portal;
        private readonly IBuyerCommand buyerCommand;
        private readonly IHSBuyerLocationCommand buyerLocationCommand;
        private readonly IUploadTranslationsCommand uploadTranslationsCommand;
        private IOrderCloudClient oc;

        public EnvironmentSeedCommand(
            EnvironmentSettings environmentSettings,
            OrderCloudSettings orderCloudSettings,
            IPortalService portal,
            IBuyerCommand buyerCommand,
            IHSBuyerLocationCommand buyerLocationCommand,
            IUploadTranslationsCommand uploadTranslationsCommand,
            IOrderCloudClient oc)
            {
            this.portal = portal;
            this.buyerCommand = buyerCommand;
            this.buyerLocationCommand = buyerLocationCommand;
            this.oc = oc;
            this.environmentSettings = environmentSettings;
            this.orderCloudSettings = orderCloudSettings;
            this.uploadTranslationsCommand = uploadTranslationsCommand;
        }

        /// <summary>
        /// This seeding function can be used to initially seed an marketplace
        /// it is also meant to be safe to call after an marketplace has been seeded (by including seed.Marketplace.ID)
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
                await CreateOrUpdateAdminPermissionGroups(marketplaceToken);
                await CreateOrUpdateOrderReturnApprovalRule(marketplaceToken);
                await CreateOnlyOnceBuyers(seed, marketplaceToken);
                await CreateOnlyOnceApiClients(seed, marketplaceToken);
                await CreateOrUpdateSecurityProfileAssignments(marketplaceToken);
                await CreateOrUpdateXPIndices(marketplaceToken);
                await CreateOrUpdateAndAssignIntegrationEvents(marketplaceToken, seed);
                await CreateOrUpdateProductFacets(marketplaceToken);

                await uploadTranslationsCommand.UploadTranslationsFiles();

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
            catch (OrderCloudException ex)
            {
                // the amount of data contained in an exception is overwhelming to try to simply serialize (200k+ lines) so instead extrapolating the most salient
                var flurlException = ex.InnerException as FlurlHttpException;
                return new EnvironmentSeedResponse
                {
                    Comments = $"Error! Environment seeding failed. The marketplace may be partially seeded with incomplete data. Please see Exception for details.",
                    Exception = new
                    {
                        Message = ex.Message,
                        Errors = ex.Errors,
                        RequestMessage = flurlException.Message,
                        RequestBody = flurlException.Call.RequestBody,
                        RequestHeaders = flurlException.Call.Request.Headers,
                    },
                    Success = false,
                };
            }
            catch (Exception ex)
            {
                return new EnvironmentSeedResponse
                {
                    Comments = $"Error! Environment seeding failed. The marketplace may be partially seeded with incomplete data. Please see Exception for details.",
                    Exception = ex,
                    Success = false,
                };
            }
        }

        public async Task<Marketplace> VerifyMarketplaceExists(string marketplaceID, string devToken)
        {
            try
            {
                return await portal.GetMarketplace(marketplaceID, devToken);
            }
            catch (Exception e)
            {
                // Although its technically possible to use the Portal API to create a sandbox marketplace
                // It is not possible to do so with a production marketplace
                // for consistency sake we'll require the marketplace is created before seeding
                Console.WriteLine(e.Message);
                throw new Exception($"Failed to retrieve marketplace with ID '{marketplaceID}'. The marketplace must exist before it can be seeded");
            }
        }

        public async Task<Marketplace> GetOrCreateMarketplace(string token, MarketplaceSettings marketplaceSettings)
        {
            if (!string.IsNullOrWhiteSpace(marketplaceSettings.ID))
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

        public async Task CreateOrUpdateAdminPermissionGroups(string accessToken)
        {
            foreach (var userType in HSUserTypes.Admin())
            {
                var userGroupId = userType.UserGroupIDSuffix;
                await oc.AdminUserGroups.SaveAsync(
                    userGroupId,
                    new UserGroup()
                    {
                        ID = userGroupId,
                        Name = userType.UserGroupName,
                        Description = userType.Description,
                        xp = new
                        {
                            Type = "UserPermissions",
                        },
                    },
                    accessToken);

                foreach (var customRole in userType.CustomRoles)
                {
                    await oc.SecurityProfiles.SaveAssignmentAsync(
                        new SecurityProfileAssignment()
                        {
                            UserGroupID = userGroupId,
                            SecurityProfileID = customRole.ToString(),
                        },
                        accessToken);
                }
            }
        }

        public async Task CreateOrUpdateOrderReturnApprovalRule(string accessToken)
        {
            var userTypes = HSUserTypes.Admin();
            var returnApprovalType = userTypes.FirstOrDefault(type => type.CustomRoles.Contains(CustomRole.HSOrderReturnApprover));
            if (returnApprovalType == null)
            {
                throw new Exception("Missing user type definition for order returns");
            }

            var approvalRule = await oc.SellerApprovalRules.SaveAsync(
                returnApprovalType.UserGroupIDSuffix,
                new SellerApprovalRule
                {
                    ID = returnApprovalType.UserGroupIDSuffix,
                    ApprovalType = ApprovalType.OrderReturn,
                    ApprovingGroupID = returnApprovalType.UserGroupIDSuffix,
                    Description = "Admin users in this group will be able to approve order returns submitted by buyer users",
                    Name = "Order Return Approval",
                    RuleExpression = "true", // require all orders to be approved
                },
                accessToken);
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

        private async Task CreateOrUpdateSecurityProfileAssignments(string marketplaceToken)
        {
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
        }

        private async Task CreateOnlyOnceAnonBuyerConfig(string token)
        {
            // create and assign initial buyer location
            await buyerLocationCommand.Save(
                SeedData.Constants.DefaultBuyerID,
                $"{SeedData.Constants.DefaultBuyerID}-{SeedData.Constants.DefaultLocationID}",
                SeedData.BuyerLocations.DefaultBuyerLocation(),
                token,
                oc);

            // create user
            var anonBuyerUser = await oc.Users.SaveAsync(SeedData.Constants.DefaultBuyerID, SeedData.Constants.AnonymousBuyerUserID, SeedData.Users.AnonymousBuyerUser(), token);

            // save assignment between user and buyergroup (location)
            var assignment = new UserGroupAssignment()
            {
                UserGroupID = $"{SeedData.Constants.DefaultBuyerID}-{SeedData.Constants.DefaultLocationID}",
                UserID = anonBuyerUser.ID,
            };
            await oc.UserGroups.SaveUserAssignmentAsync(SeedData.Constants.DefaultBuyerID, assignment, accessToken: token);
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

        private async Task<User> CreateOrUpdateDefaultSellerUser(EnvironmentSeedRequest seed, string token)
        {
            // the middleware api client will use this user as the default context user
            var middlewareIntegrationsUser = SeedData.Users.MiddlewareIntegrationsUser();
            await oc.AdminUsers.SaveAsync(middlewareIntegrationsUser.ID, middlewareIntegrationsUser, token);

            // used to log in immediately after seeding the marketplace
            var initialAdminUser = SeedData.Users.IntialAdminUser(seed.Marketplace.InitialAdmin.Username, seed.Marketplace.InitialAdmin.Password);
            return await oc.AdminUsers.SaveAsync(initialAdminUser.ID, initialAdminUser, token);
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
                await CreateOnlyOnceAnonBuyerConfig(token);
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
            var checkoutIntegrationEvent = SeedData.IntegrationEvents.CheckoutEvent(middlewareBaseUrl, webhookHashKey);
            await oc.IntegrationEvents.SaveAsync(checkoutIntegrationEvent.ID, checkoutIntegrationEvent, token);
            var localCheckoutIntegrationEvent = SeedData.IntegrationEvents.LocalCheckoutEvent(webhookHashKey);
            await oc.IntegrationEvents.SaveAsync(localCheckoutIntegrationEvent.ID, localCheckoutIntegrationEvent, token);
            var returnIntegrationEvent = SeedData.IntegrationEvents.ReturnsEvent(middlewareBaseUrl, webhookHashKey);
            await oc.IntegrationEvents.SaveAsync(returnIntegrationEvent.ID, returnIntegrationEvent, token);

            await oc.ApiClients.PatchAsync(localBuyerClientID, new PartialApiClient { OrderCheckoutIntegrationEventID = localCheckoutIntegrationEvent.ID }, token);
            await Throttler.RunAsync(storefrontApiClientIDs, 500, 20, clientID =>
                oc.ApiClients.PatchAsync(
                        clientID,
                        new PartialApiClient
                        {
                            OrderCheckoutIntegrationEventID = checkoutIntegrationEvent.ID,
                            OrderReturnIntegrationEventID = returnIntegrationEvent.ID,
                        },
                        token));
        }

        private async Task ShutOffSupplierEmailsAsync(string token)
        {
            var allSuppliers = await oc.Suppliers.ListAllAsync(accessToken: token);
            await Throttler.RunAsync(allSuppliers, 500, 20, supplier =>
                oc.Suppliers.PatchAsync(supplier.ID, new PartialSupplier { xp = new { NotificationRcpts = new string[] { } } }, token));
        }
    }
}
