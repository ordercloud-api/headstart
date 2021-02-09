using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Models;
using ordercloud.integrations.library;
using ordercloud.integrations.library.helpers;
using OrderCloud.SDK;

namespace Headstart.API.Commands.Crud
{
	public interface IHSCatalogCommand
	{
		Task<ListPage<HSCatalog>> List(string buyerID, ListArgs<HSCatalog> args, VerifiedUserContext user);
		Task<HSCatalog> Post(string buyerID, HSCatalog catalog, VerifiedUserContext user);
		Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerID, string locationID, VerifiedUserContext user);
		Task SetAssignments(string buyerID, string locationID, List<string> assignments, string token);
		Task<HSCatalog> Get(string buyerID, string catalogID, VerifiedUserContext user);
		Task<HSCatalog> Put(string buyerID, string catalogID, HSCatalog catalog, VerifiedUserContext user);
		Task Delete(string buyerID, string catalogID, VerifiedUserContext user);
		Task SyncUserCatalogAssignments(string buyerID, string userID);
	}

	public class HSCatalogCommand : IHSCatalogCommand
	{
		private readonly IOrderCloudClient _oc;
		public HSCatalogCommand(AppSettings settings, IOrderCloudClient oc)
		{
			_oc = oc;
		}

		public async Task<HSCatalog> Get(string buyerID, string catalogID, VerifiedUserContext user)
		{
			return await _oc.UserGroups.GetAsync<HSCatalog>(buyerID, catalogID, user.AccessToken);
		}

		public async Task<ListPage<HSCatalog>> List(string buyerID, ListArgs<HSCatalog> args, VerifiedUserContext user)
		{
			var queryParamsForCatalogUserGroup = new Tuple<string, string>("xp.Type", "Catalog");
			args.Filters.Add(new ListFilter()
			{
				QueryParams = new List<Tuple<string, string>> { queryParamsForCatalogUserGroup }
			});
			return await _oc.UserGroups.ListAsync<HSCatalog>(buyerID, filters: args.ToFilterString(),
				search: args.Search,
				pageSize: args.PageSize,
				page: args.Page,
				accessToken: user.AccessToken);
		}
		
		public async Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerID, string locationID, VerifiedUserContext user)
		{
			// assignments are stored on location usergroup xp in a string array with the ids of the catalogs
			// currently they can only be assessed by location ID
			// limiting to 20 catalog assignments for now

			var location = await _oc.UserGroups.GetAsync<HSLocationUserGroup>(buyerID, locationID, user.AccessToken);

			var catalogAssignments = new List<HSCatalogAssignment>{};
			
			if(location.xp.CatalogAssignments != null)
			{
				catalogAssignments = location.xp.CatalogAssignments.Select(catalogIDOnXp => new HSCatalogAssignment()
					{
						CatalogID = catalogIDOnXp,
						LocationID = locationID
					}).ToList();
			}

			return catalogAssignments.ToListPage(page: 1, pageSize: 100);
		}

		public async Task SetAssignments(string buyerID, string locationID, List<string> newAssignments, string token)
		{
			await _oc.UserGroups.PatchAsync(buyerID, locationID, new PartialUserGroup() { xp = new { CatalogAssignments = newAssignments } }, token);
			await UpdateUserCatalogAssignmentsForLocation(buyerID, locationID);
		}

		//	This function looks at all catalog-user-group ids on the xp.CatalogAssignments array of all assigned BuyerLocation usergroups
		//	Then we add or remove usergroup assignments so the actual assignments allign with what is in the BuyerLocation usergroups
		public async Task SyncUserCatalogAssignments(string buyerID, string userID)
        {
			var currentAssignments = await ListAllAsync.List((page) => _oc.UserGroups.ListUserAssignmentsAsync(buyerID: buyerID, userID: userID, page: page, pageSize: 100));
			var currentAssignedCatalogIDs = currentAssignments?.Select(assignment => assignment?.UserGroupID)?.ToList();
			var currentUserGroups = await _oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID: buyerID, filters: $"ID={string.Join("|", currentAssignedCatalogIDs)}");
			var catalogsUserShouldSee = currentUserGroups?.Items?.Where(item => (item?.xp?.Type == "BuyerLocation"))?.SelectMany(c => c?.xp?.CatalogAssignments);

			var actualCatalogAssignments = currentUserGroups?.Items?.Where(item => item?.xp?.Type == "Catalog")?.Select(c => c.ID)?.ToList();
			//now remove all actualCatalogAssignments that are not included in catalogsUserShouldSee
			var assignmentsToRemove = actualCatalogAssignments?.Where(id => !catalogsUserShouldSee.Contains(id));
			var assignmentsToAdd = catalogsUserShouldSee?.Where(id => !actualCatalogAssignments.Contains(id));
			await Throttler.RunAsync(assignmentsToRemove, 100, 5, catalogAssignmentToRemove =>
			{
				return _oc.UserGroups.DeleteUserAssignmentAsync(buyerID, catalogAssignmentToRemove, userID);
			});
			await Throttler.RunAsync(assignmentsToAdd, 100, 5, catalogAssignmentToAdd =>
			{
				return _oc.UserGroups.SaveUserAssignmentAsync(buyerID, new UserGroupAssignment()
                {
					UserGroupID = catalogAssignmentToAdd,
					UserID = userID
                });
			});
		}

		private async Task UpdateUserCatalogAssignmentsForLocation(string buyerID, string locationID)
		{
			try
			{
				var users = await ListAllAsync.List((page) => _oc.Users.ListAsync<HSUser>(buyerID, userGroupID: locationID, page: page, pageSize: 100));
				await Throttler.RunAsync(users, 100, 4, user =>
				{
					return SyncUserCatalogAssignments(buyerID, user.ID);
				});
			} catch (Exception ex)
			{
				Console.WriteLine(ex.Message);	
			}

		}

		public async Task<HSCatalog> Post(string buyerID, HSCatalog catalog, VerifiedUserContext user)
		{
			return await _oc.UserGroups.CreateAsync<HSCatalog>(buyerID, catalog, user.AccessToken);
		}

		public async Task<HSCatalog> Put(string buyerID, string catalogID, HSCatalog catalog, VerifiedUserContext user)
		{
			return await _oc.UserGroups.SaveAsync<HSCatalog>(buyerID, catalogID, catalog, user.AccessToken);
		}

		public async Task Delete(string buyerID, string catalogID, VerifiedUserContext user)
		{
			await _oc.UserGroups.DeleteAsync(buyerID, catalogID, user.AccessToken);
		}
	}
}
