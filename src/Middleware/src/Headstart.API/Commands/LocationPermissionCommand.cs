using System;
using System.Net;
using System.Linq;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using System.Collections.Generic;
using Headstart.Common.Models.Misc;
using Headstart.Common.Models.Headstart;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface ILocationPermissionCommand
	{
		Task<List<UserGroupAssignment>> ListLocationPermissionAssignments(string buyerId, string locationId, DecodedToken decodedToken);
		Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAssignments(string buyerId, string locationId, DecodedToken decodedToken);
		Task<decimal> GetApprovalThreshold(string buyerId, string locationId, DecodedToken decodedToken);
		Task<ListPage<HsUser>> ListLocationUsers(string buyerId, string locationId, DecodedToken decodedToken);
		Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerId, string locationId, LocationPermissionUpdate locationPermissionUpdate, DecodedToken decodedToken);
		Task<bool> IsUserInAccessGroup(string locationId, string groupSuffix, DecodedToken decodedToken);
		Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentId, string userId, DecodedToken decodedToken);
		Task<ListPage<HsLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HsLocationUserGroup> args, string buyerId, string userId, DecodedToken decodedToken);
		Task<ListPage<HsLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HsLocationUserGroup> args, string buyerId, string userId, DecodedToken decodedToken);
		Task<ApprovalRule> SaveApprovalRule(string buyerId, string locationId, ApprovalRule approval, DecodedToken decodedToken);
		Task DeleteApprovalRule(string buyerId, string locationId, string approvalId, DecodedToken decodedToken);
	}

	public class LocationPermissionCommand : ILocationPermissionCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the LocationPermissionCommand class object with Dependency Injection
		/// </summary>
		/// <param name="oc"></param>
		public LocationPermissionCommand(IOrderCloudClient oc)
		{
			try
			{
				_oc = oc;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ListLocationPermissionAsssignments task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The list of UserGroupAssignment response objects from the ListLocationPermissionAssignments process</returns>
		public async Task<List<UserGroupAssignment>> ListLocationPermissionAssignments(string buyerId, string locationId, DecodedToken decodedToken)
		{
			var resp = new List<UserGroupAssignment>();
			try
			{
				await EnsureUserIsLocationAdmin(locationId, decodedToken);
				var locationUserTypes = HsUserTypes.BuyerLocation().Where(s => s.UserGroupIdSuffix != UserGroupSuffix.NeedsApproval.ToString() || s.UserGroupIdSuffix != UserGroupSuffix.OrderApprover.ToString());
				var userGroupAssignmentResponses = await Throttler.RunAsync(locationUserTypes, 100, 5, locationUserType => {
					return _oc.UserGroups.ListUserAssignmentsAsync(buyerId, userGroupID: $"{locationId}-{locationUserType.UserGroupIdSuffix}", pageSize: 100);
				});
				resp = userGroupAssignmentResponses.Select(userGroupAssignmentResponse => userGroupAssignmentResponse.Items).SelectMany(l => l).ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable GetApprovalThreshold task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The threshold decimal value from the GetApprovalThreshold process</returns>
		public async Task<decimal> GetApprovalThreshold(string buyerId, string locationId, DecodedToken decodedToken)
		{
			decimal threshold = 0;
			try
			{
				await EnsureUserIsLocationAdmin(locationId, decodedToken);
				var approvalRule = await _oc.ApprovalRules.GetAsync(buyerId, locationId);
				threshold = Convert.ToDecimal(approvalRule.RuleExpression.Split('>')[1]);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return threshold;
		}

		/// <summary>
		/// Public re-usable SaveApprovalRule task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="approval"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ApprovalRule response object from the SaveApprovalRule process</returns>
		public async Task<ApprovalRule> SaveApprovalRule(string buyerId, string locationId, ApprovalRule approval, DecodedToken decodedToken)
		{
			var resp = new ApprovalRule(); 
			try
			{
				await EnsureUserIsLocationAdmin(locationId, decodedToken);
				resp = await _oc.ApprovalRules.SaveAsync(buyerId, approval.ID, approval);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable DeleteApprovalRule task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="approvalId"></param>
		/// <param name="decodedToken"></param>
		/// <returns></returns>
		public async Task DeleteApprovalRule(string buyerId, string locationId, string approvalId, DecodedToken decodedToken)
		{
			try
			{
				await EnsureUserIsLocationAdmin(locationId, decodedToken);
				await _oc.ApprovalRules.DeleteAsync(buyerId, approvalId);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ListLocationApprovalPermissionAsssignments task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The list of UserGroupAssignment response objects from the ListLocationApprovalPermissionAssignments process</returns>
		public async Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAssignments(string buyerId, string locationId, DecodedToken decodedToken)
		{
			var resp = new List<UserGroupAssignment>();
			try
			{
				await EnsureUserIsLocationAdmin(locationId, decodedToken);
				var locationUserTypes = HsUserTypes.BuyerLocation().Where(s => s.UserGroupIdSuffix == UserGroupSuffix.NeedsApproval.ToString() || s.UserGroupIdSuffix == UserGroupSuffix.OrderApprover.ToString());
				var userGroupAssignmentResponses = await Throttler.RunAsync(locationUserTypes, 100, 5, locationUserType => {
					return _oc.UserGroups.ListUserAssignmentsAsync(buyerId, userGroupID: $@"{locationId}-{locationUserType.UserGroupIdSuffix}", pageSize: 100);
				});
				resp = userGroupAssignmentResponses.Select(userGroupAssignmentResponse => userGroupAssignmentResponse.Items).SelectMany(l => l).ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable UpdateLocationPermissions task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="locationPermissionUpdate"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The list of UserGroupAssignment response objects from the UpdateLocationPermissions process</returns>
		public async Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerId, string locationId, LocationPermissionUpdate locationPermissionUpdate, DecodedToken decodedToken)
		{
			var resp = new List<UserGroupAssignment>();
			try
			{
				await EnsureUserIsLocationAdmin(locationId, decodedToken);
				await Throttler.RunAsync(locationPermissionUpdate.AssignmentsToAdd, 100, 5, assignmentToAdd =>
				{
					return _oc.UserGroups.SaveUserAssignmentAsync(buyerId, assignmentToAdd);
				});
				await Throttler.RunAsync(locationPermissionUpdate.AssignmentsToDelete, 100, 5, assignmentToDelete =>
				{
					return _oc.UserGroups.DeleteUserAssignmentAsync(buyerId, assignmentToDelete.UserGroupID, assignmentToDelete.UserID);
				});
				resp = await ListLocationPermissionAssignments(buyerId, locationId, decodedToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ListLocationUsers task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPage of HsUser response objects from the ListLocationUsers process</returns>
		public async Task<ListPage<HsUser>> ListLocationUsers(string buyerId, string locationId, DecodedToken decodedToken)
		{
			var resp = new ListPage<HsUser>();
			try
			{
				await EnsureUserIsLocationAdmin(locationId, decodedToken);
				resp = await _oc.Users.ListAsync<HsUser>(buyerId, userGroupID: locationId);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable EnsureUserIsLocationAdmin task method
		/// </summary>
		/// <param name="locationId"></param>
		/// <param name="decodedToken"></param>
		/// <returns></returns>
		public async Task EnsureUserIsLocationAdmin(string locationId, DecodedToken decodedToken)
		{
			try
			{
				var hasAccess = await IsUserInAccessGroup(locationId, UserGroupSuffix.PermissionAdmin.ToString(), decodedToken);
				Require.That(hasAccess, new ErrorCode(@"Insufficient Access", $@"The User cannot manage permissions for: {locationId}.", HttpStatusCode.Forbidden));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ValidateCurrentQuantities task method
		/// </summary>
		/// <param name="locationId"></param>
		/// <param name="groupSuffix"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The boolean status for the IsUserInAccessGroup process</returns>
		public async Task<bool> IsUserInAccessGroup(string locationId, string groupSuffix, DecodedToken decodedToken)
		{
			var resp = false; 
			try
			{
				var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
				var buyerId = me.Buyer.ID;
				var userGroupId = $@"{locationId}-{groupSuffix}";
				resp = await IsUserInUserGroup(buyerId, userGroupId, decodedToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ListUserUserGroupAssignments task method
		/// </summary>
		/// <param name="userGroupType"></param>
		/// <param name="parentId"></param>
		/// <param name="userId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The list of UserGroupAssignment response objects from the ListLocationUsers process</returns>
		public async Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentId, string userId, DecodedToken decodedToken)
		{
			var userUserGroupAssignments = new List<UserGroupAssignment>();
			try
			{
				userUserGroupAssignments = await GetUserUserGroupAssignments(userGroupType, parentId, userId, decodedToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return userUserGroupAssignments;
		}

		/// <summary>
		/// Public re-usable ListUserGroupsByCountry task method
		/// </summary>
		/// <param name="args"></param>
		/// <param name="buyerId"></param>
		/// <param name="userId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPage of HsLocationUserGroup response objects from the ListUserGroupsByCountry process</returns>
		public async Task<ListPage<HsLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HsLocationUserGroup> args, string buyerId, string userId, DecodedToken decodedToken)
		{
			var userGroups = new ListPage<HsLocationUserGroup>();
			try
			{
				var user = await _oc.Users.GetAsync(buyerId, userId);
				var assigned = args.Filters.FirstOrDefault(f => f.PropertyName.Equals(@"assigned", StringComparison.OrdinalIgnoreCase))?.FilterExpression;
				if (!bool.Parse(assigned))
				{
					userGroups = await _oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerId, search: args.Search, filters: $@"xp.Country={user.xp.Country}&xp.Type=BuyerLocation",
						page: args.Page, pageSize: 100);
				}
				else
				{
					var userUserGroupAssignments = await GetUserUserGroupAssignments("BuyerLocation", buyerId, userId, decodedToken);
					var userBuyerLocationAssignments = new List<HsLocationUserGroup>();
					foreach (var assignment in userUserGroupAssignments)
					{
						//Buyer Location user groups are formatted as {buyerID}-{userID}.  This eliminates the unnecessary groups that end in "-{OrderApproval}", for example, helping performance.
						if (assignment.UserGroupID.Split('-').Length != 2)
						{
							continue;
						}

						var userGroupLocation = await _oc.UserGroups.GetAsync<HsLocationUserGroup>(buyerId, assignment.UserGroupID);
						if (args.Search == null || userGroupLocation.Name.ToLower().Contains(args.Search))
						{
							userBuyerLocationAssignments.Add(userGroupLocation);
						}
					}

					userGroups.Items = userBuyerLocationAssignments;
					userGroups.Meta = new ListPageMeta()
					{
						Page = 1,
						PageSize = 100
					};
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return userGroups;
		}

		/// <summary>
		/// Private re-usable IsUserInUserGroup task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="userGroupId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The boolean status for the IsUserInUserGroup process</returns>
		private async Task<bool> IsUserInUserGroup(string buyerId, string userGroupId, DecodedToken decodedToken)
		{
			var resp = false;
			try
			{
				var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
				var userGroupAssignmentForAccess = await _oc.UserGroups.ListUserAssignmentsAsync(buyerId, userGroupId, me.ID);
				resp = userGroupAssignmentForAccess.Items.Count > 0;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable GetUserUserGroupAssignments task method
		/// </summary>
		/// <param name="userGroupType"></param>
		/// <param name="parentId"></param>
		/// <param name="userId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The list of UserGroupAssignment response objects from the GetUserUserGroupAssignments process</returns>
		public async Task<List<UserGroupAssignment>> GetUserUserGroupAssignments(string userGroupType, string parentId, string userId, DecodedToken decodedToken)
		{
			var resp = new List<UserGroupAssignment>();
			try
			{
				if (userGroupType.Equals($@"UserPermissions", StringComparison.OrdinalIgnoreCase))
				{
					resp = await _oc.SupplierUserGroups.ListAllUserAssignmentsAsync(parentId, userID: userId, accessToken: decodedToken.AccessToken);
				}
				else
				{
					resp = await _oc.UserGroups.ListAllUserAssignmentsAsync(parentId, userID: userId, accessToken: decodedToken.AccessToken);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ListUserGroupsForNewUser task method
		/// </summary>
		/// <param name="args"></param>
		/// <param name="buyerId"></param>
		/// <param name="homeCountry"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPage of HsLocationUserGroup response objects from the ListUserGroupsForNewUser process</returns>
		public async Task<ListPage<HsLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HsLocationUserGroup> args, string buyerId, string homeCountry, DecodedToken decodedToken)
		{
			var userGroups = new ListPage<HsLocationUserGroup>();
			try
			{
				userGroups = await _oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerId, search: args.Search, filters: $@"xp.Country={homeCountry}&xp.Type=BuyerLocation",
					page: args.Page, pageSize: 100);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return userGroups;
		}
	}
}