using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrderCloud.SDK;
using Headstart.Models;
using Headstart.Models.Misc;
using ordercloud.integrations.library;
using Headstart.Common.Constants;
using Azure.Core;
using ordercloud.integrations.library.helpers;

namespace Headstart.API.Commands
{
    public interface ILocationPermissionCommand
    {
        Task<List<UserGroupAssignment>> ListLocationPermissionAsssignments(string buyerID, string locationID, VerifiedUserContext verifiedUser);
        Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerID, string locationID, VerifiedUserContext verifiedUser);
        Task<decimal> GetApprovalThreshold(string buyerID, string locationID, VerifiedUserContext verifiedUser);
        Task<decimal> SetLocationApprovalThreshold(string buyerID, string locationID, decimal newApprovalThreshold, VerifiedUserContext verifiedUser);
        Task<ListPage<HSUser>> ListLocationUsers(string buyerID, string locationID, VerifiedUserContext verifiedUser);
        Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerID, string locationID, LocationPermissionUpdate locationPermissionUpdate, VerifiedUserContext verifiedUser);
        Task<bool> IsUserInAccessGroup(string locationID, string groupSuffix, VerifiedUserContext verifiedUser);
        Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentID, string userID, VerifiedUserContext verifiedUser);
        Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerID, string userID, VerifiedUserContext verifiedUser);
        Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerID, string userID, VerifiedUserContext verifiedUser);
    }

    public class LocationPermissionCommand : ILocationPermissionCommand
    {
        private readonly IOrderCloudClient _oc;
        
        public LocationPermissionCommand(IOrderCloudClient oc)
        {
			_oc = oc;
        }

        public async Task<List<UserGroupAssignment>> ListLocationPermissionAsssignments(string buyerID, string locationID, VerifiedUserContext verifiedUser)
        {
            await EnsureUserIsLocationAdmin(locationID, verifiedUser);
            var locationUserTypes = HSUserTypes.BuyerLocation().Where(s => s.UserGroupIDSuffix != UserGroupSuffix.NeedsApproval.ToString() || s.UserGroupIDSuffix != UserGroupSuffix.OrderApprover.ToString());
            var userGroupAssignmentResponses = await Throttler.RunAsync(locationUserTypes, 100, 5, locationUserType => {
                return _oc.UserGroups.ListUserAssignmentsAsync(buyerID, userGroupID: $"{locationID}-{locationUserType.UserGroupIDSuffix}", pageSize: 100);
            });
            return userGroupAssignmentResponses
                .Select(userGroupAssignmentResponse => userGroupAssignmentResponse.Items)
                .SelectMany(l => l)
                .ToList();
        }

        public async Task<decimal> GetApprovalThreshold(string buyerID, string locationID, VerifiedUserContext verifiedUser)
        {
            await EnsureUserIsLocationAdmin(locationID, verifiedUser);
            var approvalRule = await _oc.ApprovalRules.GetAsync(buyerID, locationID);
            var threshold = Convert.ToDecimal(approvalRule.RuleExpression.Split('>')[1]);
            return threshold;
        }

        public async Task<decimal> SetLocationApprovalThreshold(string buyerID, string locationID, decimal newApprovalThreshold, VerifiedUserContext verifiedUser)
        {
            await EnsureUserIsLocationAdmin(locationID, verifiedUser);
            var approvalRulePatch = new PartialApprovalRule()
            { 
                RuleExpression = $"order.xp.ApprovalNeeded = '{locationID}' & order.Total > {newApprovalThreshold}"
            };
            await _oc.ApprovalRules.PatchAsync(buyerID, locationID, approvalRulePatch);
            return newApprovalThreshold;
        }

        public async Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerID, string locationID, VerifiedUserContext verifiedUser)
        {
            await EnsureUserIsLocationAdmin(locationID, verifiedUser);
            var locationUserTypes = HSUserTypes.BuyerLocation().Where(s => s.UserGroupIDSuffix == UserGroupSuffix.NeedsApproval.ToString() || s.UserGroupIDSuffix == UserGroupSuffix.OrderApprover.ToString());
            var userGroupAssignmentResponses = await Throttler.RunAsync(locationUserTypes, 100, 5, locationUserType => {
                return _oc.UserGroups.ListUserAssignmentsAsync(buyerID, userGroupID: $"{locationID}-{locationUserType.UserGroupIDSuffix}", pageSize: 100);
            });
            return userGroupAssignmentResponses
                .Select(userGroupAssignmentResponse => userGroupAssignmentResponse.Items)
                .SelectMany(l => l)
                .ToList();
        }

        public async Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerID, string locationID, LocationPermissionUpdate locationPermissionUpdate, VerifiedUserContext verifiedUser)
        {
            await EnsureUserIsLocationAdmin(locationID, verifiedUser);

            await Throttler.RunAsync(locationPermissionUpdate.AssignmentsToAdd, 100, 5, assignmentToAdd =>
            {
                return _oc.UserGroups.SaveUserAssignmentAsync(buyerID, assignmentToAdd);
            });
            await Throttler.RunAsync(locationPermissionUpdate.AssignmentsToDelete, 100, 5, assignmentToDelete =>
            {
                return _oc.UserGroups.DeleteUserAssignmentAsync(buyerID, assignmentToDelete.UserGroupID, assignmentToDelete.UserID);
            });

            return await ListLocationPermissionAsssignments(buyerID, locationID, verifiedUser);
        }

        public async Task<ListPage<HSUser>> ListLocationUsers(string buyerID, string locationID, VerifiedUserContext verifiedUser)
        {
            await EnsureUserIsLocationAdmin(locationID, verifiedUser);
            return await _oc.Users.ListAsync<HSUser>(buyerID, userGroupID: locationID);
        }

        public async Task EnsureUserIsLocationAdmin(string locationID, VerifiedUserContext verifiedUser)
        {
            var hasAccess = await IsUserInAccessGroup(locationID, UserGroupSuffix.PermissionAdmin.ToString(), verifiedUser);
            Require.That(hasAccess, new ErrorCode("Insufficient Access", 403, $"User cannot manage permissions for: {locationID}"));
        }

        public async Task<bool> IsUserInAccessGroup(string locationID, string groupSuffix, VerifiedUserContext verifiedUser)
        {
            var buyerID = verifiedUser.BuyerID;
            var userGroupID = $"{locationID}-{groupSuffix}";
            return await IsUserInUserGroup(buyerID, userGroupID, verifiedUser);
        }

        public async Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentID, string userID, VerifiedUserContext verifiedUser)
        {
            var userUserGroupAssignments = await GetUserUserGroupAssignments(userGroupType, parentID, userID, verifiedUser);
            return userUserGroupAssignments;
        }

        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerID, string userID, VerifiedUserContext verifiedUser)
        {
            var user = await _oc.Users.GetAsync(
                buyerID,
                userID
                );
            var userGroups = new ListPage<HSLocationUserGroup>();
            var assigned = args.Filters.FirstOrDefault(f => f.Name == "assigned").QueryParams.
                              FirstOrDefault(q => q.Item1 == "assigned").Item2;

            if (!bool.Parse(assigned))
            {
                userGroups = await _oc.UserGroups.ListAsync<HSLocationUserGroup>(
                   buyerID,
                   search: args.Search,
                   filters: $"xp.Country={user.xp.Country}&xp.Type=BuyerLocation",
                   page: args.Page,
                   pageSize: 100
                   );
            } else
            {
                var userUserGroupAssignments = await GetUserUserGroupAssignments("BuyerLocation", buyerID, userID, verifiedUser);
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
            return userGroups;
        }

    private async Task<bool> IsUserInUserGroup(string buyerID, string userGroupID, VerifiedUserContext verifiedUser)
        {
            var userGroupAssignmentForAccess = await _oc.UserGroups.ListUserAssignmentsAsync(buyerID, userGroupID, verifiedUser.UserID);
            return userGroupAssignmentForAccess.Items.Count > 0;
        }

    public async Task<List<UserGroupAssignment>> GetUserUserGroupAssignments(string userGroupType, string parentID, string userID, VerifiedUserContext verifiedUser)
        {
            if (userGroupType == "UserPermissions")
            {
                return await ListAllAsync.List((page) => _oc.SupplierUserGroups.ListUserAssignmentsAsync(
                   parentID,
                   userID: userID,
                   page: page,
                   pageSize: 100,
                   accessToken: verifiedUser.AccessToken
                   ));
            } else
            {
                return await ListAllAsync.List((page) => _oc.UserGroups.ListUserAssignmentsAsync(
                   parentID,
                   userID: userID,
                   page: page,
                   pageSize: 100,
                   accessToken: verifiedUser.AccessToken
                   ));
            }
        }

        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerID, string homeCountry, VerifiedUserContext verifiedUser)
        {
            var userGroups = await _oc.UserGroups.ListAsync<HSLocationUserGroup>(
                   buyerID,
                   search: args.Search,
                   filters: $"xp.Country={homeCountry}&xp.Type=BuyerLocation",
                   page: args.Page,
                   pageSize: 100
                   );
            return userGroups;
        }
    };
}