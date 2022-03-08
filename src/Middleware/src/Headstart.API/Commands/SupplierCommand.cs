using System;
using OrderCloud.SDK;
using Headstart.Models;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Helpers;
using Headstart.Models.Headstart;
using Headstart.Common.Constants;
using System.Collections.Generic;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

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
        private readonly IOrderCloudClient _oc;
        private readonly ISupplierSyncCommand _supplierSync;
        private readonly AppSettings _settings;
        private readonly ISupplierApiClientHelper _apiClientHelper;
        private readonly WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

        /// <summary>
        /// The IOC based constructor method for the HSSupplierCommand class object with Dependency Injection
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="oc"></param>
        /// <param name="apiClientHelper"></param>
        /// <param name="supplierSync"></param>
        public HSSupplierCommand(AppSettings settings, IOrderCloudClient oc, ISupplierApiClientHelper apiClientHelper, ISupplierSyncCommand supplierSync)
        {            
            try
            {
                _settings = settings;
                _oc = oc;
                _apiClientHelper = apiClientHelper;
                _supplierSync = supplierSync;
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Public re-usable GetMySupplier task method
        /// </summary>
        /// <param name="supplierID"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The HSSupplier response object from the GetMySupplier process</returns>
        public async Task<HSSupplier> GetMySupplier(string supplierID, DecodedToken decodedToken)
        {
            var resp = new HSSupplier();
            try
            {
                var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
                Require.That(supplierID == me.Supplier.ID, new ErrorCode($@"Unauthorized", $@"You are only authorized to view the {me.Supplier.ID}.", 401));
                resp = await _oc.Suppliers.GetAsync<HSSupplier>(supplierID);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable UpdateSupplier task method
        /// </summary>
        /// <param name="supplierID"></param>
        /// <param name="supplier"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The HSSupplier response object from the UpdateSupplier process</returns>
        public async Task<HSSupplier> UpdateSupplier(string supplierID, PartialSupplier supplier, DecodedToken decodedToken)
        {
            var updatedSupplier = new HSSupplier();
            try
            {
                var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
                Require.That(decodedToken.CommerceRole == CommerceRole.Seller || supplierID == me.Supplier.ID, new ErrorCode("Unauthorized", $"You are not authorized to update supplier {supplierID}", 401));
                var currentSupplier = await _oc.Suppliers.GetAsync<HSSupplier>(supplierID);
                updatedSupplier = await _oc.Suppliers.PatchAsync<HSSupplier>(supplierID, supplier);
                // Update supplier products only on a name change
                if (currentSupplier.Name != supplier.Name || currentSupplier.xp.Currency.ToString() != supplier.xp.Currency.Value)
                {
                    var productsToUpdate = await _oc.Products.ListAllAsync<HSProduct>(supplierID: supplierID, accessToken: decodedToken.AccessToken);
                    ApiClient supplierClient = await _apiClientHelper.GetSupplierApiClient(supplierID, decodedToken.AccessToken);
                    if (supplierClient == null) 
                    { 
                        var ex = new Exception($@"The default supplier client with the SupplierID: {supplierID} was not found.");
                        LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                        throw ex;
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
                            ApiRole.ProductAdmin
                        },
                    };

                    var ocClient = new OrderCloudClient(configToUse);
                    await ocClient.AuthenticateAsync();
                    var token = ocClient.TokenResponse.AccessToken;
                    foreach (var product in productsToUpdate)
                    {
                        product.xp.Facets[$@"supplier"] = new List<string>() { supplier.Name };
                        product.xp.Currency = supplier.xp.Currency;
                    }
                    await Throttler.RunAsync(productsToUpdate, 100, 5, product => ocClient.Products.SaveAsync(product.ID, product, accessToken: token));
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return updatedSupplier;

        }

        /// <summary>
        /// Public re-usable Create task method to created the HSSupplier object
        /// </summary>
        /// <param name="supplier"></param>
        /// <param name="accessToken"></param>
        /// <param name="isSeedingEnvironment"></param>
        /// <returns>The newly created HSSupplier response object</returns>
        public async Task<HSSupplier> Create(HSSupplier supplier, string accessToken, bool isSeedingEnvironment = false)
        {
            try
            {
                var token = isSeedingEnvironment ? accessToken : null;
                // Create Supplier
                supplier.ID = "{supplierIncrementor}";
                var ocSupplier = await _oc.Suppliers.CreateAsync(supplier, token);
                supplier.ID = ocSupplier.ID;
                var ocSupplierID = ocSupplier.ID;

                // This supplier user is created so that we can define an api client with it as the default context user
                // this allows us to perform elevated supplier actions on behalf of that supplier company
                // It is not an actual user that will login so there is no password or valid email
                var supplierUser = await _oc.SupplierUsers.CreateAsync(ocSupplierID, new User()
                {
                    Active = true,
                    FirstName = $@"Integration",
                    LastName = $@"Developer",
                    Username = $@"dev_{ocSupplierID}",
                    Email = $@"test@test.com"
                }, token);

                await CreateUserTypeUserGroupsAndSecurityProfileAssignments(supplierUser, token, ocSupplierID);
                // Create API Client for new supplier
                var apiClient = await _oc.ApiClients.CreateAsync(new ApiClient()
                {
                    AppName = $@"Integration Client {ocSupplier.Name}",
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
                // assign to message sender
                await _oc.MessageSenders.SaveAssignmentAsync(new MessageSenderAssignment
                {
                    MessageSenderID = $@"SupplierEmails",
                    SupplierID = ocSupplierID
                });
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return supplier;
        }

        /// <summary>
        /// Public re-usable CreateUserTypeUserGroupsAndSecurityProfileAssignments task method
        /// </summary>
        /// <param name="user"></param>
        /// <param name="token"></param>
        /// <param name="supplierID"></param>
        /// <returns></returns>
        public async Task CreateUserTypeUserGroupsAndSecurityProfileAssignments(User user, string token, string supplierID)
        {
            try
            {
                // Assign supplier to HSMeAdmin security profile
                await _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
                {
                    SupplierID = supplierID,
                    SecurityProfileID = $@"HSMeAdmin"
                }, token);

                foreach (var userType in HSUserTypes.Supplier())
                {
                    var userGroupID = $@"{supplierID}{userType.UserGroupIDSuffix}";

                    await _oc.SupplierUserGroups.CreateAsync(supplierID, new UserGroup()
                    {
                        ID = userGroupID,
                        Name = userType.UserGroupName,
                        xp =
                        {
                            Type = $@"UserPermissions",
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
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Public re-usable GetSupplierOrderData task method
        /// </summary>
        /// <param name="supplierOrderID"></param>
        /// <param name="orderType"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The HSSupplierOrderData response object from the GetSupplierOrderData process</returns>
        public async Task<HSSupplierOrderData> GetSupplierOrderData(string supplierOrderID, OrderType orderType, DecodedToken decodedToken)
        {
            var resp = new HSSupplierOrderData();
            try
            {
                var orderData = await _supplierSync.GetOrderAsync(supplierOrderID, orderType, decodedToken);
                resp = (HSSupplierOrderData)orderData.ToObject(typeof(HSSupplierOrderData));
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }
    }
}