using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using Headstart.Models.Misc;
using Headstart.API.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Headstart.API.Controllers
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
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <returns>The HSBuyerLocation object by buyerId and buyerLocationId</returns>
		[HttpGet, Route("{buyerId}/{buyerLocationId}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
		public async Task<HSBuyerLocation> Get(string buyerId, string buyerLocationId)
		{
			return await _buyerLocationCommand.Get(buyerId, buyerLocationId);
		}

		/// <summary>
		/// Creates a Buyer location action (POST method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocation"></param>
		/// <returns>The newly created HSBuyerLocation object</returns>
		[HttpPost, Route("{buyerId}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
		public async Task<HSBuyerLocation> Create(string buyerId, [FromBody] HSBuyerLocation buyerLocation)
		{
			return await _buyerLocationCommand.Create(buyerId, buyerLocation);
		}

		/// <summary>
		/// Creates a Buyer location permission group action (POST method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <param name="permissionGroupId"></param>
		/// <returns></returns>
		[HttpPost, Route("{buyerId}/{buyerLocationId}/permissions/{permissionGroupID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
		public async Task CreatePermissionGroup(string buyerId, string buyerLocationId, string permissionGroupId)
		{
			await _buyerLocationCommand.CreateSinglePermissionGroup(buyerLocationId, permissionGroupId);
		}

		/// <summary>
		/// Updates the Buyer location action (PUT method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <param name="buyerLocation"></param>
		/// <returns>The newly updated HSBuyerLocation object</returns>
		[HttpPut, Route("{buyerId}/{buyerLocationId}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
		public async Task<HSBuyerLocation> Save(string buyerId, string buyerLocationId, [FromBody] HSBuyerLocation buyerLocation)
		{
			return await _buyerLocationCommand.Save(buyerId, buyerLocationId, buyerLocation);
		}

		/// <summary>
		/// Removes/Deletes the Buyer location action (DELETE method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <returns></returns>
		[HttpDelete, Route("{buyerId}/{buyerLocationId}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
		public async Task Delete(string buyerId, string buyerLocationId)
		{
			await _buyerLocationCommand.Delete(buyerId, buyerLocationId);
		}

		/// <summary>
		/// Gets the list of location permission user groups (GET method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <returns>The list of UserGroupAssignment objects by buyerId and buyerLocationId</returns>
		[HttpGet, Route("{buyerId}/{buyerLocationId}/permissions"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<List<UserGroupAssignment>> ListLocationPermissionUserGroups(string buyerId, string buyerLocationId)
		{
			return await _locationPermissionCommand.ListLocationPermissionAsssignments(buyerId, buyerLocationId, UserContext);
		}

		/// <summary>
		/// Gets the ListPage of orders for a specific location as a buyer, ensures user has access to location orders (GET method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <param name="listArgs"></param>
		/// <returns>The ListPage of orders for a specific location as a buyer, ensures user has access to location orders, by buyerId and buyerLocationId</returns>
		[HttpGet, Route("{buyerId}/{buyerLocationId}/users"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<ListPage<HSUser>> ListLocationUsers(string buyerId, string buyerLocationId, ListArgs<HSOrder> listArgs)
		{
			return await _locationPermissionCommand.ListLocationUsers(buyerId, buyerLocationId, UserContext);
		}

		/// <summary>
		/// Updates the Buyer location permissions, add or delete access (POST method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <param name="locationPermissionUpdate"></param>
		/// <returns>The updated the Headstart Buyer Location permissions, add or delete access</returns>
		[HttpPost, Route("{buyerId}/{buyerLocationId}/permissions"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerId, string buyerLocationId, [FromBody] LocationPermissionUpdate locationPermissionUpdate)
		{
			return await _locationPermissionCommand.UpdateLocationPermissions(buyerId, buyerLocationId, locationPermissionUpdate, UserContext);
		}

		/// <summary>
		/// Gets the list of Buyer location approval permission user groups (GET method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <returns>The updated list of UserGroupAssignment object with approval permissions</returns>
		[HttpGet, Route("{buyerId}/{buyerLocationId}/approvalpermissions"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerId, string buyerLocationId)
		{
			return await _locationPermissionCommand.ListLocationPermissionAsssignments(buyerId, buyerLocationId, UserContext);
		}

		/// <summary>
		/// Gets the general approval threshold for location (GET method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <returns>The general approval threshold for location</returns>
		[HttpGet, Route("{buyerId}/{buyerLocationId}/approvalthreshold"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<decimal> GetApprovalThreshold(string buyerId, string buyerLocationId)
		{
			return await _locationPermissionCommand.GetApprovalThreshold(buyerId, buyerLocationId, UserContext);
		}

		/// <summary>
		/// Creates a Buyer Location Approval Rule object (POST method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <param name="approval"></param>
		/// <returns>The newly created Buyer Location Approval Rule object</returns>
		[HttpPost, Route("{buyerId}/{buyerLocationId}/approval"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<ApprovalRule> SetLocationApprovalThreshold(string buyerId, string buyerLocationId, [FromBody] ApprovalRule approval)
		{
			return await _locationPermissionCommand.SaveApprovalRule(buyerId, buyerLocationId, approval, UserContext);
		}

		/// <summary>
		/// Removes/Deletes the Location Approval Rule object (PUT method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="buyerLocationId"></param>
		/// <param name="approvalId"></param>
		/// <returns></returns>
		[HttpPut, Route("{buyerId}/{buyerLocationId}/approval/{approvalID}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task DeleteLocationApprovalThreshold(string buyerId, string buyerLocationId, string approvalId)
		{
			await _locationPermissionCommand.DeleteApprovalRule(buyerId, buyerLocationId, approvalId, UserContext);
		}

		/// <summary>
		/// Gets the list of all of a user's, user group assignments (GET method)
		/// </summary>
		/// <param name="userGroupType"></param>
		/// <param name="parentId"></param>
		/// <param name="userId"></param>
		/// <returns>The list of all of a user's user group assignments</returns>
		[HttpGet, Route("{userGroupType}/{parentID}/usergroupassignments/{userID}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
		public async Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentId, string userId)
		{
			return await _locationPermissionCommand.ListUserUserGroupAssignments(userGroupType, parentId, userId, UserContext);
		}

		/// <summary>
		/// Gets the ListPage of user groups for home country (GET method)
		/// </summary>
		/// <param name="args"></param>
		/// <param name="buyerId"></param>
		/// <param name="userId"></param>
		/// <returns>The ListPage of user groups for home country</returns>
		[HttpGet, Route("{buyerId}/usergroups/{userId}"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
		public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerId, string userId)
		{
			return await _locationPermissionCommand.ListUserGroupsByCountry(args, buyerId, userId, UserContext);
		}

		/// <summary>
		/// Gets the ListPage of user groups for new user (GET method)
		/// </summary>
		/// <param name="args"></param>
		/// <param name="buyerId"></param>
		/// <param name="homeCountry"></param>
		/// <returns>The ListPage of user groups for new user</returns>
		[HttpGet, Route("{buyerId}/{homeCountry}/usergroups"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
		public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerId, string homeCountry)
		{
			return await _locationPermissionCommand.ListUserGroupsForNewUser(args, buyerId, homeCountry, UserContext);
		}

		/// <summary>
		/// Updates the User Groups from anonymous to new user (PUT method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="newUserId"></param>
		/// <returns></returns>
		[HttpPut, Route("{buyerId}/reassignusergroups/{newUserId}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task ReassignUserGroups(string buyerId, string newUserId)
		{
			await _buyerLocationCommand.ReassignUserGroups(buyerId, newUserId);
		}
	}
}