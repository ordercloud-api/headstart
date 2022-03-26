using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface IHsCatalogCommand
	{
		Task<ListPage<HsCatalog>> List(string buyerId, ListArgs<HsCatalog> args, DecodedToken decodedToken);
		Task<HsCatalog> Post(string buyerId, HsCatalog catalog, DecodedToken decodedToken);
		Task<ListPage<HsCatalogAssignment>> GetAssignments(string buyerId, string locationId, DecodedToken decodedToken);
		Task SetAssignments(string buyerId, string locationId, List<string> assignments, string token);
		Task<HsCatalog> Get(string buyerId, string catalogId, DecodedToken decodedToken);
		Task<HsCatalog> Put(string buyerId, string catalogId, HsCatalog catalog, DecodedToken decodedToken);
		Task Delete(string buyerId, string catalogId, DecodedToken decodedToken);
		Task SyncUserCatalogAssignments(string buyerId, string userId);
	}

	public class HsCatalogCommand : IHsCatalogCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the HsCatalogCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="oc"></param>
		public HsCatalogCommand(AppSettings settings, IOrderCloudClient oc)
		{			
			try
			{
				_oc = oc;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable Get HsCatalog task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="catalogId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HsCatalog response object from the Get HsCatalog process</returns>
		public async Task<HsCatalog> Get(string buyerId, string catalogId, DecodedToken decodedToken)
		{
			var resp = new HsCatalog();
			try
			{
				resp = await _oc.UserGroups.GetAsync<HsCatalog>(buyerId, catalogId, decodedToken.AccessToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable get a list of ListPage of HsCatalog response objects task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="args"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPage of HsCatalog response objects</returns>
		public async Task<ListPage<HsCatalog>> List(string buyerId, ListArgs<HsCatalog> args, DecodedToken decodedToken)
		{
			var resp = new ListPage<HsCatalog>();
			try
			{
				resp = await _oc.UserGroups.ListAsync<HsCatalog>(buyerId, filters: $@"xp.Type=Catalog", search: args.Search, pageSize: args.PageSize, page: args.Page, accessToken: decodedToken.AccessToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable GetAssignments task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPage of HsCatalogAssignment response objects</returns>
		public async Task<ListPage<HsCatalogAssignment>> GetAssignments(string buyerId, string locationId, DecodedToken decodedToken)
		{
			var catalogAssignments = new List<HsCatalogAssignment> { };
			try
			{
				// assignments are stored on location usergroup xp in a string array with the Ids of the catalogs
				// currently they can only be assessed by location Id
				// limiting to 20 catalog assignments for now
				var location = await _oc.UserGroups.GetAsync<HsLocationUserGroup>(buyerId, locationId, decodedToken.AccessToken);
				if (location.xp.CatalogAssignments != null)
				{
					catalogAssignments = location.xp.CatalogAssignments.Select(catalogIdOnXp => new HsCatalogAssignment()
					{
						CatalogId = catalogIdOnXp,
						LocationId = locationId
					}).ToList();
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return catalogAssignments.ToListPage(page: 1, pageSize: 100);
		}

		/// <summary>
		/// Public re-usable SetAssignments task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="newAssignments"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task SetAssignments(string buyerId, string locationId, List<string> newAssignments, string token)
		{
			try
			{
				await _oc.UserGroups.PatchAsync(buyerId, locationId, new PartialUserGroup() { xp = new { CatalogAssignments = newAssignments } }, token);
				await UpdateUserCatalogAssignmentsForLocation(buyerId, locationId);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable SyncUserCatalogAssignments task method
		/// This function looks at all catalog-user-group Ids on the xp.CatalogAssignments array of all assigned BuyerLocation usergroups
		///	Then we add or remove usergroup assignments so the actual assignments allign with what is in the BuyerLocation usergroups
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task SyncUserCatalogAssignments(string buyerId, string userId)
		{
			try
			{
				// retrieve the data we'll need for further analysis
				var allUserAssignments = await _oc.UserGroups.ListAllUserAssignmentsAsync(buyerID: buyerId, userID: userId);
				var assignedGroupIds = allUserAssignments?.Select(assignment => assignment?.UserGroupID)?.ToList();
				var assignedGroups = await _oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerID: buyerId, filters: $"ID={string.Join("|", assignedGroupIds)}", pageSize: 100);
				var existingCatalogs = await _oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerId, filters: "xp.Type=Catalog", pageSize: 100);

				// from the data extract the relevant catalogIds
				var expectedAssignedCatalogIDs = assignedGroups.Items?.Where(item => (item?.xp?.Type == "BuyerLocation"))?.SelectMany(c => c?.xp?.CatalogAssignments);
				var actualAssignedCatalogIDs = assignedGroups.Items?.Where(item => item?.xp?.Type == "Catalog")?.Select(c => c.ID)?.ToList();
				var existingCatalogIDs = existingCatalogs.Items.Select(x => x.ID);

				// analyze list to determine the catalogIds to remove, and the list of catalogIds to add
				var assignmentsToRemove = actualAssignedCatalogIDs?.Where(id => !expectedAssignedCatalogIDs.Contains(id));
				var assignmentsToAdd = expectedAssignedCatalogIDs?.Where(id => !actualAssignedCatalogIDs.Contains(id) && existingCatalogIDs.Contains(id));

				// throttle the calls with a 100 millisecond wait in between so as not to overload the API
				await Throttler.RunAsync(assignmentsToRemove, 100, 5, catalogAssignmentToRemove =>
				{
					return _oc.UserGroups.DeleteUserAssignmentAsync(buyerId, catalogAssignmentToRemove, userId);
				});
				await Throttler.RunAsync(assignmentsToAdd, 100, 5, catalogAssignmentToAdd =>
				{
					return _oc.UserGroups.SaveUserAssignmentAsync(buyerId, new UserGroupAssignment()
					{
						UserGroupID = catalogAssignmentToAdd,
						UserID = userId
					});
				});
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable UpdateUserCatalogAssignmentsForLocation task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <returns></returns>
		private async Task UpdateUserCatalogAssignmentsForLocation(string buyerId, string locationId)
		{
			try
			{
				var users = await _oc.Users.ListAllAsync<HsUser>(buyerId, userGroupID: locationId);
				await Throttler.RunAsync(users, 100, 4, user =>
				{
					return SyncUserCatalogAssignments(buyerId, user.ID);
				});
			} 
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable Post task method for the UserGroups.CreateAsync process
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="catalog"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HsCatalog response object from the UserGroups.CreateAsync process</returns>
		public async Task<HsCatalog> Post(string buyerId, HsCatalog catalog, DecodedToken decodedToken)
		{
			var resp = new HsCatalog();
			try
			{
				resp = await _oc.UserGroups.CreateAsync<HsCatalog>(buyerId, catalog, decodedToken.AccessToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable Put task method for the UserGroups.CreateAsync process
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="catalogId"></param>
		/// <param name="catalog"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HsCatalog response object from the UserGroups.SaveAsync process</returns>
		public async Task<HsCatalog> Put(string buyerId, string catalogId, HsCatalog catalog, DecodedToken decodedToken)
		{
			return await _oc.UserGroups.SaveAsync<HsCatalog>(buyerId, catalogId, catalog, decodedToken.AccessToken);
		}

		/// <summary>
		/// Public re-usable Delete task method for the UserGroups.DeleteAsync process
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="catalogId"></param>
		/// <param name="decodedToken"></param>
		/// <returns></returns>
		public async Task Delete(string buyerId, string catalogId, DecodedToken decodedToken)
		{
			await _oc.UserGroups.DeleteAsync(buyerId, catalogId, decodedToken.AccessToken);
		}
	}
}