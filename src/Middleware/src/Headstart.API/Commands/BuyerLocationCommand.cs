using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using Headstart.Common.Models.Headstart;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface IHsBuyerLocationCommand
	{
		Task<HsBuyerLocation> Get(string buyerId, string buyerLocationId);
		Task<HsBuyerLocation> Create(string buyerId, HsBuyerLocation buyerLocation);
		Task<HsBuyerLocation> Save(string buyerId, string buyerLocationId, HsBuyerLocation buyerLocation);
		Task<HsBuyerLocation> Save(string buyerId, string buyerLocationId, HsBuyerLocation buyerLocation, string token, IOrderCloudClient oc = null);
		Task Delete(string buyerId, string buyerLocationId);
		Task CreateSinglePermissionGroup(string buyerLocationId, string permissionGroupId);
		Task ReassignUserGroups(string buyerId, string newUserId);
	}

	public class HsBuyerLocationCommand : IHsBuyerLocationCommand
	{
		private IOrderCloudClient _oc;
		private readonly AppSettings _settings;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the HsBuyerLocationCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="oc"></param>
		public HsBuyerLocationCommand(AppSettings settings, IOrderCloudClient oc)
		{
			try
			{
				_settings = settings;
				_oc = oc;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable Get task method to get the HsBuyerLocation object by the buyerId and buyerLocationId
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <returns>The HsBuyerLocation response object by the buyerId and buyerLocationId</returns>
		public async Task<HsBuyerLocation> Get(string buyerId, string buyerLocationId)
		{
			var resp = new HsBuyerLocation();
			try
			{
				var buyerAddress = await _oc.Addresses.GetAsync<HsAddressBuyer>(buyerId, buyerLocationId);
				var buyerUserGroup = await _oc.UserGroups.GetAsync<HsLocationUserGroup>(buyerId, buyerLocationId);
				resp = new HsBuyerLocation 
				{
					Address = buyerAddress,
					UserGroup = buyerUserGroup
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable task method to create the HsBuyerLocation object for the buyerId
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocation"></param>
		/// <returns>The newly created HsBuyerLocation object by the buyerId</returns>
		public async Task<HsBuyerLocation> Create(string buyerId, HsBuyerLocation buyerLocation)
		{
			var resp = new HsBuyerLocation();
			try
			{
				resp = await Create(buyerId, buyerLocation, null, _oc);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable task method to create the HsBuyerLocation object for the buyerId
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocation"></param>
		/// <param name="token"></param>
		/// <param name="ocClient"></param>
		/// <returns>The newly created HsBuyerLocation object by the buyerId</returns>
		public async Task<HsBuyerLocation> Create(string buyerId, HsBuyerLocation buyerLocation, string token, IOrderCloudClient ocClient)
		{
			var resp = new HsBuyerLocation();
			try
			{
				var buyerLocationId = CreatebuyerLocationId(buyerId, buyerLocation.Address.ID);
				buyerLocation.Address.ID = buyerLocationId;
				var buyerAddress = await ocClient.Addresses.CreateAsync<HsAddressBuyer>(buyerId, buyerLocation.Address, accessToken: token);

				buyerLocation.UserGroup.ID = buyerAddress.ID;
				var buyerUserGroup = await ocClient.UserGroups.CreateAsync<HsLocationUserGroup>(buyerId, buyerLocation.UserGroup, accessToken: token);
				await CreateUserGroupAndAssignments(buyerId, buyerAddress.ID, token, ocClient);
				await CreateLocationUserGroupsAndApprovalRule(buyerAddress.ID, buyerAddress.AddressName, token, ocClient);
				resp = new HsBuyerLocation
				{
					Address = buyerAddress,
					UserGroup = buyerUserGroup,
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable CreatebuyerLocationId method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="idInRequest"></param>
		/// <returns>The idInRequest string response for the </returns>
		private string CreatebuyerLocationId(string buyerId, string idInRequest)
		{
			string resp = $@"{buyerId}-{idInRequest.Replace("-", "_")}";
			try
			{
				if (idInRequest.Contains("LocationIncrementor"))
				{
					// prevents prefix duplication with address validation prewebhooks
					resp = idInRequest;
				}
				if (idInRequest == null || idInRequest.Length == 0)
				{
					resp = $@"{buyerId}-[{buyerId}-LocationIncrementor]";
				}
				if (idInRequest.StartsWith($@"{buyerId}-"))
				{
					// prevents prefix duplication
					resp = idInRequest;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable CreateUserGroupAndAssignments task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <param name="token"></param>
		/// <param name="ocClient"></param>
		/// <returns></returns>
		public async Task CreateUserGroupAndAssignments(string buyerId, string buyerLocationId, string token, IOrderCloudClient ocClient)
		{
			try
			{
				var assignment = new AddressAssignment
				{
					AddressID = buyerLocationId,
					UserGroupID = buyerLocationId,
					IsBilling = true,
					IsShipping = true
				};
				await ocClient.Addresses.SaveAssignmentAsync(buyerId, assignment, accessToken: token);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable Save task method to update the HsBuyerLocation object data by the buyerId and buyerLocationId
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <param name="buyerLocation"></param>
		/// <returns>The newly updated HsBuyerLocation object by the buyerId and buyerLocationId</returns>
		public async Task<HsBuyerLocation> Save(string buyerId, string buyerLocationId, HsBuyerLocation buyerLocation)
		{
			var resp = new HsBuyerLocation();
			try
			{
				resp = await Save(buyerId, buyerLocationId, buyerLocation, null, _oc);
				// not being called by seed endpoint - use stored ordercloud client and stored admin token
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable Save task method to update the HsBuyerLocation object data by the buyerId and buyerLocationId
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <param name="buyerLocation"></param>
		/// <param name="token"></param>
		/// <param name="ocClient"></param>
		/// <returns>The newly updated HsBuyerLocation object by the buyerId and buyerLocationId</returns>
		public async Task<HsBuyerLocation> Save(string buyerId, string buyerLocationId, HsBuyerLocation buyerLocation, string token, IOrderCloudClient ocClient)
		{
			var location = new HsBuyerLocation();
			try
			{
				buyerLocation.Address.ID = buyerLocationId;
				buyerLocation.UserGroup.ID = buyerLocationId;
				UserGroup existingLocation = null;
				try
				{
					existingLocation = await ocClient.UserGroups.GetAsync(buyerId, buyerLocationId, token);
				}
				catch (Exception ex) 
				{
					LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
				}

				var updatedBuyerAddress = await ocClient.Addresses.SaveAsync<HsAddressBuyer>(buyerId, buyerLocationId, buyerLocation.Address, accessToken: token);
				var updatedBuyerUserGroup = await ocClient.UserGroups.SaveAsync<HsLocationUserGroup>(buyerId, buyerLocationId, buyerLocation.UserGroup, accessToken: token);
				location = new HsBuyerLocation
				{
					Address = updatedBuyerAddress,
					UserGroup = updatedBuyerUserGroup,
				};
				if (existingLocation == null)
				{
					var assignments = CreateUserGroupAndAssignments(buyerId, buyerLocationId, token, ocClient);
					var groups = CreateLocationUserGroupsAndApprovalRule(buyerLocationId, buyerLocation.Address.AddressName, token, ocClient);
					await Task.WhenAll(assignments, groups);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}            
			return location;
		}

		/// <summary>
		/// Public re-usable Delete task method to delete required Addresses and UserGroups for the specific buyerId and buyerLocationId
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <returns></returns>
		public async Task Delete(string buyerId, string buyerLocationId)
		{
			try
			{
				var deleteAddressReq = _oc.Addresses.DeleteAsync(buyerId, buyerLocationId);
				var deleteUserGroupReq = _oc.UserGroups.DeleteAsync(buyerId, buyerLocationId);
				await Task.WhenAll(deleteAddressReq, deleteUserGroupReq);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable CreateLocationUserGroupsAndApprovalRule task method
		/// </summary>
		/// <param name="buyerLocationId"></param>
		/// <param name="locationName"></param>
		/// <param name="accessToken"></param>
		/// <param name="ocClient"></param>
		/// <returns></returns>
		public async Task CreateLocationUserGroupsAndApprovalRule(string buyerLocationId, string locationName, string accessToken, IOrderCloudClient ocClient)
		{
			try
			{
				var buyerId = buyerLocationId.Split('-').First();
				var AddUserTypeRequests = HsUserTypes.BuyerLocation().Select(userType => AddUserTypeToLocation(buyerLocationId, userType, accessToken, ocClient));
				await Task.WhenAll(AddUserTypeRequests);
				var isSeedingEnvironment = ocClient != null;
				if (!isSeedingEnvironment)
				{
					var approvingGroupID = $"{buyerLocationId}-{UserGroupSuffix.OrderApprover}";
					await ocClient.ApprovalRules.CreateAsync(buyerId, new ApprovalRule()
					{
						ID = buyerLocationId,
						ApprovingGroupID = approvingGroupID,
						Description = "General Approval Rule for Location. Every Order Over a Certain Limit will Require Approval for the designated group of users.",
						Name = $@"{locationName} General Location Approval Rule.",
						RuleExpression = $@"order.xp.ApprovalNeeded = '{buyerLocationId}' & order.Total > 0"
					});
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable CreateSinglePermissionGroup task method
		/// </summary>
		/// <param name="buyerLocationId"></param>
		/// <param name="permissionGroupId"></param>
		/// <returns></returns>
		public async Task CreateSinglePermissionGroup(string buyerLocationId, string permissionGroupId)
		{
			try
			{
				var permissionGroup = HsUserTypes.BuyerLocation().Find(userType => permissionGroupId.Contains(userType.UserGroupIdSuffix));
				await AddUserTypeToLocation(buyerLocationId, permissionGroup);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable AddUserTypeToLocation task method
		/// </summary>
		/// <param name="buyerLocationId"></param>
		/// <param name="HsUserType"></param>
		/// <returns></returns>
		public async Task AddUserTypeToLocation(string buyerLocationId, HsUserType HsUserType)
		{
			try
			{
				// not being called by seed endpoint - use stored ordercloud client and stored admin token
				await AddUserTypeToLocation(buyerLocationId, HsUserType, null, _oc);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable AddUserTypeToLocation task method
		/// </summary>
		/// <param name="buyerLocationId"></param>
		/// <param name="HsUserType"></param>
		/// <param name="accessToken"></param>
		/// <param name="oc"></param>
		/// <returns></returns>
		public async Task AddUserTypeToLocation(string buyerLocationId, HsUserType HsUserType, string accessToken, IOrderCloudClient oc)
		{
			try
			{
				// if we're seeding then use the passed in oc client
				// to support multiple environments and ease of setup for new orgs
				// else used the configured client
				var token = oc == null ? null : accessToken;
				var ocClient = oc ?? _oc;

				var buyerId = buyerLocationId.Split('-').First();
				var userGroupID = $"{buyerLocationId}-{HsUserType.UserGroupIdSuffix}";
				await ocClient.UserGroups.CreateAsync(buyerId, new PartialUserGroup()
				{
					ID = userGroupID,
					Name = HsUserType.UserGroupName,
					xp = new
					{
						Role = HsUserType.UserGroupIdSuffix.ToString(),
						Type = HsUserType.UserGroupType,
						Location = buyerLocationId
					}
				}, token);
				foreach (var customRole in HsUserType.CustomRoles)
				{
					await ocClient.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
					{
						BuyerID = buyerId,
						UserGroupID = userGroupID,
						SecurityProfileID = customRole.ToString()
					}, token);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ReassignUserGroups task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="newUserId"></param>
		/// <returns></returns>
		public async Task ReassignUserGroups(string buyerId, string newUserId)
		{
			try
			{
				var userGroupAssignments = await _oc.UserGroups.ListAllUserAssignmentsAsync(buyerId, userID: newUserId);
				await Throttler.RunAsync(userGroupAssignments, 100, 5, assignment => RemoveAndAddUserGroupAssignment(buyerId, newUserId, assignment?.UserGroupID));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ReassignUserGroups task method
		/// Temporary work around for a platform issue. When a new user is registered we need to 
		/// delete and reassign usergroup assignments for that user to view products
		/// issue: https://four51.atlassian.net/browse/EX-2222
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="newUserId"></param>
		/// <param name="userGroupId"></param>
		/// <returns></returns>
		private async Task RemoveAndAddUserGroupAssignment(string buyerId, string newUserId, string userGroupId)
		{
			try
			{
				await _oc.UserGroups.DeleteUserAssignmentAsync(buyerId, userGroupId, newUserId);
				await _oc.UserGroups.SaveUserAssignmentAsync(buyerId, new UserGroupAssignment
				{
					UserGroupID = userGroupId,
					UserID = newUserId
				});
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}
	}
}