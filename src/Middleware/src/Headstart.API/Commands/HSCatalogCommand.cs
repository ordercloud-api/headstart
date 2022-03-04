using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using Headstart.Models;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using ordercloud.integrations.library;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extenstions;

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
		private WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the HSCatalogCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="oc"></param>
		public HSCatalogCommand(AppSettings settings, IOrderCloudClient oc)
		{			
			try
			{
				_oc = oc;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable Get HSCatalog task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="catalogID"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HSCatalog response object from the Get HSCatalog process</returns>
		public async Task<HSCatalog> Get(string buyerID, string catalogID, DecodedToken decodedToken)
		{
			var resp = new HSCatalog();
			try
			{
				resp = await _oc.UserGroups.GetAsync<HSCatalog>(buyerID, catalogID, decodedToken.AccessToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable get a list of ListPage of HSCatalog response objects task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="args"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPage of HSCatalog response objects</returns>
		public async Task<ListPage<HSCatalog>> List(string buyerID, ListArgs<HSCatalog> args, DecodedToken decodedToken)
		{
			var resp = new ListPage<HSCatalog>();
			try
			{
				resp = await _oc.UserGroups.ListAsync<HSCatalog>(buyerID, filters: $@"xp.Type=Catalog", search: args.Search, pageSize: args.PageSize, page: args.Page, accessToken: decodedToken.AccessToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable GetAssignments task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="locationID"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPage of HSCatalogAssignment response objects</returns>
		public async Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerID, string locationID, DecodedToken decodedToken)
		{
			var catalogAssignments = new List<HSCatalogAssignment> { };
			try
			{
				// assignments are stored on location usergroup xp in a string array with the ids of the catalogs
				// currently they can only be assessed by location ID
				// limiting to 20 catalog assignments for now
				var location = await _oc.UserGroups.GetAsync<HSLocationUserGroup>(buyerID, locationID, decodedToken.AccessToken);
				if (location.xp.CatalogAssignments != null)
				{
					catalogAssignments = location.xp.CatalogAssignments.Select(catalogIDOnXp => new HSCatalogAssignment()
					{
						CatalogID = catalogIDOnXp,
						LocationID = locationID
					}).ToList();
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return catalogAssignments.ToListPage(page: 1, pageSize: 100);
		}

		/// <summary>
		/// Public re-usable SetAssignments task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="locationID"></param>
		/// <param name="newAssignments"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task SetAssignments(string buyerID, string locationID, List<string> newAssignments, string token)
		{
			try
			{
				await _oc.UserGroups.PatchAsync(buyerID, locationID, new PartialUserGroup() { xp = new { CatalogAssignments = newAssignments } }, token);
				await UpdateUserCatalogAssignmentsForLocation(buyerID, locationID);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable SyncUserCatalogAssignments task method
		/// This function looks at all catalog-user-group ids on the xp.CatalogAssignments array of all assigned BuyerLocation usergroups
		///	Then we add or remove usergroup assignments so the actual assignments allign with what is in the BuyerLocation usergroups
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="userID"></param>
		/// <returns></returns>
		public async Task SyncUserCatalogAssignments(string buyerID, string userID)
        {
			try
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
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable UpdateUserCatalogAssignmentsForLocation task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="locationID"></param>
		/// <returns></returns>
		private async Task UpdateUserCatalogAssignmentsForLocation(string buyerID, string locationID)
		{
			try
			{
				var users = await _oc.Users.ListAllAsync<HSUser>(buyerID, userGroupID: locationID);
				await Throttler.RunAsync(users, 100, 4, user =>
				{
					return SyncUserCatalogAssignments(buyerID, user.ID);
				});
			} 
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable Post task method for the UserGroups.CreateAsync process
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="catalog"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HSCatalog response object from the UserGroups.CreateAsync process</returns>
		public async Task<HSCatalog> Post(string buyerID, HSCatalog catalog, DecodedToken decodedToken)
		{
			var resp = new HSCatalog();
			try
			{
				resp = await _oc.UserGroups.CreateAsync<HSCatalog>(buyerID, catalog, decodedToken.AccessToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable Put task method for the UserGroups.CreateAsync process
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="catalogID"></param>
		/// <param name="catalog"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HSCatalog response object from the UserGroups.SaveAsync process</returns>
		public async Task<HSCatalog> Put(string buyerID, string catalogID, HSCatalog catalog, DecodedToken decodedToken)
		{
			return await _oc.UserGroups.SaveAsync<HSCatalog>(buyerID, catalogID, catalog, decodedToken.AccessToken);
		}

		/// <summary>
		/// Public re-usable Delete task method for the UserGroups.DeleteAsync process
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="catalogID"></param>
		/// <param name="decodedToken"></param>
		/// <returns></returns>
		public async Task Delete(string buyerID, string catalogID, DecodedToken decodedToken)
		{
			await _oc.UserGroups.DeleteAsync(buyerID, catalogID, decodedToken.AccessToken);
		}
	}
}