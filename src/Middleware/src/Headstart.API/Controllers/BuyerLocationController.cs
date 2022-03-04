using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using Headstart.Models.Misc;
using Headstart.API.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Headstart.Common.Controllers
{
    [Route("buyerlocations")]
    public class BuyerLocationController : CatalystController
    {
        private readonly IHSBuyerLocationCommand _buyerLocationCommand;
        private readonly IOrderCloudClient _oc;
        private readonly ILocationPermissionCommand _locationPermissionCommand;

        /// <summary>
        /// The IOC based constructor method for the BuyerLocationController class object with Dependency Injection
        /// </summary>
        /// <param name="locationPermissionCommand"></param>
        /// <param name="buyerLocationCommand"></param>
        /// <param name="oc"></param>
        public BuyerLocationController(ILocationPermissionCommand locationPermissionCommand, IHSBuyerLocationCommand buyerLocationCommand, IOrderCloudClient oc) 
        {
            _buyerLocationCommand = buyerLocationCommand;
            _locationPermissionCommand = locationPermissionCommand;
            _oc = oc;
        }

        /// <summary>
        /// Gets the Buyer location action (GET method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <returns>The HSBuyerLocation object by buyerID and buyerLocationID</returns>
        [HttpGet, Route("{buyerID}/{buyerLocationID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Get(string buyerID, string buyerLocationID)
        {
            return await _buyerLocationCommand.Get(buyerID, buyerLocationID);
        }

        /// <summary>
        /// Creates a Buyer location action (POST method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocation"></param>
        /// <returns>The newly created HSBuyerLocation object</returns>
        [HttpPost, Route("{buyerID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Create(string buyerID, [FromBody] HSBuyerLocation buyerLocation)
        {
            return await _buyerLocationCommand.Create(buyerID, buyerLocation);
        }

        /// <summary>
        /// Creates a Buyer location permission group action (POST method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <param name="permissionGroupID"></param>
        /// <returns></returns>
        [HttpPost, Route("{buyerID}/{buyerLocationID}/permissions/{permissionGroupID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task CreatePermissionGroup(string buyerID, string buyerLocationID, string permissionGroupID)
        {
            await _buyerLocationCommand.CreateSinglePermissionGroup(buyerLocationID, permissionGroupID);
        }

        /// <summary>
        /// Updates the Buyer location action (PUT method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <param name="buyerLocation"></param>
        /// <returns>The newly updated HSBuyerLocation object</returns>
        [HttpPut, Route("{buyerID}/{buyerLocationID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, [FromBody] HSBuyerLocation buyerLocation)
        {
            return await _buyerLocationCommand.Save(buyerID, buyerLocationID, buyerLocation);
        }

        /// <summary>
        /// Removes/Deletes the Buyer location action (DELETE method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <returns></returns>
        [HttpDelete, Route("{buyerID}/{buyerLocationID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task Delete(string buyerID, string buyerLocationID)
        {
            await _buyerLocationCommand.Delete(buyerID, buyerLocationID);
        }

        /// <summary>
        /// Gets the list of location permission user groups (GET method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <returns>The list of UserGroupAssignment objects by buyerID and buyerLocationID</returns>
        [HttpGet, Route("{buyerID}/{buyerLocationID}/permissions"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<UserGroupAssignment>> ListLocationPermissionUserGroups(string buyerID, string buyerLocationID)
        {
            return await _locationPermissionCommand.ListLocationPermissionAsssignments(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// Gets the ListPage of orders for a specific location as a buyer, ensures user has access to location orders (GET method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <param name="listArgs"></param>
        /// <returns>The ListPage of orders for a specific location as a buyer, ensures user has access to location orders, by buyerID and buyerLocationID</returns>
        [HttpGet, Route("{buyerID}/{buyerLocationID}/users"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<ListPage<HSUser>> ListLocationUsers(string buyerID, string buyerLocationID, ListArgs<HSOrder> listArgs)
        {
            return await _locationPermissionCommand.ListLocationUsers(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// Updates the Buyer location permissions, add or delete access (POST method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <param name="locationPermissionUpdate"></param>
        /// <returns>The updated the Headstart Buyer Location permissions, add or delete access</returns>
        [HttpPost, Route("{buyerID}/{buyerLocationID}/permissions"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerID, string buyerLocationID, [FromBody] LocationPermissionUpdate locationPermissionUpdate)
        {
            return await _locationPermissionCommand.UpdateLocationPermissions(buyerID, buyerLocationID, locationPermissionUpdate, UserContext);
        }

        /// <summary>
        /// Gets the list of Buyer location approval permission user groups (GET method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <returns>The updated list of UserGroupAssignment object with approval permissions</returns>
        [HttpGet, Route("{buyerID}/{buyerLocationID}/approvalpermissions"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerID, string buyerLocationID)
        {
            return await _locationPermissionCommand.ListLocationPermissionAsssignments(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// Gets the general approval threshold for location (GET method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <returns>The general approval threshold for location</returns>
        [HttpGet, Route("{buyerID}/{buyerLocationID}/approvalthreshold"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<decimal> GetApprovalThreshold(string buyerID, string buyerLocationID)
        {
            return await _locationPermissionCommand.GetApprovalThreshold(buyerID, buyerLocationID, UserContext);
        }

        /// <summary>
        /// Creates a Buyer Location Approval Rule object (POST method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <param name="approval"></param>
        /// <returns>The newly created Buyer Location Approval Rule object</returns>
        [HttpPost, Route("{buyerID}/{buyerLocationID}/approval"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<ApprovalRule> SetLocationApprovalThreshold(string buyerID, string buyerLocationID, [FromBody] ApprovalRule approval)
        {
            return await _locationPermissionCommand.SaveApprovalRule(buyerID, buyerLocationID, approval, UserContext);
        }

        /// <summary>
        /// Removes/Deletes the Location Approval Rule object (PUT method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="buyerLocationID"></param>
        /// <param name="approvalID"></param>
        /// <returns></returns>
        [HttpPut, Route("{buyerID}/{buyerLocationID}/approval/{approvalID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task DeleteLocationApprovalThreshold(string buyerID, string buyerLocationID, string approvalID)
        {
            await _locationPermissionCommand.DeleteApprovalRule(buyerID, buyerLocationID, approvalID, UserContext);
        }

        /// <summary>
        /// Gets the list of all of a user's, user group assignments (GET method)
        /// </summary>
        /// <param name="userGroupType"></param>
        /// <param name="parentID"></param>
        /// <param name="userID"></param>
        /// <returns>The list of all of a user's user group assignments</returns>
        [HttpGet, Route("{userGroupType}/{parentID}/usergroupassignments/{userID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentID, string userID)
        {
            return await _locationPermissionCommand.ListUserUserGroupAssignments(userGroupType, parentID, userID, UserContext);
        }

        /// <summary>
        /// Gets the ListPage of user groups for home country (GET method)
        /// </summary>
        /// <param name="args"></param>
        /// <param name="buyerID"></param>
        /// <param name="userID"></param>
        /// <returns>The ListPage of user groups for home country</returns>
        [HttpGet, Route("{buyerID}/usergroups/{userID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerID, string userID)
        {
            return await _locationPermissionCommand.ListUserGroupsByCountry(args, buyerID, userID, UserContext);
        }

        /// <summary>
        /// Gets the ListPage of user groups for new user (GET method)
        /// </summary>
        /// <param name="args"></param>
        /// <param name="buyerID"></param>
        /// <param name="homeCountry"></param>
        /// <returns>The ListPage of user groups for new user</returns>
        [HttpGet, Route("{buyerID}/{homeCountry}/usergroups"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerID, string homeCountry)
        {
            return await _locationPermissionCommand.ListUserGroupsForNewUser(args, buyerID, homeCountry, UserContext);
        }

        /// <summary>
        /// Updates the usergroups from anonymous to new user (PUT method)
        /// </summary>
        /// <param name="buyerID"></param>
        /// <param name="newUserID"></param>
        /// <returns></returns>
        [HttpPut, Route("{buyerID}/reassignusergroups/{newUserID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task ReassignUserGroups(string buyerID, string newUserID)
        {
            await _buyerLocationCommand.ReassignUserGroups(buyerID, newUserID);
        }
    }
}