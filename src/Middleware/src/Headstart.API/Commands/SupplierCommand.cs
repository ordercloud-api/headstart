using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Helpers;
using System.Collections.Generic;
using Headstart.Common.Constants;
using Headstart.Common.Models.Headstart;
using Headstart.API.Commands.SupplierSync;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface IHsSupplierCommand
	{
		Task<HsSupplier> Create(HsSupplier supplier, string accessToken, bool isSeedingEnvironment = false);
		Task<HsSupplier> GetMySupplier(string supplierID, DecodedToken decodedToken);
		Task<HsSupplier> UpdateSupplier(string supplierID, PartialSupplier supplier, DecodedToken decodedToken);
		Task<HsSupplierOrderData> GetSupplierOrderData(string supplierOrderID, OrderType orderType, DecodedToken decodedToken);
	}

	public class HsSupplierCommand : IHsSupplierCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly ISupplierSyncCommand _supplierSync;
		private readonly AppSettings _settings;
		private readonly ISupplierApiClientHelper _apiClientHelper;
		private readonly WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the HsSupplierCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="oc"></param>
		/// <param name="apiClientHelper"></param>
		/// <param name="supplierSync"></param>
		public HsSupplierCommand(AppSettings settings, IOrderCloudClient oc, ISupplierApiClientHelper apiClientHelper, ISupplierSyncCommand supplierSync)
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
		/// <param name="supplierId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HsSupplier response object from the GetMySupplier process</returns>
		public async Task<HsSupplier> GetMySupplier(string supplierId, DecodedToken decodedToken)
		{
			var resp = new HsSupplier();
			try
			{
				var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
				Require.That(supplierId == me.Supplier.ID, new ErrorCode($@"Unauthorized", $@"You are only authorized to view the {me.Supplier.ID}.", 401));
				resp = await _oc.Suppliers.GetAsync<HsSupplier>(supplierId);
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
		/// <param name="supplierId"></param>
		/// <param name="supplier"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HsSupplier response object from the UpdateSupplier process</returns>
		public async Task<HsSupplier> UpdateSupplier(string supplierId, PartialSupplier supplier, DecodedToken decodedToken)
		{
			var updatedSupplier = new HsSupplier();
			try
			{
				var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
				Require.That(decodedToken.CommerceRole == CommerceRole.Seller || supplierId == me.Supplier.ID, new ErrorCode("Unauthorized", $@"You are not authorized to update supplier {supplierId}.", 401));
				var currentSupplier = await _oc.Suppliers.GetAsync<HsSupplier>(supplierId);
				updatedSupplier = await _oc.Suppliers.PatchAsync<HsSupplier>(supplierId, supplier);
				// Update supplier products only on a name change
				if (currentSupplier.Name != supplier.Name || currentSupplier.xp.Currency.ToString() != supplier.xp.Currency.Value)
				{
					var productsToUpdate = await _oc.Products.ListAllAsync<HsProduct>(supplierID: supplierId, accessToken: decodedToken.AccessToken);
					ApiClient supplierClient = await _apiClientHelper.GetSupplierApiClient(supplierId, decodedToken.AccessToken);
					if (supplierClient == null) 
					{ 
						var ex = new Exception($@"The default supplier client with the SupplierID: {supplierId} was not found.");
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
		/// Public re-usable Create task method to created the HsSupplier object
		/// </summary>
		/// <param name="supplier"></param>
		/// <param name="accessToken"></param>
		/// <param name="isSeedingEnvironment"></param>
		/// <returns>The newly created HsSupplier response object</returns>
		public async Task<HsSupplier> Create(HsSupplier supplier, string accessToken, bool isSeedingEnvironment = false)
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
		/// <param name="supplierId"></param>
		/// <returns></returns>
		public async Task CreateUserTypeUserGroupsAndSecurityProfileAssignments(User user, string token, string supplierId)
		{
			try
			{
				// Assign supplier to HsMeAdmin security profile
				await _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
				{
					SupplierID = supplierId,
					SecurityProfileID = $@"HsMeAdmin"
				}, token);

				foreach (var userType in HsUserTypes.Supplier())
				{
					var userGroupID = $@"{supplierId}{userType.UserGroupIdSuffix}";

					await _oc.SupplierUserGroups.CreateAsync(supplierId, new UserGroup()
					{
						ID = userGroupID,
						Name = userType.UserGroupName,
						xp =
						{
							Type = $@"UserPermissions",
						}
					}, token);

					await _oc.SupplierUserGroups.SaveUserAssignmentAsync(supplierId, new UserGroupAssignment()
					{
						UserID = user.ID,
						UserGroupID = userGroupID
					}, token);

					foreach (var customRole in userType.CustomRoles.ToList())
					{
						await _oc.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
						{
							SupplierID = supplierId,
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
		/// <param name="supplierOrderId"></param>
		/// <param name="orderType"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HsSupplierOrderData response object from the GetSupplierOrderData process</returns>
		public async Task<HsSupplierOrderData> GetSupplierOrderData(string supplierOrderId, OrderType orderType, DecodedToken decodedToken)
		{
			var resp = new HsSupplierOrderData();
			try
			{
				var orderData = await _supplierSync.GetOrderAsync(supplierOrderId, orderType, decodedToken);
				resp = (HsSupplierOrderData)orderData.ToObject(typeof(HsSupplierOrderData));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}
	}
}