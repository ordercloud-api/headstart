using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
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
}
