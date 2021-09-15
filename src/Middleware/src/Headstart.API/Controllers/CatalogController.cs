using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.API.Commands.Crud;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
	/// <summary>
	/// Catalogs for product groupings and visibility in Headstart
	/// </summary>
	[Route("buyers")]
	public class CatalogController : CatalystController
	{

		private readonly IHSCatalogCommand _command;
		public CatalogController(IHSCatalogCommand command)
		{
			_command = command;
		}

		/// <summary>
		/// GET a list of Catalogs
		/// </summary>
		[HttpGet, Route("{buyerID}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<HSCatalog>> List(ListArgs<HSCatalog> args, string buyerID)
		{
			return await _command.List(buyerID, args, UserContext);
		}

		/// <summary>
		/// GET a single Catalog
		/// </summary>
		[HttpGet, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<HSCatalog> Get(string buyerID, string catalogID)
		{
			return await _command.Get(buyerID, catalogID, UserContext);
		}

		/// <summary>
		/// Create a new Catalog
		/// </summary>
		[HttpPost, Route("{buyerID}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Post([FromBody] HSCatalog obj, string buyerID)
		{
			return await _command.Post(buyerID, obj, UserContext);
		}

		/// <summary>
		/// Get a list of catalog location assignments
		/// </summary>
		[HttpGet, Route("{buyerID}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerID, [FromQuery(Name = "catalogID")] string catalogID = "", [FromQuery(Name = "locationID")] string locationID = "")
		{
			return await _command.GetAssignments(buyerID, locationID, UserContext);
		}

		/// <summary>
		/// Set catalog assignments for a location
		/// </summary>
		[HttpPost, Route("{buyerID}/{locationID}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
		public async Task SetAssignments(string buyerID, string locationID, [FromBody] HSCatalogAssignmentRequest assignmentRequest)
		{
			await _command.SetAssignments(buyerID, locationID, assignmentRequest.CatalogIDs, UserContext.AccessToken);
		}

		/// <summary>
		/// PUT Catalog
		/// </summary>
		[HttpPut, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Put([FromBody] HSCatalog obj, string buyerID, string catalogID)
		{
			return await _command.Put(buyerID, catalogID, obj, UserContext);
		}

		/// <summary>
		/// DELETE Catalog
		/// </summary>
		[HttpDelete, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task Delete(string buyerID, string catalogID)
		{
			await _command.Delete(buyerID, catalogID, UserContext);
		}

		/// <summary>
		/// SYNC User Catalogs Assignments
		/// </summary>
        [HttpPost, Route("{buyerID}/catalogs/user/{userID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task SyncOnRemoveFromLocation(string buyerID, string userID)
        {
			await _command.SyncUserCatalogAssignments(buyerID, userID);
        }
    }
}
