using Headstart.Models;
using Headstart.Models.Misc;
using OrderCloud.SDK;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using System;
using Headstart.Common;
using OrderCloud.Catalyst;
using Headstart.API.Commands.Crud;

namespace Headstart.API.Commands
{
    public interface IHSBuyerLocationCommand
    {
        Task<HSBuyerLocation> Get(string buyerID, string buyerLocationID);
        Task<HSBuyerLocation> Create(string buyerID, HSBuyerLocation buyerLocation);
        Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, HSBuyerLocation buyerLocation);
        Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, HSBuyerLocation buyerLocation, string token, IOrderCloudClient oc = null);
        Task Delete(string buyerID, string buyerLocationID);
        Task CreateSinglePermissionGroup(string buyerLocationID, string permissionGroupID);
        Task ReassignUserGroups(string buyerID, string newUserID);
    }

    public class HSBuyerLocationCommand : IHSBuyerLocationCommand
    {
        private IOrderCloudClient _oc;
        private readonly AppSettings _settings;
        public HSBuyerLocationCommand(AppSettings settings, IOrderCloudClient oc)
        {
            _settings = settings;
            _oc = oc;
        }

        public async Task<HSBuyerLocation> Get(string buyerID, string buyerLocationID)
        {
            var buyerAddress = await _oc.Addresses.GetAsync<HSAddressBuyer>(buyerID, buyerLocationID);
            var buyerUserGroup = await _oc.UserGroups.GetAsync<HSLocationUserGroup>(buyerID, buyerLocationID);
            return new HSBuyerLocation
            {
                Address = buyerAddress,
                UserGroup = buyerUserGroup
            };
        }

        public async Task<HSBuyerLocation> Create(string buyerID, HSBuyerLocation buyerLocation)
        {
            return await Create(buyerID, buyerLocation, null, _oc);
        }

        public async Task<HSBuyerLocation> Create(string buyerID, HSBuyerLocation buyerLocation, string token, IOrderCloudClient ocClient)
        {
            var buyerLocationID = CreateBuyerLocationID(buyerID, buyerLocation.Address.ID);
            buyerLocation.Address.ID = buyerLocationID;
            var buyerAddress = await ocClient.Addresses.CreateAsync<HSAddressBuyer>(buyerID, buyerLocation.Address, accessToken: token);

            buyerLocation.UserGroup.ID = buyerAddress.ID;
            var buyerUserGroup = await ocClient.UserGroups.CreateAsync<HSLocationUserGroup>(buyerID, buyerLocation.UserGroup, accessToken: token);
            await CreateUserGroupAndAssignments(buyerID, buyerAddress.ID, token, ocClient);
            await CreateLocationUserGroupsAndApprovalRule(buyerAddress.ID, buyerAddress.AddressName, token, ocClient);

            return new HSBuyerLocation
            {
                Address = buyerAddress,
                UserGroup = buyerUserGroup,
            };
        }

        private string CreateBuyerLocationID(string buyerID, string idInRequest)
        {
            if (idInRequest.Contains("LocationIncrementor"))
            {
                // prevents prefix duplication with address validation prewebhooks
                return idInRequest;
            }
            if (idInRequest == null || idInRequest.Length == 0)
            {
                return buyerID + "-{" + buyerID + "-LocationIncrementor}";
            }
            if (idInRequest.StartsWith(buyerID + "-"))
			{
                // prevents prefix duplication
                return idInRequest;
            }
            return buyerID + "-" + idInRequest.Replace("-", "_");
        }

        public async Task CreateUserGroupAndAssignments(string buyerID, string buyerLocationID, string token, IOrderCloudClient ocClient)
        {
            var assignment = new AddressAssignment
            {
                AddressID = buyerLocationID,
                UserGroupID = buyerLocationID,
                IsBilling = true,
                IsShipping = true
            };
            await ocClient.Addresses.SaveAssignmentAsync(buyerID, assignment, accessToken: token);
        }

        public async Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, HSBuyerLocation buyerLocation)
        {
            // not being called by seed endpoint - use stored ordercloud client and stored admin token
            return await Save(buyerID, buyerLocationID, buyerLocation, null, _oc);
        }

        public async Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, HSBuyerLocation buyerLocation, string token, IOrderCloudClient ocClient)
        {
            buyerLocation.Address.ID = buyerLocationID;
            buyerLocation.UserGroup.ID = buyerLocationID;
            UserGroup existingLocation = null;
            try
            {
                existingLocation = await ocClient.UserGroups.GetAsync(buyerID, buyerLocationID, token);
            } catch (Exception e) { } // Do nothing if not found
            var updatedBuyerAddress = await ocClient.Addresses.SaveAsync<HSAddressBuyer>(buyerID, buyerLocationID, buyerLocation.Address, accessToken: token);
            var updatedBuyerUserGroup = await ocClient.UserGroups.SaveAsync<HSLocationUserGroup>(buyerID, buyerLocationID, buyerLocation.UserGroup, accessToken: token);
            var location = new HSBuyerLocation
            {
                Address = updatedBuyerAddress,
                UserGroup = updatedBuyerUserGroup,
            };
            if (existingLocation == null)
			{
                var assignments = CreateUserGroupAndAssignments(buyerID, buyerLocationID, token, ocClient);
                var groups = CreateLocationUserGroupsAndApprovalRule(buyerLocationID, buyerLocation.Address.AddressName, token, ocClient);
                await Task.WhenAll(assignments, groups);
            }
            return location;
        }

        public async Task Delete(string buyerID, string buyerLocationID)
        {
            var deleteAddressReq = _oc.Addresses.DeleteAsync(buyerID, buyerLocationID);
            var deleteUserGroupReq = _oc.UserGroups.DeleteAsync(buyerID, buyerLocationID);
            await Task.WhenAll(deleteAddressReq, deleteUserGroupReq);
        }

        public async Task CreateLocationUserGroupsAndApprovalRule(string buyerLocationID, string locationName, string accessToken, IOrderCloudClient ocClient)
        {
            var buyerID = buyerLocationID.Split('-').First();
            var AddUserTypeRequests = HSUserTypes.BuyerLocation().Select(userType => AddUserTypeToLocation(buyerLocationID, userType, accessToken, ocClient));
            await Task.WhenAll(AddUserTypeRequests);
            var isSeedingEnvironment = ocClient != null;
            if(!isSeedingEnvironment)
            {
                var approvingGroupID = $"{buyerLocationID}-{UserGroupSuffix.OrderApprover}";
                await ocClient.ApprovalRules.CreateAsync(buyerID, new ApprovalRule()
                {
                    ID = buyerLocationID,
                    ApprovingGroupID = approvingGroupID,
                    Description = "General Approval Rule for Location. Every Order Over a Certain Limit will Require Approval for the designated group of users.",
                    Name = $"{locationName} General Location Approval Rule",
                    RuleExpression = $"order.xp.ApprovalNeeded = '{buyerLocationID}' & order.Total > 0"
                });
            }
        }

        public async Task CreateSinglePermissionGroup(string buyerLocationID, string permissionGroupID)
        {
            var permissionGroup = HSUserTypes.BuyerLocation().Find(userType => permissionGroupID.Contains(userType.UserGroupIDSuffix));
            await AddUserTypeToLocation(buyerLocationID, permissionGroup);
        }

        public async Task AddUserTypeToLocation(string buyerLocationID, HSUserType hsUserType)
        {
            // not being called by seed endpoint - use stored ordercloud client and stored admin token
            await AddUserTypeToLocation(buyerLocationID, hsUserType, null, _oc);
        }

        public async Task AddUserTypeToLocation(string buyerLocationID, HSUserType hsUserType, string accessToken, IOrderCloudClient oc)
        {
            // if we're seeding then use the passed in oc client
            // to support multiple environments and ease of setup for new orgs
            // else used the configured client
            var token = oc == null ? null : accessToken;
            var ocClient = oc ?? _oc;

            var buyerID = buyerLocationID.Split('-').First();
            var userGroupID = $"{buyerLocationID}-{hsUserType.UserGroupIDSuffix}";
            await ocClient.UserGroups.CreateAsync(buyerID, new PartialUserGroup()
            {
                ID = userGroupID,
                Name = hsUserType.UserGroupName,
                xp = new
                {
                    Role = hsUserType.UserGroupIDSuffix.ToString(),
                    Type = hsUserType.UserGroupType,
                    Location = buyerLocationID
                }
            }, token);
            foreach (var customRole in hsUserType.CustomRoles)
            {
                await ocClient.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment()
                {
                    BuyerID = buyerID,
                    UserGroupID = userGroupID,
                    SecurityProfileID = customRole.ToString()
                }, token);
            }
        }

        public async Task ReassignUserGroups(string buyerID, string newUserID)
        {
            var userGroupAssignments = await _oc.UserGroups.ListAllUserAssignmentsAsync(buyerID, userID: newUserID);
            await Throttler.RunAsync(userGroupAssignments, 100, 5, assignment =>
                RemoveAndAddUserGroupAssignment(buyerID, newUserID, assignment?.UserGroupID)
                ); 
        }

        // Temporary work around for a platform issue. When a new user is registered we need to 
        // delete and reassign usergroup assignments for that user to view products
        // issue: https://four51.atlassian.net/browse/EX-2222
        private async Task RemoveAndAddUserGroupAssignment(string buyerID, string newUserID, string userGroupID)
        {
            await _oc.UserGroups.DeleteUserAssignmentAsync(buyerID, userGroupID, newUserID);
            await _oc.UserGroups.SaveUserAssignmentAsync(buyerID, new UserGroupAssignment
            {
                UserGroupID = userGroupID,
                UserID = newUserID
            });
        }
    }
}