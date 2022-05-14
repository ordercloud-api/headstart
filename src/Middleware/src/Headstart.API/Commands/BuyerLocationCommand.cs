using Headstart.Models;
using Headstart.Models.Misc;
using OrderCloud.SDK;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using System;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

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
		private readonly AppSettings settings;
		private IOrderCloudClient oc;

		/// <summary>
		/// The IOC based constructor method for the HSBuyerLocationCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="oc"></param>
		public HSBuyerLocationCommand(AppSettings settings, IOrderCloudClient oc)
		{
			try
			{
				this.settings = settings;
            	this.oc = oc;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable Get task method to get the HsBuyerLocation object by the buyerId and buyerLocationID
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="buyerLocationID"></param>
		/// <returns>The HSBuyerLocation object by the buyerID and buyerLocationID</returns>
		public async Task<HSBuyerLocation> Get(string buyerID, string buyerLocationID)
		{
            var buyerAddress = await oc.Addresses.GetAsync<HSAddressBuyer>(buyerID, buyerLocationID);
            var buyerUserGroup = await oc.UserGroups.GetAsync<HSLocationUserGroup>(buyerID, buyerLocationID);
			return new HSBuyerLocation 
			{
				Address = buyerAddress,
                UserGroup = buyerUserGroup,
			};
		}

		/// <summary>
		/// Public re-usable task method to create the HsBuyerLocation object for the buyerID
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="buyerLocation"></param>
		/// <returns>The newly created HsBuyerLocation object by the buyerID</returns>
		public async Task<HSBuyerLocation> Create(string buyerID, HSBuyerLocation buyerLocation)
		{
            return await Create(buyerID, buyerLocation, null, oc);
		}

		/// <summary>
		/// Public re-usable task method to create the HsBuyerLocation object for the buyerID
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="buyerLocation"></param>
		/// <param name="token"></param>
		/// <param name="ocClient"></param>
		/// <returns>The newly created HsBuyerLocation object by the buyerID</returns>
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

		/// <summary>
		/// Public re-usable CreateUserGroupAndAssignments task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="buyerLocationID"></param>
		/// <param name="token"></param>
		/// <param name="ocClient"></param>
		/// <returns></returns>
		public async Task CreateUserGroupAndAssignments(string buyerID, string buyerLocationID, string token, IOrderCloudClient ocClient)
		{
			var assignment = new AddressAssignment
			{
				AddressID = buyerLocationID,
				UserGroupID = buyerLocationID,
				IsBilling = true,
                IsShipping = true,
			};
			await ocClient.Addresses.SaveAssignmentAsync(buyerID, assignment, accessToken: token);
		}

		/// <summary>
		/// Public re-usable Save task method to update the HsBuyerLocation object data by the buyerId and buyerLocationID
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="buyerLocationID"></param>
		/// <param name="buyerLocation"></param>
		/// <returns>The newly updated HSBuyerLocation object by the buyerID and buyerLocationID</returns>
		public async Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, HSBuyerLocation buyerLocation)
		{
			// not being called by seed endpoint - use stored ordercloud client and stored admin token
            return await Save(buyerID, buyerLocationID, buyerLocation, null, oc);
		}

		/// <summary>
		/// Public re-usable Save task method to update the HsBuyerLocation object data by the buyerId and buyerLocationID
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="buyerLocationID"></param>
		/// <param name="buyerLocation"></param>
		/// <param name="token"></param>
		/// <param name="ocClient"></param>
		/// <returns>The newly updated HSBuyerLocation object by the buyerID and buyerLocationID</returns>
		public async Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, HSBuyerLocation buyerLocation, string token, IOrderCloudClient ocClient)
		{
			buyerLocation.Address.ID = buyerLocationID;
			buyerLocation.UserGroup.ID = buyerLocationID;
			UserGroup existingLocation = null;
			try
			{
				existingLocation = await ocClient.UserGroups.GetAsync(buyerID, buyerLocationID, token);
            }
            catch (Exception)
            {
            } // Do nothing if not found

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

		/// <summary>
		/// Public re-usable Delete task method to delete required Addresses and UserGroups for the specific buyerId and buyerLocationID
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="buyerLocationID"></param>
		/// <returns></returns>
		public async Task Delete(string buyerID, string buyerLocationID)
		{
            var deleteAddressReq = oc.Addresses.DeleteAsync(buyerID, buyerLocationID);
            var deleteUserGroupReq = oc.UserGroups.DeleteAsync(buyerID, buyerLocationID);
			await Task.WhenAll(deleteAddressReq, deleteUserGroupReq);
		}

		/// <summary>
		/// Public re-usable CreateLocationUserGroupsAndApprovalRule task method
		/// </summary>
		/// <param name="buyerLocationID"></param>
		/// <param name="locationName"></param>
		/// <param name="accessToken"></param>
		/// <param name="ocClient"></param>
		/// <returns></returns>
		public async Task CreateLocationUserGroupsAndApprovalRule(string buyerLocationID, string locationName, string accessToken, IOrderCloudClient ocClient)
		{
			var buyerID = buyerLocationID.Split('-').First();
            var addUserTypeRequests = HSUserTypes.BuyerLocation().Select(userType => AddUserTypeToLocation(buyerLocationID, userType, accessToken, ocClient));
            await Task.WhenAll(addUserTypeRequests);
			var isSeedingEnvironment = ocClient != null;
			if (!isSeedingEnvironment)
			{
				var approvingGroupID = $"{buyerLocationID}-{UserGroupSuffix.OrderApprover}";
				await ocClient.ApprovalRules.CreateAsync(buyerID, new ApprovalRule()
				{
					ID = buyerLocationID,
					ApprovingGroupID = approvingGroupID,
					Description = "General Approval Rule for Location. Every Order Over a Certain Limit will Require Approval for the designated group of users.",
					Name = $"{locationName} General Location Approval Rule",
                    RuleExpression = $"order.xp.ApprovalNeeded = '{buyerLocationID}' & order.Total > 0",
				});
			}
		}

		/// <summary>
		/// Public re-usable CreateSinglePermissionGroup task method
		/// </summary>
		/// <param name="buyerLocationID"></param>
		/// <param name="permissionGroupID"></param>
		/// <returns></returns>
		public async Task CreateSinglePermissionGroup(string buyerLocationID, string permissionGroupID)
		{
			var permissionGroup = HSUserTypes.BuyerLocation().Find(userType => permissionGroupID.Contains(userType.UserGroupIDSuffix));
			await AddUserTypeToLocation(buyerLocationID, permissionGroup);
		}

		/// <summary>
		/// Public re-usable AddUserTypeToLocation task method
		/// </summary>
		/// <param name="buyerLocationID"></param>
		/// <param name="hsUserType"></param>
		/// <returns></returns>
		public async Task AddUserTypeToLocation(string buyerLocationID, HSUserType hsUserType)
		{
			// not being called by seed endpoint - use stored ordercloud client and stored admin token
            await AddUserTypeToLocation(buyerLocationID, hsUserType, null, oc);
		}

		public async Task AddUserTypeToLocation(string buyerLocationID, HSUserType hsUserType, string accessToken, IOrderCloudClient oc)
		{
			// if we're seeding then use the passed in oc client
			// to support multiple environments and ease of setup for new orgs
			// else used the configured client
			var token = oc == null ? null : accessToken;
            var ocClient = oc ?? this.oc;

			var buyerID = buyerLocationID.Split('-').First();
			var userGroupID = $"{buyerLocationID}-{hsUserType.UserGroupIDSuffix}";
            await ocClient.UserGroups.CreateAsync(
                buyerID,
                new PartialUserGroup()
			{
				ID = userGroupID,
				Name = hsUserType.UserGroupName,
				xp = new
				{
					Role = hsUserType.UserGroupIDSuffix.ToString(),
					Type = hsUserType.UserGroupType,
                        Location = buyerLocationID,
                    },
                },
                token);
			foreach (var customRole in hsUserType.CustomRoles)
			{
                await ocClient.SecurityProfiles.SaveAssignmentAsync(
                    new SecurityProfileAssignment()
				{
					BuyerID = buyerID,
					UserGroupID = userGroupID,
                        SecurityProfileID = customRole.ToString(),
                    },
                    token);
			}
		}

		/// <summary>
		/// Public re-usable ReassignUserGroups task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="newUserID"></param>
		/// <returns></returns>
		public async Task ReassignUserGroups(string buyerID, string newUserID)
		{
            var userGroupAssignments = await oc.UserGroups.ListAllUserAssignmentsAsync(buyerID, userID: newUserID);
			await Throttler.RunAsync(userGroupAssignments, 100, 5, assignment => 
                RemoveAndAddUserGroupAssignment(buyerID, newUserID, assignment?.UserGroupID));
		}
		
		/// <summary>
		/// Private re-usable CreateBuyerLocationID method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="idInRequest"></param>
		/// <returns>The idInRequest string response for the </returns>
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

		/// <summary>
		/// Public re-usable ReassignUserGroups task method
		/// Temporary work around for a platform issue. When a new user is registered we need to 
		/// delete and reassign usergroup assignments for that user to view products
		/// issue: https://four51.atlassian.net/browse/EX-2222
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="newUserID"></param>
		/// <param name="userGroupID"></param>
		/// <returns></returns>
		private async Task RemoveAndAddUserGroupAssignment(string buyerID, string newUserID, string userGroupID)
		{
            await oc.UserGroups.DeleteUserAssignmentAsync(buyerID, userGroupID, newUserID);
            await oc.UserGroups.SaveUserAssignmentAsync(buyerID, new UserGroupAssignment
			{
				UserGroupID = userGroupID,
                UserID = newUserID,
			});
		}
	}
}
