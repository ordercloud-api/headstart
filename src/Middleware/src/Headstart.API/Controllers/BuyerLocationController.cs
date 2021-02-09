using Headstart.Models;
using Headstart.Models.Attributes;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Models.Misc;
using ordercloud.integrations.library;
using Headstart.API.Controllers;
using Headstart.API.Commands;

namespace Headstart.Common.Controllers
{
    [DocComments("\"Files\" represents files for Headstart content management control")]
    [HSSection.Headstart(ListOrder = 6)]
    [Route("buyerlocations")]
    public class BuyerLocationController : BaseController
    {
        private readonly IHSBuyerLocationCommand _buyerLocationCommand;
        private readonly IOrderCloudClient _oc;
        private readonly ILocationPermissionCommand _locationPermissionCommand;
        public BuyerLocationController(ILocationPermissionCommand locationPermissionCommand, IHSBuyerLocationCommand buyerLocationCommand, IOrderCloudClient oc, AppSettings settings) : base(settings)
        {
            _buyerLocationCommand = buyerLocationCommand;
            _locationPermissionCommand = locationPermissionCommand;
            _oc = oc;
        }

        [DocName("GET a Buyer Location")]
        [HttpGet, Route("{buyerID}/{buyerLocationID}"), OrderCloudIntegrationsAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Get(string buyerID, string buyerLocationID)
        {
            return await _buyerLocationCommand.Get(buyerID, buyerLocationID, VerifiedUserContext.AccessToken);
        }

        [DocName("POST a Buyer Location")]
        [HttpPost, Route("{buyerID}"), OrderCloudIntegrationsAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Create(string buyerID, [FromBody] HSBuyerLocation buyerLocation)
        {
            // ocAuth is the token for the organization that is specified in the AppSettings
            var ocAuth = await _oc.AuthenticateAsync();
            return await _buyerLocationCommand.Create(buyerID, buyerLocation, ocAuth.AccessToken);
        }

        [DocName("PUT a Buyer Location")]
        [HttpPut, Route("{buyerID}/{buyerLocationID}"), OrderCloudIntegrationsAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, [FromBody] HSBuyerLocation buyerLocation)
        {
            return await _buyerLocationCommand.Save(buyerID, buyerLocationID, buyerLocation, VerifiedUserContext.AccessToken);
        }

        [DocName("Delete a Buyer Location")]
        [HttpDelete, Route("{buyerID}/{buyerLocationID}"), OrderCloudIntegrationsAuth(ApiRole.UserGroupAdmin, ApiRole.AddressAdmin)]
        public async Task Delete(string buyerID, string buyerLocationID)
        {
            await _buyerLocationCommand.Delete(buyerID, buyerLocationID, VerifiedUserContext.AccessToken);
        }

        [DocName("GET List of location permission user groups")]
        [HttpGet]
        [OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/permissions")]
        public async Task<List<UserGroupAssignment>> ListLocationPermissionUserGroups(string buyerID, string buyerLocationID)
        {
            return await _locationPermissionCommand.ListLocationPermissionAsssignments(buyerID, buyerLocationID, VerifiedUserContext);
        }

        [DocName("LIST orders for a specific location as a buyer, ensures user has access to location orders")]
        [HttpGet, Route("{buyerID}/{buyerLocationID}/users"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        public async Task<ListPage<HSUser>> ListLocationUsers(string buyerID, string buyerLocationID, ListArgs<HSOrder> listArgs)
        {
            return await _locationPermissionCommand.ListLocationUsers(buyerID, buyerLocationID, VerifiedUserContext);
        }

        [DocName("POST location permissions, add or delete access")]
        [HttpPost, Route("{buyerID}/{buyerLocationID}/permissions"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        public async Task<List<UserGroupAssignment>> UpdateLocationPermissions(string buyerID, string buyerLocationID, [FromBody] LocationPermissionUpdate locationPermissionUpdate)
        {
            return await _locationPermissionCommand.UpdateLocationPermissions(buyerID, buyerLocationID, locationPermissionUpdate, VerifiedUserContext);
        }

        [DocName("GET List of location approval permission user groups")]
        [HttpGet]
        [OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approvalpermissions")]
        public async Task<List<UserGroupAssignment>> ListLocationApprovalPermissionAsssignments(string buyerID, string buyerLocationID)
        {
            return await _locationPermissionCommand.ListLocationPermissionAsssignments(buyerID, buyerLocationID, VerifiedUserContext);
        }

        [DocName("GET general approval threshold for location")]
        [HttpGet]
        [OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approvalthreshold")]
        public async Task<decimal> GetApprovalThreshold(string buyerID, string buyerLocationID)
        {
            return await _locationPermissionCommand.GetApprovalThreshold(buyerID, buyerLocationID, VerifiedUserContext);
        }

        [DocName("POST set location approval threshold")]
        [HttpPost]
        [OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        [Route("{buyerID}/{buyerLocationID}/approvalthreshold")]
        public async Task<decimal> SetLocationApprovalThreshold(string buyerID, string buyerLocationID, [FromBody] LocationApprovalThresholdUpdate locationApprovalThresholdUpdate)
        {
            return await _locationPermissionCommand.SetLocationApprovalThreshold(buyerID, buyerLocationID, locationApprovalThresholdUpdate.Threshold, VerifiedUserContext);
        }

        [DocName("LIST all of a user's user group assignments")]
        [HttpGet, Route("{userGroupType}/{parentID}/usergroupassignments/{userID}"), OrderCloudIntegrationsAuth(ApiRole.UserGroupAdmin)]
        public async Task<List<UserGroupAssignment>> ListUserUserGroupAssignments(string userGroupType, string parentID, string userID)
        {
            return await _locationPermissionCommand.ListUserUserGroupAssignments(userGroupType, parentID, userID, VerifiedUserContext);
        }

        [DocName("LIST user groups for home country")]
        [HttpGet, Route("{buyerID}/usergroups/{userID}"), OrderCloudIntegrationsAuth(ApiRole.UserGroupAdmin)]
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsByCountry(ListArgs<HSLocationUserGroup> args, string buyerID, string userID)
        {
            return await _locationPermissionCommand.ListUserGroupsByCountry(args, buyerID, userID, VerifiedUserContext);
        }

        [DocName("LIST user groups for new user")]
        [HttpGet, Route("{buyerID}/{homeCountry}/usergroups"), OrderCloudIntegrationsAuth(ApiRole.UserGroupAdmin)]
        public async Task<ListPage<HSLocationUserGroup>> ListUserGroupsForNewUser(ListArgs<HSLocationUserGroup> args, string buyerID, string homeCountry)
        {
            return await _locationPermissionCommand.ListUserGroupsForNewUser(args, buyerID, homeCountry, VerifiedUserContext);
        }
    }
}
