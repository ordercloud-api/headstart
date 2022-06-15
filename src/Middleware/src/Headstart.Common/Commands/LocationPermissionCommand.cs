using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using Headstart.Common.Models;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public class LocationPermissionCommand : ILocationPermissionCommand
    {
        private readonly IOrderCloudClient oc;

        public LocationPermissionCommand(IOrderCloudClient oc)
        {
            this.oc = oc;
        }

        public async Task<List<UserGroupAssignment>> ListLocationPermissionAsssignments(string buyerID, string locationID, DecodedToken decodedToken)
        {
            await EnsureUserIsLocationAdmin(locationID, decodedToken);
            var locationUserTypes = HSUserTypes.BuyerLocation().Where(s => s.UserGroupIDSuffix != UserGroupSuffix.NeedsApproval.ToString() || s.UserGroupIDSuffix != UserGroupSuffix.OrderApprover.ToString());
            var userGroupAssignmentResponses = await Throttler.RunAsync(locationUserTypes, 100, 5, locationUserType =>
            {
                return oc.UserGroups.ListUserAssignmentsAsync(buyerID, userGroupID: $"{locationID}-{locationUserType.UserGroupIDSuffix}", pageSize: 100);
            });
            return userGroupAssignmentResponses
                .Select(userGroupAssignmentResponse => userGroupAssignmentResponse.Items)
                .SelectMany(l => l)
                .ToList();
        }

        public async Task<decimal> GetApprovalThreshold(string buyerID, string locationID, DecodedToken decodedToken)
        {
            await EnsureUserIsLocationAdmin(locationID, decodedToken);
            var approvalRule = await oc.ApprovalRules.GetAsync(buyerID, locationID);
            var threshold = Convert.ToDecimal(approvalRule.RuleExpression.Split('>')[1]);
            return threshold;
        }

        public async Task<ApprovalRule> SaveApprovalRule(string buyerID, string locationID, ApprovalRule approval, DecodedToken decodedToken)
        {
            await EnsureUserIsLocationAdmin(locationID, decodedToken);
            return await oc.ApprovalRules.SaveAsync(buyerID, approval.ID, approval);
        }

        public async Task DeleteApprovalRule(string buyerID, string locationID, string approvalID, DecodedToken decodedToken)
        {
            await EnsureUserIsLocationAdmin(locationID, decodedToken);
            await oc.ApprovalRules.DeleteAsync(buyerID, approvalID);
        }

        public async Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerID, string locationID, DecodedToken decodedToken)
        {
            await EnsureUserIsLocationAdmin(locationID, decodedToken);
            var locationUserTypes = HSUserTypes.BuyerLocation().Where(s => s.UserGroupIDSuffix == UserGroupSuffix.NeedsApproval.ToString() || s.UserGroupIDSuffix == UserGroupSuffix.OrderApprover.ToString());
            var userGroupAssignmentResponses = await Throttler.RunAsync(locationUserTypes, 100, 5, locationUserType =>
            {
                return oc.UserGroups.ListUserAssignmentsAsync(buyerID, userGroupID: $"{locationID}-{locationUserType.UserGroupIDSuffix}", pageSize: 100);
            });
            return userGroupAssignmentResponses
                .Select(userGroupAssignmentResponse => userGroupAssignmentResponse.Items)
                .SelectMany(l => l)
                .ToList();
        }

        public async Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerID, string locationID, LocationPermissionUpdate locationPermissionUpdate, DecodedToken decodedToken)
        {
            await EnsureUserIsLocationAdmin(locationID, decodedToken);

            await Throttler.RunAsync(locationPermissionUpdate.AssignmentsToAdd, 100, 5, assignmentToAdd =>
            {
                return oc.UserGroups.SaveUserAssignmentAsync(buyerID, assignmentToAdd);
            });
            await Throttler.RunAsync(locationPermissionUpdate.AssignmentsToDelete, 100, 5, assignmentToDelete =>
            {
                return oc.UserGroups.DeleteUserAssignmentAsync(buyerID, assignmentToDelete.UserGroupID, assignmentToDelete.UserID);
            });

            return await ListLocationPermissionAsssignments(buyerID, locationID, decodedToken);
        }

        public async Task<ListPage<HSUser>> ListLocationUsers(string buyerID, string locationID, DecodedToken decodedToken)
        {
            await EnsureUserIsLocationAdmin(locationID, decodedToken);
            return await oc.Users.ListAsync<HSUser>(buyerID, userGroupID: locationID);
        }

        public async Task EnsureUserIsLocationAdmin(string locationID, DecodedToken decodedToken)
        {
            var hasAccess = await IsUserInAccessGroup(locationID, UserGroupSuffix.PermissionAdmin.ToString(), decodedToken);
            Require.That(hasAccess, new ErrorCode("Insufficient Access", $"User cannot manage permissions for: {locationID}", HttpStatusCode.Forbidden));
        }

        public async Task<bool> IsUserInAccessGroup(string locationID, string groupSuffix, DecodedToken decodedToken)
        {
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            var buyerID = me.Buyer.ID;
            var userGroupID = $"{locationID}-{groupSuffix}";
            return await IsUserInUserGroup(buyerID, userGroupID, decodedToken);
        }

        public async Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentID, string userID, DecodedToken decodedToken)
        {
            var userUserGroupAssignments = await GetUserUserGroupAssignments(userGroupType, parentID, userID, decodedToken);
            return userUserGroupAssignments;
        }

        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerID, string userID, DecodedToken decodedToken)
        {
            var user = await oc.Users.GetAsync(
                buyerID,
                userID);
            var userGroups = new ListPage<HSLocationUserGroup>();
            var assigned = args.Filters.FirstOrDefault(f => f.PropertyName == "assigned").FilterExpression;

            if (!bool.Parse(assigned))
            {
                userGroups = await oc.UserGroups.ListAsync<HSLocationUserGroup>(
                   buyerID,
                   search: args.Search,
                   filters: $"xp.Country={user.xp.Country}&xp.Type=BuyerLocation",
                   page: args.Page,
                   pageSize: 100);
            }
            else
            {
                var userUserGroupAssignments = await GetUserUserGroupAssignments("BuyerLocation", buyerID, userID, decodedToken);
                var userBuyerLocationAssignments = new List<HSLocationUserGroup>();
                foreach (var assignment in userUserGroupAssignments)
                {
                    // Buyer Location user groups are formatted as {buyerID}-{userID}.  This eliminates the unnecessary groups that end in "-{OrderApproval}", for example, helping performance.
                    if (assignment.UserGroupID.Split('-').Length == 2)
                    {
                        var userGroupLocation = await oc.UserGroups.GetAsync<HSLocationUserGroup>(
                            buyerID,
                            assignment.UserGroupID);
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
                    PageSize = 100,
                };
            }

            return userGroups;
        }

        public async Task<List<UserGroupAssignment>> GetUserUserGroupAssignments(string userGroupType, string parentID, string userID, DecodedToken decodedToken)
        {
            if (userGroupType == "UserPermissions")
            {
                return await oc.SupplierUserGroups.ListAllUserAssignmentsAsync(
                   parentID,
                   userID: userID,
                   accessToken: decodedToken.AccessToken);
            }
            else
            {
                return await oc.UserGroups.ListAllUserAssignmentsAsync(
                   parentID,
                   userID: userID,
                   accessToken: decodedToken.AccessToken);
            }
        }

        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerID, string homeCountry, DecodedToken decodedToken)
        {
            var userGroups = await oc.UserGroups.ListAsync<HSLocationUserGroup>(
                   buyerID,
                   search: args.Search,
                   filters: $"xp.Country={homeCountry}&xp.Type=BuyerLocation",
                   page: args.Page,
                   pageSize: 100);
            return userGroups;
        }

        private async Task<bool> IsUserInUserGroup(string buyerID, string userGroupID, DecodedToken decodedToken)
        {
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            var userGroupAssignmentForAccess = await oc.UserGroups.ListUserAssignmentsAsync(buyerID, userGroupID, me.ID);
            return userGroupAssignmentForAccess.Items.Count > 0;
        }
    }
}
