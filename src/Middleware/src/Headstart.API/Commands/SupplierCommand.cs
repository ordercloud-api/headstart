using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Headstart.API.Helpers;
using Headstart.Common.Constants;
using Headstart.Models;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.API.Commands
{
    public interface IHSSupplierCommand
    {
        Task<HSSupplier> Create(HSSupplier supplier, string accessToken, bool isSeedingEnvironment = false);

        Task<HSSupplier> GetMySupplier(string supplierID, DecodedToken decodedToken);

        Task<HSSupplier> UpdateSupplier(string supplierID, PartialSupplier supplier, DecodedToken decodedToken);

        Task<HSSupplierOrderData> GetSupplierOrderData(string supplierOrderID, OrderType orderType, DecodedToken decodedToken);
    }

    public class HSSupplierCommand : IHSSupplierCommand
    {
        private readonly IOrderCloudClient oc;
        private readonly ISupplierSyncCommand supplierSync;
        private readonly AppSettings settings;
        private readonly ISupplierApiClientHelper apiClientHelper;

        public HSSupplierCommand(AppSettings settings, IOrderCloudClient oc, ISupplierApiClientHelper apiClientHelper, ISupplierSyncCommand supplierSync)
        {
            this.settings = settings;
            this.oc = oc;
            this.apiClientHelper = apiClientHelper;
            this.supplierSync = supplierSync;
        }

        public async Task<HSSupplier> GetMySupplier(string supplierID, DecodedToken decodedToken)
        {
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            Require.That(
                supplierID == me.Supplier.ID,
                new ErrorCode("Unauthorized", $"You are only authorized to view {me.Supplier.ID}.", HttpStatusCode.Unauthorized));
            return await oc.Suppliers.GetAsync<HSSupplier>(supplierID);
        }

        public async Task<HSSupplier> UpdateSupplier(string supplierID, PartialSupplier supplier, DecodedToken decodedToken)
        {
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            Require.That(decodedToken.CommerceRole == CommerceRole.Seller || supplierID == me.Supplier.ID, new ErrorCode("Unauthorized", $"You are not authorized to update supplier {supplierID}", HttpStatusCode.Unauthorized));
            var currentSupplier = await oc.Suppliers.GetAsync<HSSupplier>(supplierID);
            var updatedSupplier = await oc.Suppliers.PatchAsync<HSSupplier>(supplierID, supplier);

            // Update supplier products only on a name change
            if (currentSupplier.Name != supplier.Name || currentSupplier.xp.Currency.ToString() != supplier.xp.Currency.Value)
            {
                var productsToUpdate = await oc.Products.ListAllAsync<HSProduct>(
                supplierID: supplierID,
                accessToken: decodedToken.AccessToken);
                ApiClient supplierClient = await apiClientHelper.GetSupplierApiClient(supplierID, decodedToken.AccessToken);
                if (supplierClient == null)
                {
                    throw new Exception($"Default supplier client not found. SupplierID: {supplierID}");
                }

                var configToUse = new OrderCloudClientConfig
                {
                    ApiUrl = decodedToken.ApiUrl,
                    AuthUrl = decodedToken.AuthUrl,
                    ClientId = supplierClient.ID,
                    ClientSecret = supplierClient.ClientSecret,
                    GrantType = GrantType.ClientCredentials,
                    Roles = new[]
                        {
                            ApiRole.SupplierAdmin,
                            ApiRole.ProductAdmin,
                        },
                };
                var ocClient = new OrderCloudClient(configToUse);
                await ocClient.AuthenticateAsync();
                var token = ocClient.TokenResponse.AccessToken;
                foreach (var product in productsToUpdate)
                {
                    product.xp.Facets["supplier"] = new List<string>() { supplier.Name };
                    product.xp.Currency = supplier.xp.Currency;
                }

                await Throttler.RunAsync(productsToUpdate, 100, 5, product => ocClient.Products.SaveAsync(product.ID, product, accessToken: token));
            }

            return updatedSupplier;
        }

        public async Task<HSSupplier> Create(HSSupplier supplier, string accessToken, bool isSeedingEnvironment = false)
        {
            var token = isSeedingEnvironment ? accessToken : null;

            // Create Supplier
            supplier.ID = "{supplierIncrementor}";
            var ocSupplier = await oc.Suppliers.CreateAsync(supplier, token);
            supplier.ID = ocSupplier.ID;
            var ocSupplierID = ocSupplier.ID;

            // This supplier user is created so that we can define an api client with it as the default context user
            // this allows us to perform elevated supplier actions on behalf of that supplier company
            // It is not an actual user that will login so there is no password or valid email
            var supplierUser = await oc.SupplierUsers.CreateAsync(
                ocSupplierID,
                new User()
                {
                    Active = true,
                    FirstName = "Integration",
                    LastName = "Developer",
                    Username = $"dev_{ocSupplierID}",
                    Email = "test@test.com",
                },
                token);

            await CreateUserTypeUserGroupsAndSecurityProfileAssignments(supplierUser, token, ocSupplierID);

            // Create API Client for new supplier
            var apiClient = await oc.ApiClients.CreateAsync(
                new ApiClient()
                {
                    AppName = $"Integration Client {ocSupplier.Name}",
                    Active = true,
                    DefaultContextUserName = supplierUser.Username,
                    ClientSecret = settings.OrderCloudSettings.MiddlewareClientSecret,
                    AccessTokenDuration = 600,
                    RefreshTokenDuration = 43200,
                    AllowAnyBuyer = false,
                    AllowAnySupplier = false,
                    AllowSeller = false,
                    IsAnonBuyer = false,
                },
                token);

            // not adding api client ID on supplier Create because that approach would require creating the API client first
            // but creating supplier first is preferable in case there are error in the request
            ocSupplier = await oc.Suppliers.PatchAsync(
                ocSupplier.ID,
                new PartialSupplier()
                {
                    xp = new
                    {
                        ApiClientID = apiClient.ID,
                    },
                },
                token);

            // Assign Supplier API Client to new supplier
            await oc.ApiClients.SaveAssignmentAsync(
                new ApiClientAssignment()
                {
                    ApiClientID = apiClient.ID,
                    SupplierID = ocSupplierID,
                },
                token);

            // assign to message sender
            await oc.MessageSenders.SaveAssignmentAsync(new MessageSenderAssignment
            {
                MessageSenderID = "SupplierEmails",
                SupplierID = ocSupplierID,
            });
            return supplier;
        }

        public async Task CreateUserTypeUserGroupsAndSecurityProfileAssignments(User user, string token, string supplierID)
        {
            // Assign supplier to HSMeAdmin security profile
            await oc.SecurityProfiles.SaveAssignmentAsync(
                new SecurityProfileAssignment()
                {
                    SupplierID = supplierID,
                    SecurityProfileID = "HSMeAdmin",
                },
                token);

            foreach (var userType in HSUserTypes.Supplier())
            {
                var userGroupID = $"{supplierID}{userType.UserGroupIDSuffix}";

                await oc.SupplierUserGroups.CreateAsync(
                    supplierID,
                    new UserGroup()
                    {
                        ID = userGroupID,
                        Name = userType.UserGroupName,
                        xp =
                            {
                                Type = "UserPermissions",
                            },
                    },
                    token);

                await oc.SupplierUserGroups.SaveUserAssignmentAsync(
                    supplierID,
                    new UserGroupAssignment()
                    {
                        UserID = user.ID,
                        UserGroupID = userGroupID,
                    },
                    token);

                foreach (var customRole in userType.CustomRoles)
                {
                    await oc.SecurityProfiles.SaveAssignmentAsync(
                        new SecurityProfileAssignment()
                        {
                            SupplierID = supplierID,
                            UserGroupID = userGroupID,
                            SecurityProfileID = customRole.ToString(),
                        },
                        token);
                }
            }
        }

        public async Task<HSSupplierOrderData> GetSupplierOrderData(string supplierOrderID, OrderType orderType, DecodedToken decodedToken)
        {
            var orderData = await supplierSync.GetOrderAsync(supplierOrderID, orderType, decodedToken);
            return (HSSupplierOrderData)orderData.ToObject(typeof(HSSupplierOrderData));
        }
    }
}
