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
using ordercloud.integrations.exchangerates;
using Headstart.Common.Models;
using OrderCloud.Catalyst;

namespace Headstart.API.Commands
{
    public interface IEnvironmentSeedCommand
    {
        Task<EnvironmentSeedResponse> Seed(EnvironmentSeed seed);
        Task PostStagingRestore();
    }
    public class EnvironmentSeedCommand : IEnvironmentSeedCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly AppSettings _settings;
        private readonly IPortalService _portal;
        private readonly IHSSupplierCommand _supplierCommand;
        private readonly IHSBuyerCommand _buyerCommand;
        private readonly IHSBuyerLocationCommand _buyerLocationCommand;
        private readonly IExchangeRatesCommand _exhangeRates;
        private readonly IOrderCloudIntegrationsBlobService _translationsBlob;

        public EnvironmentSeedCommand(
            AppSettings settings,
            IPortalService portal,
            IHSSupplierCommand supplierCommand,
            IHSBuyerCommand buyerCommand,
            IHSBuyerLocationCommand buyerLocationCommand,
            IOrderCloudClient oc,
            IExchangeRatesCommand exhangeRates
        )
        {
            _settings = settings;
            _portal = portal;
            _supplierCommand = supplierCommand;
            _buyerCommand = buyerCommand;
            _buyerLocationCommand = buyerLocationCommand;
            _oc = oc;
            _exhangeRates = exhangeRates;
            var translationsConfig = new BlobServiceConfig()
            {
                ConnectionString = _settings.BlobSettings.ConnectionString,
                Container = _settings.BlobSettings.ContainerNameTranslations
            };
            _translationsBlob = new OrderCloudIntegrationsBlobService(translationsConfig);
        }

        public async Task<EnvironmentSeedResponse> Seed(EnvironmentSeed seed)
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

            await CreateDefaultSellerUsers(seed, orgToken);

            await CreateIncrementors(orgToken); // must be before CreateBuyers
            await CreateMessageSenders(seed, orgToken); // must be before CreateBuyers and CreateSuppliers

            await CreateSecurityProfiles(orgToken);
            await CreateBuyers(seed, orgToken);
            await CreateConfigureAnonBuyer(seed, orgToken);

            await CreateApiClients(orgToken);
            await AssignSecurityProfiles(seed, orgToken);

            var apiClients = await GetApiClients(orgToken);
            await CreateXPIndices(orgToken);
            await CreateAndAssignIntegrationEvents(new string[] { apiClients.BuyerUiApiClient.ID }, apiClients.BuyerLocalUiApiClient.ID, orgToken);
            await CreateSuppliers(seed, orgToken);

            // populates exchange rates into blob container name: settings.BlobSettings.ContainerNameExchangeRates or "currency" if setting is not defined
            await _exhangeRates.Update();

            // populate default english translations into blob container name: settings.BlobSettings.ContainerNameTranslations or "ngx-translate" if setting is not defined
            // provide other language files to support multiple languages
            var currentDirectory = Directory.GetCurrentDirectory();
            var englishTranslationsPath = Path.GetFullPath(Path.Combine(currentDirectory, @"..\Headstart.Common\Assets\english-translations.json"));
            await _translationsBlob.Save("i18n/en.json", File.ReadAllText(englishTranslationsPath));

