using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    [Route("buyerlocations")]
    public class BuyerLocationController : CatalystController
    {
        private readonly IHSBuyerLocationCommand buyerLocationCommand;
        private readonly ILocationPermissionCommand locationPermissionCommand;

        public BuyerLocationController(ILocationPermissionCommand locationPermissionCommand, IHSBuyerLocationCommand buyerLocationCommand)
        {
            this.buyerLocationCommand = buyerLocationCommand;
            this.locationPermissionCommand = locationPermissionCommand;
        }

        /// <summary>
        /// GET a Buyer Location.
        /// </summary>
        [HttpGet, Route("{buyerID}/{buyerLocationID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Get(string buyerID, string buyerLocationID)
        {
            return await buyerLocationCommand.Get(buyerID, buyerLocationID);
        }

        /// <summary>
        /// POST a Buyer Location.
        /// </summary>
        [HttpPost, Route("{buyerID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Create(string buyerID, [FromBody] HSBuyerLocation buyerLocation)
        {
            return await buyerLocationCommand.Create(buyerID, buyerLocation);
        }

        /// <summary>
        /// POST a Buyer Location permission group.
        /// </summary>
        [HttpPost, Route("{buyerID}/{buyerLocationID}/permissions/{permissionGroupID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task CreatePermissionGroup(string buyerID, string buyerLocationID, string permissionGroupID)
        {
            await buyerLocationCommand.CreateSinglePermissionGroup(buyerLocationID, permissionGroupID);
        }

        /// <summary>
        /// PUT a Buyer Location.
        /// </summary>
        [HttpPut, Route("{buyerID}/{buyerLocationID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, [FromBody] HSBuyerLocation buyerLocation)
        {
            return await buyerLocationCommand.Save(buyerID, buyerLocationID, buyerLocation);
        }

        /// <summary>
        /// Delete a Buyer Location.
        /// </summary>
        [HttpDelete, Route("{buyerID}/{buyerLocationID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task Delete(string buyerID, string buyerLocationID)
        {
            await buyerLocationCommand.Delete(buyerID, buyerLocationID);
        }

        /// <summary>
        /// GET List of location permission user groups.
        /// </summary>
        [HttpGet]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/permissions")]
        public async Task<List<UserGroupAssignment>> ListLocationPermissionUserGroups(string buyerID, string buyerLocationID)
        {
            return await locationPermissionCommand.ListLocationPermissionAsssignments(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// LIST orders for a specific location as a buyer, ensures user has access to location orders.
        /// </summary>
        [HttpGet, Route("{buyerID}/{buyerLocationID}/users"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<ListPage<HSUser>> ListLocationUsers(string buyerID, string buyerLocationID, ListArgs<HSOrder> listArgs)
        {
            return await locationPermissionCommand.ListLocationUsers(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// POST location permissions, add or delete access.
        /// </summary>
        [HttpPost, Route("{buyerID}/{buyerLocationID}/permissions"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerID, string buyerLocationID, [FromBody] LocationPermissionUpdate locationPermissionUpdate)
        {
            return await locationPermissionCommand.UpdateLocationPermissions(buyerID, buyerLocationID, locationPermissionUpdate, UserContext);
        }

        /// <summary>
        /// GET List of location approval permission user groups.
        /// </summary>
        [HttpGet]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approvalpermissions")]
        public async Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerID, string buyerLocationID)
        {
            return await locationPermissionCommand.ListLocationPermissionAsssignments(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// GET general approval threshold for location.
        /// </summary>
        [HttpGet]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approvalthreshold")]
        public async Task<decimal> GetApprovalThreshold(string buyerID, string buyerLocationID)
        {
            return await locationPermissionCommand.GetApprovalThreshold(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// POST Location Approval Rule.
        /// </summary>
        [HttpPost]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approval")]
        public async Task<ApprovalRule> SetLocationApprovalThreshold(string buyerID, string buyerLocationID, [FromBody] ApprovalRule approval)
        {
            return await locationPermissionCommand.SaveApprovalRule(buyerID, buyerLocationID, approval, UserContext);
        }

        /// <summary>
        /// DELETE Location Approval Rule.
        /// </summary>
        [HttpPut]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approval/{approvalID}")]
        public async Task DeleteLocationApprovalThreshold(string buyerID, string buyerLocationID, string approvalID)
        {
            await locationPermissionCommand.DeleteApprovalRule(buyerID, buyerLocationID, approvalID, UserContext);
        }

        /// <summary>
        /// LIST all of a user's user group assignments.
        /// </summary>
        [HttpGet, Route("{userGroupType}/{parentID}/usergroupassignments/{userID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentID, string userID)
        {
            return await locationPermissionCommand.ListUserUserGroupAssignments(userGroupType, parentID, userID, UserContext);
        }

        /// <summary>
        /// LIST user groups for home country.
        /// </summary>
        [HttpGet, Route("{buyerID}/usergroups/{userID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerID, string userID)
        {
            return await locationPermissionCommand.ListUserGroupsByCountry(args, buyerID, userID, UserContext);
        }

        /// <summary>
        /// LIST user groups for new user.
        /// </summary>
        [HttpGet, Route("{buyerID}/{homeCountry}/usergroups"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerID, string homeCountry)
        {
            return await locationPermissionCommand.ListUserGroupsForNewUser(args, buyerID, homeCountry, UserContext);
        }

        /// <summary>
        /// PUT usergroups from anonymous to new user.
        /// </summary>
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [HttpPut, Route("{buyerID}/reassignusergroups/{newUserID}")]
        public async Task ReassignUserGroups(string buyerID, string newUserID)
        {
            await buyerLocationCommand.ReassignUserGroups(buyerID, newUserID);
        }
    }
}
