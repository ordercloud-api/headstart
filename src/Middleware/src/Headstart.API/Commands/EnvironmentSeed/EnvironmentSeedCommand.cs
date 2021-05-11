using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Models;
using Headstart.Models.Misc;
using Headstart.Models.Headstart;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.IO;
using Headstart.Common.Services;
using Headstart.Common;
using OrderCloud.Catalyst;
using Headstart.Common.Services.Portal.Models;
using System.Net;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Headstart.API.Commands
{
    public interface IEnvironmentSeedCommand
    {
        Task<EnvironmentSeedResponse> Seed(EnvironmentSeed seed);
        Task PostStagingRestore();
    }

    public class EnvironmentSeedCommand : IEnvironmentSeedCommand
    {
        private IOrderCloudClient _oc;
        private readonly AppSettings _settings;
        private readonly IPortalService _portal;
        private readonly IHSSupplierCommand _supplierCommand;
        private readonly IHSBuyerCommand _buyerCommand;
        private readonly IHSBuyerLocationCommand _buyerLocationCommand;

        public EnvironmentSeedCommand(
            AppSettings settings,
            IPortalService portal,
            IHSSupplierCommand supplierCommand,
            IHSBuyerCommand buyerCommand,
            IHSBuyerLocationCommand buyerLocationCommand,
            IOrderCloudClient oc
        )
        {
            _portal = portal;
            _supplierCommand = supplierCommand;
            _buyerCommand = buyerCommand;
            _buyerLocationCommand = buyerLocationCommand;
            _oc = oc;
            _settings = settings;
        }

        /// <summary>
        /// This seeding function can be used to initially seed an organization
        /// it is also meant to be safe to call after an organization has been seeded (by including seed.SellerOrgID)
        /// If a method starts with CreateOrUpdate it will update the resource every time its called based on what has been defined in SeedConstants.cs
        /// If a method starts with CreateOnlyOnce it will only create the resource once and then ignore thereafter
        /// The CreateOnlyOnce resources are likely to change after initial creation so we ignore to avoid overwriting desired changes that happen outside of seeding
        /// </summary>
        public async Task<EnvironmentSeedResponse> Seed(EnvironmentSeed seed)
        {
            OcEnv requestedEnv = validateEnvironment(seed.OrderCloudSettings.Environment);

            if (requestedEnv.environmentName == OrderCloudEnvironments.Production.environmentName && seed.SellerOrgID == null)
            {
                throw new Exception("Cannot create a production environment via the environment seed endpoint. Please contact an OrderCloud Developer to create a production org.");
            }

            // lets us handle requests to multiple api environments
            _oc = new OrderCloudClient(new OrderCloudClientConfig
            {
                ApiUrl = requestedEnv.apiUrl,
                AuthUrl = requestedEnv.apiUrl
            });

            var portalUserToken = await _portal.Login(seed.PortalUsername, seed.PortalPassword);
            var sellerOrg = await GetOrCreateOrg(portalUserToken, requestedEnv.environmentName, seed.SellerOrgName, seed.SellerOrgID);
            var orgToken = await _portal.GetOrgToken(sellerOrg.Id, portalUserToken);

            await CreateOrUpdateDefaultSellerUser(seed, orgToken);

            await CreateOnlyOnceIncrementors(orgToken); // must be before CreateBuyers
            await CreateOrUpdateMessageSendersAndAssignments(seed, orgToken); // must be before CreateBuyers and CreateSuppliers

            await CreateOrUpdateSecurityProfiles(orgToken);
            await CreateOnlyOnceBuyers(seed, orgToken);
            await CreateOrUpdateAnonBuyerConfig(seed, orgToken);

            await CreateOnlyOnceApiClients(seed, orgToken);
            await CreateOrUpdateSecurityProfileAssignments(seed, orgToken);

            var apiClients = await GetApiClients(orgToken);
            await CreateOrUpdateXPIndices(orgToken);
            await CreateOrUpdateAndAssignIntegrationEvents(new string[] { apiClients.BuyerUiApiClient.ID }, apiClients.BuyerLocalUiApiClient.ID, orgToken, seed);
            await CreateOrUpdateSuppliers(seed, orgToken);

            // populate default english translations into blob container name: settings.BlobSettings.ContainerNameTranslations or "ngx-translate" if setting is not defined
            // provide other language files to support multiple languages

            var englishTranslationsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Assets", "english-translations.json"));
            if (seed?.BlobSettings?.ConnectionString != null && seed?.BlobSettings?.ContainerNameTranslations != null)
            {
                var translationsConfig = new BlobServiceConfig()
                {
                    ConnectionString = seed.BlobSettings.ConnectionString,
                    Container = seed.BlobSettings.ContainerNameTranslations,
                    AccessType = BlobContainerPublicAccessType.Container
                };
                var translationsBlob = new OrderCloudIntegrationsBlobService(translationsConfig);
                await translationsBlob.Save("i18n/en.json", File.ReadAllText(englishTranslationsPath));
            }

            return new EnvironmentSeedResponse
            {
                Comments = "Success! Your environment is now seeded. The following clientIDs & secrets should be used to finalize the configuration of your application. The initial admin username and password can be used to sign into your admin application",
                OrganizationName = sellerOrg.Name,
                OrganizationID = sellerOrg.Id,
                OrderCloudEnvironment = requestedEnv.environmentName,
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

        private OcEnv validateEnvironment(string environment)
        {
            if (environment.ToLower() == "production")
            {
                return OrderCloudEnvironments.Production;
            }
            else if (environment.ToLower() == "sandbox")
            {
                return OrderCloudEnvironments.Sandbox;
            }
            else return null;
        }
        public async Task<Organization> VerifyOrgExists(string orgID, string devToken)
        {
            try
            {
                return await _portal.GetOrganization(orgID, devToken);
            }
            catch
            {
                // the portal API no longer allows us to create a production organization outside of portal
                // though its possible to create on sandbox - for consistency sake we'll require its created before seeding
                throw new Exception("Failed to retrieve seller organization with SellerOrgID. The organization must exist before it can be seeded");
            }
        }

        public async Task<Organization> GetOrCreateOrg(string token, string env, string orgName, string orgID = null)
        {
            if (orgID != null)
            {
                var org = await VerifyOrgExists(orgID, token);
                return org;
            }
            else
            {
                var org = new Organization()
                {
                    Id = Guid.NewGuid().ToString(),
                    Environment = env,
                    Name = orgName == null ? "My Headstart Organization" : orgName
                };
                try
                {
                    await _portal.GetOrganization(org.Id, token);
                    return await GetOrCreateOrg(token, env, orgName, orgID);
                }
                catch (Exception ex)
                {
                    await _portal.CreateOrganization(org, token);
                    return await _portal.GetOrganization(org.Id, token);
                }
            }
        }

        /// <summary>
        /// The staging environment gets restored weekly from production
        /// during that restore things like webhooks, message senders, and integration events are shut off (so you don't for example email production customers)
        /// this process restores integration events which are required for checkout (with environment specific settings)
        public async Task PostStagingRestore()
        {
            var token = (await _oc.AuthenticateAsync()).AccessToken;
            var apiClients = await GetApiClients(token);
            var storefrontClientIDs = await GetStoreFrontClientIDs(token);

            var deleteIE = DeleteAllIntegrationEvents(token);
            await Task.WhenAll(deleteIE);

            // recreate with environment specific data
            var createIE = CreateOrUpdateAndAssignIntegrationEvents(storefrontClientIDs, apiClients.BuyerLocalUiApiClient.ID, token);
            var shutOffSupplierEmails = ShutOffSupplierEmailsAsync(token); // shut off email notifications for all suppliers

            await Task.WhenAll(createIE, shutOffSupplierEmails);
        }

        private async Task CreateOrUpdateSecurityProfileAssignments(EnvironmentSeed seed, string orgToken)
        {
            // assign buyer security profiles
            var buyerSecurityProfileAssignmentRequests = seed.Buyers.Select(b =>
            {
                return _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment
                {
                    BuyerID = b.ID,
                    SecurityProfileID = CustomRole.HSBaseBuyer.ToString()
                }, orgToken);
            });
            await Task.WhenAll(buyerSecurityProfileAssignmentRequests);

            // assign seller security profiles to seller org
            var sellerSecurityProfileAssignmentRequests = SeedConstants.SellerHsRoles.Select(role =>
            {
                return _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
                {
                    SecurityProfileID = role.ToString()
                }, orgToken);
            });
            await Task.WhenAll(sellerSecurityProfileAssignmentRequests);

            // assign full access security profile to default admin user
            var defaultAdminUser = (await _oc.AdminUsers.ListAsync(accessToken: orgToken)).Items.First(u => u.Username == SeedConstants.SellerUserName);
            await _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
            {
                SecurityProfileID = SeedConstants.FullAccessSecurityProfile,
                UserID = defaultAdminUser.ID
            }, orgToken);
        }

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
                    var superBuyer = new SuperHSBuyer()
                    {
                        Buyer = buyer,
                        Markup = new BuyerMarkup() { Percent = 0 }
                    };
                    await _buyerCommand.Create(superBuyer, token, _oc);
                }
            }
        }

        private async Task CreateOrUpdateAnonBuyerConfig(EnvironmentSeed seed, string token)
        {
            if(seed.EnableAnonymousShopping == false)
            {
                var clientsWithAnonShopping = await _oc.ApiClients.ListAllAsync(filters: "IsAnonBuyer=true", accessToken: token);
                if(clientsWithAnonShopping.Any())
                {
                    var requests = clientsWithAnonShopping.Select(apiClient => _oc.ApiClients.PatchAsync(apiClient.ID, new PartialApiClient { IsAnonBuyer = false }, accessToken: token));
                    await Task.WhenAll(requests);
                }
                return;
            }

            var allBuyers = await _oc.Buyers.ListAllAsync(accessToken: token);

            // validate AnonymousShoppingBuyerID or provide fallback if none is defined
            if (seed.AnonymousShoppingBuyerID != null)
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

            var anonBuyerUser = SeedConstants.AnonymousBuyerUser();

            //create and assign initial buyer location
            await _buyerLocationCommand.Save(seed.AnonymousShoppingBuyerID,
                $"{seed.AnonymousShoppingBuyerID}-{SeedConstants.DefaultLocationID}",
                SeedConstants.DefaultBuyerLocation(), token, _oc);

            // create user, transfer to another buyer org if needed
            try
            {
                await _oc.Users.SaveAsync(seed.AnonymousShoppingBuyerID, anonBuyerUser.ID, anonBuyerUser, token);
            } catch(OrderCloudException ex)
            {
                if (ex.HttpStatus == HttpStatusCode.Conflict && ex.Errors.Length > 0 && ex.Errors.First()?.ErrorCode == "User.UsernameMustBeUnique")
                {
                    // user already exists in a different buyer org
                    // we'll need to move them from previous buyer org into the new org
                    // there isn't a way of determining which buyer they're in without scanning all buyers
                    var fromBuyerID = await FindBuyerUserExistsIn(allBuyers, anonBuyerUser.Username, token);
                    var toBuyerID = seed.AnonymousShoppingBuyerID;
                    await _oc.Users.MoveAsync(fromBuyerID, anonBuyerUser.ID, toBuyerID, UserOrderMoveOption.None, token);
                } else
                {
                    throw;
                }
            }

            // save assignment between user and buyergroup (location)
            var assignment = new UserGroupAssignment()
            {
                UserGroupID = $"{seed.AnonymousShoppingBuyerID}-{SeedConstants.DefaultLocationID}",
                UserID = anonBuyerUser.ID
            };
            await _oc.UserGroups.SaveUserAssignmentAsync(seed.AnonymousShoppingBuyerID, assignment, accessToken: token);
        }

        private async Task<string> FindBuyerUserExistsIn(List<Buyer> allBuyers, string username, string token)
        {
            foreach(var buyer in allBuyers)
            {
                var list = await _oc.Users.ListAsync(buyer.ID, filters: $"Username={username}", accessToken: token);
                if (list.Items.Any())
                {
                    return buyer.ID;
                }
            }
            return null;
        }

        private async Task<HSBuyer> GetBuyerByName(string buyerName, string token)
        {
            var list = await _oc.Buyers.ListAsync<HSBuyer>(filters: new { Name = buyerName }, accessToken: token);
            return list.Items.ToList().FirstOrDefault();
        }

        private async Task<HSBuyer> GetBuyerByID(string buyerID, string token)
        {
            var list = await _oc.Buyers.ListAsync<HSBuyer>(filters: new { ID = buyerID }, accessToken: token);
            return list.Items.ToList().FirstOrDefault();
        }

        private async Task CreateOrUpdateSuppliers(EnvironmentSeed seed, string token)
        {
            // Create Suppliers and necessary user groups and security profile assignments
            foreach (HSSupplier supplier in seed.Suppliers)
            {
                var exists = await SupplierExistsAsync(supplier.Name, token);
                if (!exists)
                {
                    await _supplierCommand.Create(supplier, token, isSeedingEnvironment: true);
                }
            }
        }

        private async Task<bool> SupplierExistsAsync(string supplierName, string token)
        {
            var list = await _oc.Suppliers.ListAsync(filters: new { Name = supplierName }, accessToken: token);
            return list.Items.Any();
        }

        private async Task CreateOrUpdateDefaultSellerUser(EnvironmentSeed seed, string token)
        {
            // the middleware api client will use this user as the default context user
            var middlewareIntegrationsUser = SeedConstants.MiddlewareIntegrationsUser();

            await _oc.AdminUsers.SaveAsync(middlewareIntegrationsUser.ID, middlewareIntegrationsUser, token);

            // used to log in immediately after seeding the organization
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

        public async Task CreateOrUpdateXPIndices(string token)
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
                        throw ex;
                    }
                }
            }
        }

        public async Task CreateOnlyOnceIncrementors(string token)
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

        private async Task<ApiClientIDs> GetApiClients(string token)
        {
            var list = await _oc.ApiClients.ListAllAsync(accessToken: token);
            var appNames = list.Select(x => x.AppName);
            var adminUIApiClient = list.First(a => a.AppName == SeedConstants.SellerApiClientName);
            var buyerUIApiClient = list.First(a => a.AppName == SeedConstants.BuyerApiClientName);
            var buyerLocalUIApiClient = list.First(a => a.AppName == SeedConstants.BuyerLocalApiClientName);
            var middlewareApiClient = list.First(a => a.AppName == SeedConstants.IntegrationsApiClientName);
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
            // API clients don't have an xp that we can conveniently use to differentiate other api clients from 
            // storefront api clients so we're using a naming convention whereby all API clients that start with "Storefront - " are storefront api clients
            var list = await _oc.ApiClients.ListAllAsync(filters: new { AppName = "Storefront - *" }, accessToken: token);
            return list.Select(client => client.ID).ToArray();
        }

        public class ApiClientIDs
        {
            public ApiClient AdminUiApiClient { get; set; }
            public ApiClient BuyerUiApiClient { get; set; }
            public ApiClient BuyerLocalUiApiClient { get; set; }
            public ApiClient MiddlewareApiClient { get; set; }
        }

        private async Task CreateOnlyOnceApiClients(EnvironmentSeed seed, string token)
        {
            var existingClients = await _oc.ApiClients.ListAllAsync(accessToken: token);

            var integrationsClientRequest = CreateOrGetApiClient(existingClients, SeedConstants.IntegrationsClient(), token);
            var sellerClientRequest = CreateOrGetApiClient(existingClients, SeedConstants.SellerClient(), token);
            var buyerClientRequest = CreateOrGetApiClient(existingClients, SeedConstants.BuyerClient(seed), token);
            var buyerLocalClientRequest = CreateOrGetApiClient(existingClients, SeedConstants.BuyerLocalClient(seed), token);

            await Task.WhenAll(integrationsClientRequest, sellerClientRequest, buyerClientRequest, buyerLocalClientRequest);
        }

        private async Task<ApiClient> CreateOrGetApiClient(List<ApiClient> existingClients, ApiClient client, string token)
        {
            var match = existingClients.Find(c => c.AppName == client.AppName);
            if (match == null)
            {
                return await _oc.ApiClients.CreateAsync(client, token);
            }
            return match;
        }

        private async Task CreateOrUpdateMessageSendersAndAssignments(EnvironmentSeed seed, string accessToken)
        {
            var defaultMessageSenders = new List<MessageSender>()
            {
                SeedConstants.BuyerEmails(seed),
                SeedConstants.SellerEmails(seed),
                SeedConstants.SuplierEmails(seed)
            };
            var existingMessageSenders = await _oc.MessageSenders.ListAllAsync(accessToken: accessToken);
            foreach (var sender in defaultMessageSenders)
            {
                var messageSender = await GetOrCreateMessageSender(existingMessageSenders, sender, accessToken);
                if (messageSender.ID == "BuyerEmails")
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
                                throw ex;
                            }
                        }
                    }
                }
                else if (messageSender.ID == "SellerEmails")
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
                            throw ex;
                        }
                    }
                }
                else if (messageSender.ID == "SupplierEmails")
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
                return await _oc.MessageSenders.CreateAsync(messageSender, accessToken);
            }
            return match;
        }

        private async Task CreateOrUpdateAndAssignIntegrationEvents(string[] buyerClientIDs, string localBuyerClientID, string token, EnvironmentSeed seed = null)
        {
            // this gets called by both the /seed command and the post-staging restore so we need to handle getting settings from two sources
            var middlewareBaseUrl = seed != null ? seed.MiddlewareBaseUrl : _settings.EnvironmentSettings.MiddlewareBaseUrl;
            var webhookHashKey = seed != null ? seed.OrderCloudSettings.WebhookHashKey : _settings.OrderCloudSettings.WebhookHashKey;
            var checkoutEvent = SeedConstants.CheckoutEvent(middlewareBaseUrl, webhookHashKey);
            await _oc.IntegrationEvents.SaveAsync(checkoutEvent.ID, checkoutEvent, token);
            var localCheckoutEvent = SeedConstants.LocalCheckoutEvent(webhookHashKey);
            await _oc.IntegrationEvents.SaveAsync(localCheckoutEvent.ID, localCheckoutEvent, token);

            await _oc.ApiClients.PatchAsync(localBuyerClientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckoutLOCAL" }, token);
            await Throttler.RunAsync(buyerClientIDs, 500, 20, clientID =>
                _oc.ApiClients.PatchAsync(clientID, new PartialApiClient { OrderCheckoutIntegrationEventID = "HeadStartCheckout" }, token));
        }

        private async Task ShutOffSupplierEmailsAsync(string token)
        {
            var allSuppliers = await _oc.Suppliers.ListAllAsync(accessToken: token);
            await Throttler.RunAsync(allSuppliers, 500, 20, supplier =>
                _oc.Suppliers.PatchAsync(supplier.ID, new PartialSupplier { xp = new { NotificationRcpts = new string[] { } } }, token));
        }

        public async Task CreateOrUpdateSecurityProfiles(string accessToken)
        {
            var profiles = SeedConstants.DefaultSecurityProfiles.Select(p =>
                new SecurityProfile()
                {
                    Name = p.ID.ToString(),
                    ID = p.ID.ToString(),
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

        public async Task DeleteAllMessageSenders(string token)
        {
            var messageSenders = await _oc.MessageSenders.ListAllAsync(accessToken: token);
            await Throttler.RunAsync(messageSenders, 500, 20, messageSender =>
                _oc.MessageSenders.DeleteAsync(messageSender.ID, accessToken: token));
        }

        public async Task DeleteAllIntegrationEvents(string token)
        {
            // can't delete integration event if its referenced by an api client so first patch it to null
            var apiClientsWithIntegrationEvent = await _oc.IntegrationEvents.ListAllAsync(filters: new { OrderCheckoutIntegrationEventID = "*" }, accessToken: token);
            await Throttler.RunAsync(apiClientsWithIntegrationEvent, 500, 20, apiClient =>
                _oc.ApiClients.PatchAsync(apiClient.ID, new PartialApiClient { OrderCheckoutIntegrationEventID = null }, accessToken: token));

            var integrationEvents = await _oc.IntegrationEvents.ListAllAsync(accessToken: token);
            await Throttler.RunAsync(integrationEvents, 500, 20, integrationEvent =>
                _oc.IntegrationEvents.DeleteAsync(integrationEvent.ID, accessToken: token));
        }
    }
}