            return new EnvironmentSeedResponse
            {
                Comments = "Success! Your environment is now seeded. The following clientIDs & secrets should be used to finalize the configuration of your application. The initial admin username and password can be used to sign into your admin application",
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

        public async Task VerifyOrgExists(string orgID, string devToken)
        {
            try
            {
                await _portal.GetOrganization(orgID, devToken);
            }
            catch
            {
                // the portal API no longer allows us to create a production organization outside of portal
                // though its possible to create on sandbox - for consistency sake we'll require its created before seeding
                throw new Exception("Failed to retrieve seller organization with SellerOrgID. The organization must exist before it can be seeded");
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
            var createIE = CreateAndAssignIntegrationEvents(storefrontClientIDs, apiClients.BuyerLocalUiApiClient.ID, token);
            var shutOffSupplierEmails = ShutOffSupplierEmailsAsync(token); // shut off email notifications for all suppliers

            await Task.WhenAll(createIE, shutOffSupplierEmails);
        }

        private async Task AssignSecurityProfiles(EnvironmentSeed seed, string orgToken)
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

        private async Task CreateBuyers(EnvironmentSeed seed, string token)
        {
            var defaultBuyer = SeedConstants.DefaultBuyer();
            seed.Buyers.Add(defaultBuyer);
            foreach (var buyer in seed.Buyers)
            {
                var superBuyer = new SuperHSBuyer()
                {
                    Buyer = buyer,
                    Markup = new BuyerMarkup() { Percent = 0 }
                };

                var exists = await BuyerExistsAsync(buyer.Name, token);
                if(exists == null || exists.Count == 0)
                {
                    var createdBuyer = await _buyerCommand.Create(superBuyer, token, isSeedingEnvironment: true);
                    if(createdBuyer.Buyer.Name == defaultBuyer.Name && seed.AnonymousShoppingBuyerID == null)
                    {
                        seed.AnonymousShoppingBuyerID = createdBuyer.Buyer.ID;
                    }
                } else
                {
                    seed.AnonymousShoppingBuyerID = exists.FirstOrDefault().ID;
                }
            }
        }

        private async Task CreateConfigureAnonBuyer(EnvironmentSeed seed, string token)
        {
            var anonBuyer = SeedConstants.AnonymousBuyerUser();
            var defaultBuyer = SeedConstants.DefaultBuyer();

            //create anonymous buyer user
            var createUser = _oc.Users.SaveAsync(seed.AnonymousShoppingBuyerID, anonBuyer.ID, anonBuyer, token);

            //create and assign initial buyer location
            var createBuyerLocation = _buyerLocationCommand.Save(seed.AnonymousShoppingBuyerID, 
                $"{seed.AnonymousShoppingBuyerID}-{SeedConstants.DefaultLocationID}", 
                SeedConstants.DefaultBuyerLocation(), token, true);

            var assignment = new UserGroupAssignment()
            {
                UserGroupID = $"{seed.AnonymousShoppingBuyerID}-{SeedConstants.DefaultLocationID}",
                UserID = anonBuyer.ID
            };
            var saveAssignment = _oc.UserGroups.SaveUserAssignmentAsync(seed.AnonymousShoppingBuyerID, assignment, token);
            await Task.WhenAll(createUser, createBuyerLocation, saveAssignment);
        }

        private async Task<List<Buyer>> BuyerExistsAsync(string buyerName, string token)
        {
            var list = await _oc.Buyers.ListAsync(filters: new { Name = buyerName }, accessToken: token);
            return list.Items.ToList();
        }

        private async Task CreateSuppliers(EnvironmentSeed seed, string token)
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

        private async Task CreateDefaultSellerUsers(EnvironmentSeed seed, string token)
        {
            // the middleware api client will use this user as the default context user
            var middlewareIntegrationsUser = SeedConstants.MIddlewareIntegrationsUser();

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

        public async Task CreateXPIndices(string token)
        {
            foreach (var index in SeedConstants.DefaultIndices)
            {
                //PutAsync is throwing id already exists error. Seems like it is trying to create.
                //That is why we are using try catch here
                //Bug in sdk?
                try
                {
                    await _oc.XpIndices.PutAsync(index, token);
                }
                catch (Exception ex) { }
            }
        }

        public async Task CreateIncrementors(string token)
        {
            foreach (var incrementor in SeedConstants.DefaultIncrementors)
            {
                await _oc.Incrementors.SaveAsync(incrementor.ID, incrementor, token);
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

        private async Task CreateApiClients(string token)
        {
            var existingClients = await _oc.ApiClients.ListAllAsync(accessToken: token);

            var integrationsClientRequest = GetClientRequest(existingClients, SeedConstants.IntegrationsClient(), token);
            var sellerClientRequest = GetClientRequest(existingClients, SeedConstants.SellerClient(), token);
            var buyerClientRequest = GetClientRequest(existingClients, SeedConstants.BuyerClient(), token);
            var buyerLocalClientRequest = GetClientRequest(existingClients, SeedConstants.BuyerLocalClient(), token);

            await Task.WhenAll(integrationsClientRequest, sellerClientRequest, buyerClientRequest, buyerLocalClientRequest);
        }

        private Task<ApiClient> GetClientRequest(List<ApiClient> existingClients, ApiClient client, string token)
        {
            var match = existingClients.Find(c => c.AppName == client.AppName);
            return match != null ? _oc.ApiClients.SaveAsync(match.ID, client, token) : _oc.ApiClients.CreateAsync(client, token);
        }

        private async Task CreateMessageSenders(EnvironmentSeed seed, string accessToken)
        {
            var defaultMessageSenders = new List<MessageSender>()
            {
                SeedConstants.BuyerEmails(_settings),
                SeedConstants.SellerEmails(_settings),
                SeedConstants.SuplierEmails(_settings)
            };
            foreach (var sender in defaultMessageSenders)
            {
                var messageSender = await _oc.MessageSenders.SaveAsync(sender.ID, sender, accessToken);
                if (messageSender.ID == "BuyerEmails")
                {
                    foreach (var buyer in seed.Buyers)
                    {
                        await _oc.MessageSenders.SaveAssignmentAsync(new MessageSenderAssignment
                        {
                            MessageSenderID = messageSender.ID,
                            BuyerID = buyer.ID
                        }, accessToken);
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
                    } catch(Exception ex) { }
                    
                }
                else if (messageSender.ID == "SupplierEmails")
                {
                    foreach (var supplier in seed.Suppliers)
                    {
                        await _oc.MessageSenders.SaveAssignmentAsync(new MessageSenderAssignment
                        {
                            MessageSenderID = messageSender.ID,
                            SupplierID = supplier.ID
                        }, accessToken);
                    }
                }
            }
        }

        private async Task CreateAndAssignIntegrationEvents(string[] buyerClientIDs, string localBuyerClientID, string token)
        {
            var checkoutEvent = SeedConstants.CheckoutEvent(_settings);
            await _oc.IntegrationEvents.SaveAsync(checkoutEvent.ID, checkoutEvent, token);
            var localCheckoutEvent = SeedConstants.LocalCheckoutEvent(_settings);
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

        public async Task CreateSecurityProfiles(string accessToken)
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
