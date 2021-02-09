using Headstart.Models.Headstart;
using OrderCloud.SDK;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using System.Linq;
using Headstart.Common.Constants;
using ordercloud.integrations.library.helpers;
using Headstart.Models;
using System;
using System.Collections.Generic;
using Headstart.Common.Helpers;
using Headstart.Common;

namespace Headstart.API.Commands
{
    public interface IHeadstartSupplierCommand
    {
        Task<HSSupplier> Create(HSSupplier supplier, VerifiedUserContext user, bool isSeedingEnvironment = false);
        Task<HSSupplier> GetMySupplier(string supplierID, VerifiedUserContext user);
        Task<HSSupplier> UpdateSupplier(string supplierID, PartialSupplier supplier, VerifiedUserContext user);
        Task<HSSupplierOrderData> GetSupplierOrderData(string supplierOrderID, VerifiedUserContext user);
    }
    public class HSSupplierCommand : IHeadstartSupplierCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly ISupplierSyncCommand _supplierSync;
        private readonly AppSettings _settings;
        private readonly ISupplierApiClientHelper _apiClientHelper;

        public HSSupplierCommand(AppSettings settings, IOrderCloudClient oc, ISupplierApiClientHelper apiClientHelper, ISupplierSyncCommand supplierSync)
        {
            _settings = settings;
            _oc = oc;
            _apiClientHelper = apiClientHelper;
            _supplierSync = supplierSync;
        }
        public async Task<HSSupplier> GetMySupplier(string supplierID, VerifiedUserContext user)
        {
            Require.That(supplierID == user.SupplierID,
                new ErrorCode("Unauthorized", 401, $"You are only authorized to view {user.SupplierID}."));
            return await _oc.Suppliers.GetAsync<HSSupplier>(supplierID);
        }

        public async Task<HSSupplier> UpdateSupplier(string supplierID, PartialSupplier supplier, VerifiedUserContext user)
        {
            Require.That(user.UsrType == "admin" || supplierID == user.SupplierID, new ErrorCode("Unauthorized", 401, $"You are not authorized to update supplier {supplierID}"));
            var currentSupplier = await _oc.Suppliers.GetAsync<HSSupplier>(supplierID);
            var updatedSupplier = await _oc.Suppliers.PatchAsync<HSSupplier>(supplierID, supplier);
            // Update supplier products only on a name change
            if (currentSupplier.Name != supplier.Name || currentSupplier.xp.Currency.ToString() != supplier.xp.Currency.Value)
            {
                var productsToUpdate = await ListAllAsync.ListWithFacets((page) => _oc.Products.ListAsync<HSProduct>(
                supplierID: supplierID,
                page: page,
                pageSize: 100,
                accessToken: user.AccessToken
                ));
                ApiClient supplierClient = await _apiClientHelper.GetSupplierApiClient(supplierID, user.AccessToken);
                if (supplierClient == null) { throw new Exception($"Default supplier client not found. SupplierID: {supplierID}"); }
                var configToUse = new OrderCloudClientConfig
                {
                    ApiUrl = user.ApiUrl,
                    AuthUrl = user.AuthUrl,
                    ClientId = supplierClient.ID,
                    ClientSecret = supplierClient.ClientSecret,
                    GrantType = GrantType.ClientCredentials,
                    Roles = new[]
                               {
                                 ApiRole.SupplierAdmin,
                                 ApiRole.ProductAdmin
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
        public async Task<HSSupplier> Create(HSSupplier supplier, VerifiedUserContext user, bool isSeedingEnvironment = false)
        {
            var token = isSeedingEnvironment ? user.AccessToken : null;

            // Create Supplier
            supplier.ID = "{supplierIncrementor}";
            var ocSupplier = await _oc.Suppliers.CreateAsync(supplier, token);
            supplier.ID = ocSupplier.ID;
            var ocSupplierID = ocSupplier.ID;
     
            // Create Integrations Supplier User
            var supplierUser = await _oc.SupplierUsers.CreateAsync(ocSupplierID, new User()
            {
                Active = true,
                Email = user.Email,
                FirstName = "Integration",
                LastName = "Developer",
                Password = "Four51Yet!", // _settings.OrderCloudSettings.DefaultPassword,
                Username = $"dev_{ocSupplierID}"
            }, token);

            await CreateUserTypeUserGroupsAndSecurityProfileAssignments(supplierUser, token, ocSupplierID);

            // Create API Client for new supplier
            var apiClient = await _oc.ApiClients.CreateAsync(new ApiClient()
            {
                AppName = $"Integration Client {ocSupplier.Name}",
                Active = true,
                DefaultContextUserName = supplierUser.Username,
                ClientSecret = _settings.OrderCloudSettings.MiddlewareClientSecret,
                AccessTokenDuration = 600,
                RefreshTokenDuration = 43200,
                AllowAnyBuyer = false,
                AllowAnySupplier = false,
                AllowSeller = false,
                IsAnonBuyer = false,
            }, token);


            // not adding api client ID on supplier Create because that approach would require creating the API client first
            // but creating supplier first is preferable in case there are error in the request
            ocSupplier = await _oc.Suppliers.PatchAsync(ocSupplier.ID, new PartialSupplier()
            {
                xp = new
                {
                    ApiClientID = apiClient.ID
                }
            }, token);

            // Assign Supplier API Client to new supplier
            await _oc.ApiClients.SaveAssignmentAsync(new ApiClientAssignment()
            {
                ApiClientID = apiClient.ID,
                SupplierID = ocSupplierID
            }, token);
            // list message senders
            var msList = await _oc.MessageSenders.ListAsync(accessToken: token);
            // create message sender assignment
            var assignmentList = msList.Items.Select(ms =>
            {
                return new MessageSenderAssignment
                {
                    MessageSenderID = ms.ID,
                    SupplierID = ocSupplierID
                };
            });
            await Throttler.RunAsync(assignmentList, 100, 5, a => _oc.MessageSenders.SaveAssignmentAsync(a, token));
            return supplier;
        }
    
        public async Task CreateUserTypeUserGroupsAndSecurityProfileAssignments(User user, string token, string supplierID)
        {
            // Assign supplier to MPMeAdmin security profile
            await _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
            {
                SupplierID = supplierID,
                SecurityProfileID = "MPMeAdmin"
            }, token);

            foreach(var userType in HSUserTypes.Supplier())
            {
                var userGroupID = $"{supplierID}{userType.UserGroupIDSuffix}";

                await _oc.SupplierUserGroups.CreateAsync(supplierID, new UserGroup()
                {
                    ID = userGroupID,
                    Name = userType.UserGroupName,
                    xp =
                        {
                            Type = "UserPermissions",
                        }
                }, token);
 
                await _oc.SupplierUserGroups.SaveUserAssignmentAsync(supplierID, new UserGroupAssignment()
                {
                    UserID = user.ID,
                    UserGroupID = userGroupID
                }, token);

                foreach (var customRole in userType.CustomRoles)
                {
                    await _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
                    {
                        SupplierID = supplierID,
                        UserGroupID = userGroupID,
                        SecurityProfileID = customRole.ToString()
                    }, token);
                }
            }
        }

        public async Task<HSSupplierOrderData> GetSupplierOrderData(string supplierOrderID, VerifiedUserContext user)
        {
            var orderData = await _supplierSync.GetOrderAsync(supplierOrderID, user);
            return (HSSupplierOrderData)orderData.ToObject(typeof(HSSupplierOrderData));
        }
    }
}
