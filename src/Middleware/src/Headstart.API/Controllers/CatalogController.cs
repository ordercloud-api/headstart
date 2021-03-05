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
	[DocComments("\"Headstart Catalogs\" for product groupings and visibility in Headstart")]
	[HSSection.Headstart(ListOrder = 2)]
	[Route("buyers")]
	public class CatalogController : BaseController
	{

		private readonly IHSCatalogCommand _command;
		public CatalogController(IHSCatalogCommand command)
		{
			_command = command;
		}

		[DocName("GET a list of Catalogs")]
		[HttpGet, Route("{buyerID}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<HSCatalog>> List(ListArgs<HSCatalog> args, string buyerID)
		{
			return await _command.List(buyerID, args, UserContext);
		}

		[DocName("GET a single Catalog")]
		[HttpGet, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<HSCatalog> Get(string buyerID, string catalogID)
		{
			return await _command.Get(buyerID, catalogID, UserContext);
		}

		[DocName("Create a new Catalog")]
		[HttpPost, Route("{buyerID}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Post([FromBody] HSCatalog obj, string buyerID)
		{
			return await _command.Post(buyerID, obj, UserContext);
		}

		[DocName("Get a list of catalog location assignments")]
		[HttpGet, Route("{buyerID}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerID, [FromQuery(Name = "catalogID")] string catalogID = "", [FromQuery(Name = "locationID")] string locationID = "")
		{
			return await _command.GetAssignments(buyerID, locationID, UserContext);
		}

		[DocName("Set catalog assignments for a location")]
		[HttpPost, Route("{buyerID}/{locationID}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
		public async Task SetAssignments(string buyerID, string locationID, [FromBody] HSCatalogAssignmentRequest assignmentRequest)
		{
			await _command.SetAssignments(buyerID, locationID, assignmentRequest.CatalogIDs, UserContext.AccessToken);
		}

		[DocName("PUT Catalog")]
		[HttpPut, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Put([FromBody] HSCatalog obj, string buyerID, string catalogID)
		{
			return await _command.Put(buyerID, catalogID, obj, UserContext);
		}

		[DocName("DELETE Catalog")]
		[HttpDelete, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task Delete(string buyerID, string catalogID)
		{
			await _command.Delete(buyerID, catalogID, UserContext);
		}

        [DocName("SYNC User Catalogs Assignments")]
        [HttpPost, Route("{buyerID}/catalogs/user/{userID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task SyncOnRemoveFromLocation(string buyerID, string userID)
        {
			await _command.SyncUserCatalogAssignments(buyerID, userID);
        }
    }
}
