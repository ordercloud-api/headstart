using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Models;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.API.Commands.Crud
{
	public interface IHSCatalogCommand
	{
		Task<ListPage<HSCatalog>> List(string buyerID, ListArgs<HSCatalog> args, DecodedToken decodedToken);
		Task<HSCatalog> Post(string buyerID, HSCatalog catalog, DecodedToken decodedToken);
		Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerID, string locationID, DecodedToken decodedToken);
		Task SetAssignments(string buyerID, string locationID, List<string> assignments, string token);
		Task<HSCatalog> Get(string buyerID, string catalogID, DecodedToken decodedToken);
		Task<HSCatalog> Put(string buyerID, string catalogID, HSCatalog catalog, DecodedToken decodedToken);
		Task Delete(string buyerID, string catalogID, DecodedToken decodedToken);
		Task SyncUserCatalogAssignments(string buyerID, string userID);
	}

	public class HSCatalogCommand : IHSCatalogCommand
	{
		private readonly IOrderCloudClient _oc;
		public HSCatalogCommand(AppSettings settings, IOrderCloudClient oc)
		{
			_oc = oc;
		}

		public async Task<HSCatalog> Get(string buyerID, string catalogID, DecodedToken decodedToken)
		{
			return await _oc.UserGroups.GetAsync<HSCatalog>(buyerID, catalogID, decodedToken.AccessToken);
		}

		public async Task<ListPage<HSCatalog>> List(string buyerID, ListArgs<HSCatalog> args, DecodedToken decodedToken)
		{
			return await _oc.UserGroups.ListAsync<HSCatalog>(buyerID, 
				filters: "xp.Type=Catalog",
				search: args.Search,
				pageSize: args.PageSize,
				page: args.Page,
				accessToken: decodedToken.AccessToken);
		}
		
		public async Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerID, string locationID, DecodedToken decodedToken)
		{
			// assignments are stored on location usergroup xp in a string array with the ids of the catalogs
			// currently they can only be assessed by location ID
			// limiting to 20 catalog assignments for now

			var location = await _oc.UserGroups.GetAsync<HSLocationUserGroup>(buyerID, locationID, decodedToken.AccessToken);

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
			// retrieve the data we'll need for further analysis
			var allUserAssignments = await _oc.UserGroups.ListAllUserAssignmentsAsync(buyerID: buyerID, userID: userID);
			var assignedGroupIDs = allUserAssignments?.Select(assignment => assignment?.UserGroupID)?.ToList();
			var assignedGroups = await _oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID: buyerID, filters: $"ID={string.Join("|", assignedGroupIDs)}", pageSize: 100);
			var existingCatalogs = await _oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID, filters: "xp.Type=Catalog", pageSize: 100);
			
			// from the data extract the relevant catalogIDs
			var expectedAssignedCatalogIDs = assignedGroups.Items?.Where(item => (item?.xp?.Type == "BuyerLocation"))?.SelectMany(c => c?.xp?.CatalogAssignments);
			var actualAssignedCatalogIDs = assignedGroups.Items?.Where(item => item?.xp?.Type == "Catalog")?.Select(c => c.ID)?.ToList();
			var existingCatalogIDs = existingCatalogs.Items.Select(x => x.ID);

			// analyze list to determine the catalogids to remove, and the list of catalogids to add
			var assignmentsToRemove = actualAssignedCatalogIDs?.Where(id => !expectedAssignedCatalogIDs.Contains(id));
			var assignmentsToAdd = expectedAssignedCatalogIDs?.Where(id => !actualAssignedCatalogIDs.Contains(id) && existingCatalogIDs.Contains(id));

			// throttle the calls with a 100 millisecond wait in between so as not to overload the API
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
				var users = await _oc.Users.ListAllAsync<HSUser>(buyerID, userGroupID: locationID);
				await Throttler.RunAsync(users, 100, 4, user =>
				{
					return SyncUserCatalogAssignments(buyerID, user.ID);
				});
			} catch (Exception ex)
			{
				Console.WriteLine(ex.Message);	
			}

		}

		public async Task<HSCatalog> Post(string buyerID, HSCatalog catalog, DecodedToken decodedToken)
		{
			return await _oc.UserGroups.CreateAsync<HSCatalog>(buyerID, catalog, decodedToken.AccessToken);
		}

		public async Task<HSCatalog> Put(string buyerID, string catalogID, HSCatalog catalog, DecodedToken decodedToken)
		{
			return await _oc.UserGroups.SaveAsync<HSCatalog>(buyerID, catalogID, catalog, decodedToken.AccessToken);
		}

		public async Task Delete(string buyerID, string catalogID, DecodedToken decodedToken)
		{
			await _oc.UserGroups.DeleteAsync(buyerID, catalogID, decodedToken.AccessToken);
		}
	}
}
