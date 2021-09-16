using Headstart.Models;
using Headstart.Models.Attributes;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Models.Misc;
using ordercloud.integrations.library;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
    [Route("buyerlocations")]
    public class BuyerLocationController : CatalystController
    {
        private readonly IHSBuyerLocationCommand _buyerLocationCommand;
        private readonly IOrderCloudClient _oc;
        private readonly ILocationPermissionCommand _locationPermissionCommand;
        public BuyerLocationController(ILocationPermissionCommand locationPermissionCommand, IHSBuyerLocationCommand buyerLocationCommand, IOrderCloudClient oc) 
        {
            _buyerLocationCommand = buyerLocationCommand;
            _locationPermissionCommand = locationPermissionCommand;
            _oc = oc;
        }

        /// <summary>
        /// GET a Buyer Location
        /// </summary>
        [HttpGet, Route("{buyerID}/{buyerLocationID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Get(string buyerID, string buyerLocationID)
        {
            return await _buyerLocationCommand.Get(buyerID, buyerLocationID);
        }

        /// <summary>
        /// POST a Buyer Location
        /// </summary>
        [HttpPost, Route("{buyerID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Create(string buyerID, [FromBody] HSBuyerLocation buyerLocation)
        {
            return await _buyerLocationCommand.Create(buyerID, buyerLocation);
        }
        /// <summary>
        /// POST a Buyer Location permission group
        /// </summary>
        [HttpPost, Route("{buyerID}/{buyerLocationID}/permissions/{permissionGroupID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task CreatePermissionGroup(string buyerID, string buyerLocationID, string permissionGroupID)
        {
            await _buyerLocationCommand.CreateSinglePermissionGroup(buyerLocationID, permissionGroupID);
        }

        /// <summary>
        /// PUT a Buyer Location
        /// </summary>
        [HttpPut, Route("{buyerID}/{buyerLocationID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, [FromBody] HSBuyerLocation buyerLocation)
        {
            return await _buyerLocationCommand.Save(buyerID, buyerLocationID, buyerLocation);
        }

        /// <summary>
        /// Delete a Buyer Location
        /// </summary>
        [HttpDelete, Route("{buyerID}/{buyerLocationID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task Delete(string buyerID, string buyerLocationID)
        {
            await _buyerLocationCommand.Delete(buyerID, buyerLocationID);
        }

        /// <summary>
        /// GET List of location permission user groups
        /// </summary>
        [HttpGet]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/permissions")]
        public async Task<List<UserGroupAssignment>> ListLocationPermissionUserGroups(string buyerID, string buyerLocationID)
        {
            return await _locationPermissionCommand.ListLocationPermissionAsssignments(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// LIST orders for a specific location as a buyer, ensures user has access to location orders
        /// </summary>
        [HttpGet, Route("{buyerID}/{buyerLocationID}/users"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<ListPage<HSUser>> ListLocationUsers(string buyerID, string buyerLocationID, ListArgs<HSOrder> listArgs)
        {
            return await _locationPermissionCommand.ListLocationUsers(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// POST location permissions, add or delete access
        /// </summary>
        [HttpPost, Route("{buyerID}/{buyerLocationID}/permissions"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerID, string buyerLocationID, [FromBody] LocationPermissionUpdate locationPermissionUpdate)
        {
            return await _locationPermissionCommand.UpdateLocationPermissions(buyerID, buyerLocationID, locationPermissionUpdate, UserContext);
        }

        /// <summary>
        /// GET List of location approval permission user groups
        /// </summary>
        [HttpGet]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approvalpermissions")]
        public async Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerID, string buyerLocationID)
        {
            return await _locationPermissionCommand.ListLocationPermissionAsssignments(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// GET general approval threshold for location
        /// </summary>
        [HttpGet]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approvalthreshold")]
        public async Task<decimal> GetApprovalThreshold(string buyerID, string buyerLocationID)
        {
            return await _locationPermissionCommand.GetApprovalThreshold(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// POST Location Approval Rule
        /// </summary>
        [HttpPost]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approval")]
        public async Task<ApprovalRule> SetLocationApprovalThreshold(string buyerID, string buyerLocationID, [FromBody] ApprovalRule approval)
        {
            return await _locationPermissionCommand.SaveApprovalRule(buyerID, buyerLocationID, approval, UserContext);
        }

        /// <summary>
        /// DELETE Location Approval Rule
        /// </summary>
        [HttpPut]
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approval/{approvalID}")]
        public async Task DeleteLocationApprovalThreshold(string buyerID, string buyerLocationID, string approvalID)
        {
            await _locationPermissionCommand.DeleteApprovalRule(buyerID, buyerLocationID, approvalID, UserContext);
        }

        /// <summary>
        /// LIST all of a user's user group assignments
        /// </summary>
        [HttpGet, Route("{userGroupType}/{parentID}/usergroupassignments/{userID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentID, string userID)
        {
            return await _locationPermissionCommand.ListUserUserGroupAssignments(userGroupType, parentID, userID, UserContext);
        }

        /// <summary>
        /// LIST user groups for home country
        /// </summary>
        [HttpGet, Route("{buyerID}/usergroups/{userID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerID, string userID)
        {
            return await _locationPermissionCommand.ListUserGroupsByCountry(args, buyerID, userID, UserContext);
        }

        /// <summary>
        /// LIST user groups for new user
        /// </summary>
        [HttpGet, Route("{buyerID}/{homeCountry}/usergroups"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerID, string homeCountry)
        {
            return await _locationPermissionCommand.ListUserGroupsForNewUser(args, buyerID, homeCountry, UserContext);
        }

        /// <summary>
        /// PUT usergroups from anonymous to new user
        /// </summary>
        [OrderCloudUserAuth(ApiRole.Shopper)]
        [HttpPut, Route("{buyerID}/reassignusergroups/{newUserID}")]
        public async Task ReassignUserGroups(string buyerID, string newUserID)
        {
            await _buyerLocationCommand.ReassignUserGroups(buyerID, newUserID);
        }
    }
}
