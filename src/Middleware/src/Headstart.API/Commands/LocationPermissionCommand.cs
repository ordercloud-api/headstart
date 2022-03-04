using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using Headstart.Models.Misc;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using System.Collections.Generic;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extenstions;

namespace Headstart.API.Commands
{
    public interface ILocationPermissionCommand
    {
        Task<List<UserGroupAssignment>> ListLocationPermissionAsssignments(string buyerID, string locationID, DecodedToken decodedToken);
        Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerID, string locationID, DecodedToken decodedToken);
        Task<decimal> GetApprovalThreshold(string buyerID, string locationID, DecodedToken decodedToken);
        Task<ListPage<HSUser>> ListLocationUsers(string buyerID, string locationID, DecodedToken decodedToken);
        Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerID, string locationID, LocationPermissionUpdate locationPermissionUpdate, DecodedToken decodedToken);
        Task<bool> IsUserInAccessGroup(string locationID, string groupSuffix, DecodedToken decodedToken);
        Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentID, string userID, DecodedToken decodedToken);
        Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerID, string userID, DecodedToken decodedToken);
        Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerID, string userID, DecodedToken decodedToken);
        Task<ApprovalRule> SaveApprovalRule(string buyerID, string locationID, ApprovalRule approval, DecodedToken decodedToken);
        Task DeleteApprovalRule(string buyerID, string locationID, string approvalID, DecodedToken decodedToken);
    }

    public class LocationPermissionCommand : ILocationPermissionCommand
    {
        private readonly IOrderCloudClient _oc;
        private WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

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
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Public re-usable ListLocationPermissionAsssignments task method
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="locationID"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The list of UserGroupAssignment response objects from the ListLocationPermissionAsssignments process</returns>
        public async Task<List<UserGroupAssignment>> ListLocationPermissionAsssignments(string buyerID, string locationID, DecodedToken decodedToken)
        {
            var resp = new List<UserGroupAssignment>();
            try
            {
                await EnsureUserIsLocationAdmin(locationID, decodedToken);
                var locationUserTypes = HSUserTypes.BuyerLocation().Where(s => s.UserGroupIDSuffix != UserGroupSuffix.NeedsApproval.ToString() || s.UserGroupIDSuffix != UserGroupSuffix.OrderApprover.ToString());
                var userGroupAssignmentResponses = await Throttler.RunAsync(locationUserTypes, 100, 5, locationUserType => {
                    return _oc.UserGroups.ListUserAssignmentsAsync(buyerID, userGroupID: $"{locationID}-{locationUserType.UserGroupIDSuffix}", pageSize: 100);
                });
                resp = userGroupAssignmentResponses.Select(userGroupAssignmentResponse => userGroupAssignmentResponse.Items).SelectMany(l => l).ToList();
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable GetApprovalThreshold task method
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="locationID"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The threshold decimal value from the GetApprovalThreshold process</returns>
        public async Task<decimal> GetApprovalThreshold(string buyerID, string locationID, DecodedToken decodedToken)
        {
            decimal threshold = 0;
            try
            {
                await EnsureUserIsLocationAdmin(locationID, decodedToken);
                var approvalRule = await _oc.ApprovalRules.GetAsync(buyerID, locationID);
                threshold = Convert.ToDecimal(approvalRule.RuleExpression.Split('>')[1]);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return threshold;
        }

        /// <summary>
        /// Public re-usable SaveApprovalRule task method
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="locationID"></param>
        /// <param name="approval"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The ApprovalRule response object from the SaveApprovalRule process</returns>
        public async Task<ApprovalRule> SaveApprovalRule(string buyerID, string locationID, ApprovalRule approval, DecodedToken decodedToken)
        {
            var resp = new ApprovalRule(); 
            try
            {
                await EnsureUserIsLocationAdmin(locationID, decodedToken);
                resp = await _oc.ApprovalRules.SaveAsync(buyerID, approval.ID, approval);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable DeleteApprovalRule task method
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="locationID"></param>
        /// <param name="approvalID"></param>
        /// <param name="decodedToken"></param>
        /// <returns></returns>
        public async Task DeleteApprovalRule(string buyerID, string locationID, string approvalID, DecodedToken decodedToken)
        {
            try
            {
                await EnsureUserIsLocationAdmin(locationID, decodedToken);
                await _oc.ApprovalRules.DeleteAsync(buyerID, approvalID);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Public re-usable ListLocationApprovalPermissionAsssignments task method
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="locationID"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The list of UserGroupAssignment response objects from the ListLocationApprovalPermissionAsssignments process</returns>
        public async Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerID, string locationID, DecodedToken decodedToken)
        {
            var resp = new List<UserGroupAssignment>();
            try
            {
                await EnsureUserIsLocationAdmin(locationID, decodedToken);
                var locationUserTypes = HSUserTypes.BuyerLocation().Where(s => s.UserGroupIDSuffix == UserGroupSuffix.NeedsApproval.ToString() || s.UserGroupIDSuffix == UserGroupSuffix.OrderApprover.ToString());
                var userGroupAssignmentResponses = await Throttler.RunAsync(locationUserTypes, 100, 5, locationUserType => {
                    return _oc.UserGroups.ListUserAssignmentsAsync(buyerID, userGroupID: $"{locationID}-{locationUserType.UserGroupIDSuffix}", pageSize: 100);
                });
                resp = userGroupAssignmentResponses.Select(userGroupAssignmentResponse => userGroupAssignmentResponse.Items).SelectMany(l => l).ToList();
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable UpdateLocationPermissions task method
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="locationID"></param>
        /// <param name="locationPermissionUpdate"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The list of UserGroupAssignment response objects from the UpdateLocationPermissions process</returns>
        public async Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerID, string locationID, LocationPermissionUpdate locationPermissionUpdate, DecodedToken decodedToken)
        {
            var resp = new List<UserGroupAssignment>();
            try
            {
                await EnsureUserIsLocationAdmin(locationID, decodedToken);
                await Throttler.RunAsync(locationPermissionUpdate.AssignmentsToAdd, 100, 5, assignmentToAdd =>
                {
                    return _oc.UserGroups.SaveUserAssignmentAsync(buyerID, assignmentToAdd);
                });
                await Throttler.RunAsync(locationPermissionUpdate.AssignmentsToDelete, 100, 5, assignmentToDelete =>
                {
                    return _oc.UserGroups.DeleteUserAssignmentAsync(buyerID, assignmentToDelete.UserGroupID, assignmentToDelete.UserID);
                });
                resp = await ListLocationPermissionAsssignments(buyerID, locationID, decodedToken);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable ListLocationUsers task method
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="locationID"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The ListPage of HSUser response objects from the ListLocationUsers process</returns>
        public async Task<ListPage<HSUser>> ListLocationUsers(string buyerID, string locationID, DecodedToken decodedToken)
        {
            var resp = new ListPage<HSUser>();
            try
            {
                await EnsureUserIsLocationAdmin(locationID, decodedToken);
                resp = await _oc.Users.ListAsync<HSUser>(buyerID, userGroupID: locationID);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable EnsureUserIsLocationAdmin task method
        /// </summary>
        /// <param name="locationID"></param>
        /// <param name="decodedToken"></param>
        /// <returns></returns>
        public async Task EnsureUserIsLocationAdmin(string locationID, DecodedToken decodedToken)
        {
            try
            {
                var hasAccess = await IsUserInAccessGroup(locationID, UserGroupSuffix.PermissionAdmin.ToString(), decodedToken);
                Require.That(hasAccess, new ErrorCode($@"Insufficient Access", $@"The User cannot manage permissions for: {locationID}.", 403));
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Public re-usable ValidateCurrentQuantities task method
        /// </summary>
        /// <param name="locationID"></param>
        /// <param name="groupSuffix"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The boolean status for the IsUserInAccessGroup process</returns>
        public async Task<bool> IsUserInAccessGroup(string locationID, string groupSuffix, DecodedToken decodedToken)
        {
            var resp = false; 
            try
            {
                var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
                var buyerID = me.Buyer.ID;
                var userGroupID = $@"{locationID}-{groupSuffix}";
                resp = await IsUserInUserGroup(buyerID, userGroupID, decodedToken);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable ListUserUserGroupAssignments task method
        /// </summary>
        /// <param name="userGroupType"></param>
        /// <param name="parentID"></param>
        /// <param name="userID"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The list of UserGroupAssignment response objects from the ListLocationUsers process</returns>
        public async Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentID, string userID, DecodedToken decodedToken)
        {
            var userUserGroupAssignments = new List<UserGroupAssignment>();
            try
            {
                userUserGroupAssignments = await GetUserUserGroupAssignments(userGroupType, parentID, userID, decodedToken);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return userUserGroupAssignments;
        }

        /// <summary>
        /// Public re-usable ListUserGroupsByCountry task method
        /// </summary>
        /// <param name="args"></param>
        /// <param name="buyerID"></param>
        /// <param name="userID"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The ListPage of HSLocationUserGroup response objects from the ListUserGroupsByCountry process</returns>
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerID, string userID, DecodedToken decodedToken)
        {
            var userGroups = new ListPage<HSLocationUserGroup>();
            try
            {
                var user = await _oc.Users.GetAsync(buyerID, userID);
                var assigned = args.Filters.FirstOrDefault(f => f.PropertyName == "assigned").FilterExpression;
                if (!bool.Parse(assigned))
                {
                    userGroups = await _oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID, search: args.Search, filters: $"xp.Country={user.xp.Country}&xp.Type=BuyerLocation",
                        page: args.Page, pageSize: 100);
                }
                else
                {
                    var userUserGroupAssignments = await GetUserUserGroupAssignments("BuyerLocation", buyerID, userID, decodedToken);
                    var userBuyerLocationAssignments = new List<HSLocationUserGroup>();
                    foreach (var assignment in userUserGroupAssignments)
                    {
                        //Buyer Location user groups are formatted as {buyerID}-{userID}.  This eliminates the unnecessary groups that end in "-{OrderApproval}", for example, helping performance.
                        if (assignment.UserGroupID.Split('-').Length == 2)
                        {
                            var userGroupLocation = await _oc.UserGroups.GetAsync<HSLocationUserGroup>(
                                buyerID,
                                assignment.UserGroupID
                                );
                            if (args.Search == null || userGroupLocation.Name.ToLower().Contains(args.Search))
                            {
                                userBuyerLocationAssignments.Add(userGroupLocation);
                            }
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
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return userGroups;
        }

        /// <summary>
        /// Private re-usable IsUserInUserGroup task method
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="userGroupID"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The boolean status for the IsUserInUserGroup process</returns>
        private async Task<bool> IsUserInUserGroup(string buyerID, string userGroupID, DecodedToken decodedToken)
        {
            var resp = false;
            try
            {
                var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
                var userGroupAssignmentForAccess = await _oc.UserGroups.ListUserAssignmentsAsync(buyerID, userGroupID, me.ID);
                resp = userGroupAssignmentForAccess.Items.Count > 0;
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable GetUserUserGroupAssignments task method
        /// </summary>
        /// <param name="userGroupType"></param>
        /// <param name="parentID"></param>
        /// <param name="userID"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The list of UserGroupAssignment response objects from the GetUserUserGroupAssignments process</returns>
        public async Task<List<UserGroupAssignment>> GetUserUserGroupAssignments(string userGroupType, string parentID, string userID, DecodedToken decodedToken)
        {
            var resp = new List<UserGroupAssignment>();
            try
            {
                if (userGroupType.Equals($@"UserPermissions", StringComparison.OrdinalIgnoreCase))
                {
                    resp = await _oc.SupplierUserGroups.ListAllUserAssignmentsAsync(parentID, userID: userID, accessToken: decodedToken.AccessToken);
                }
                else
                {
                    resp = await _oc.UserGroups.ListAllUserAssignmentsAsync(parentID, userID: userID, accessToken: decodedToken.AccessToken);
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable ListUserGroupsForNewUser task method
        /// </summary>
        /// <param name="args"></param>
        /// <param name="buyerID"></param>
        /// <param name="homeCountry"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The ListPage of HSLocationUserGroup response objects from the ListUserGroupsForNewUser process</returns>
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerID, string homeCountry, DecodedToken decodedToken)
        {
            var userGroups = new ListPage<HSLocationUserGroup>();
            try
            {
                userGroups = await _oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID, search: args.Search, filters: $"xp.Country={homeCountry}&xp.Type=BuyerLocation",
                   page: args.Page, pageSize: 100);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return userGroups;
        }
    };
}