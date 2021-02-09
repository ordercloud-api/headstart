using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.API.Controllers;
using Headstart.API.Commands.Crud;

namespace Headstart.Common.Controllers
{
	[DocComments("\"Headstart Catalogs\" for product groupings and visibility in Headstart")]
	[HSSection.Headstart(ListOrder = 2)]
	[Route("buyers")]
	public class CatalogController : BaseController
	{

		private readonly IHSCatalogCommand _command;
		public CatalogController(IHSCatalogCommand command, AppSettings settings) : base(settings)
		{
			_command = command;
		}

		[DocName("GET a list of Catalogs")]
		[HttpGet, Route("{buyerID}/catalogs"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<HSCatalog>> List(ListArgs<HSCatalog> args, string buyerID)
		{
			return await _command.List(buyerID, args, VerifiedUserContext);
		}

		[DocName("GET a single Catalog")]
		[HttpGet, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<HSCatalog> Get(string buyerID, string catalogID)
		{
			return await _command.Get(buyerID, catalogID, VerifiedUserContext);
		}

		[DocName("Create a new Catalog")]
		[HttpPost, Route("{buyerID}/catalogs"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Post([FromBody] HSCatalog obj, string buyerID)
		{
			return await _command.Post(buyerID, obj, VerifiedUserContext);
		}

		[DocName("Get a list of catalog location assignments")]
		[HttpGet, Route("{buyerID}/catalogs/assignments"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerID, [FromQuery(Name = "catalogID")] string catalogID = "", [FromQuery(Name = "locationID")] string locationID = "")
		{
			return await _command.GetAssignments(buyerID, locationID, VerifiedUserContext);
		}

		[DocName("Set catalog assignments for a location")]
		[HttpPost, Route("{buyerID}/{locationID}/catalogs/assignments"), OrderCloudIntegrationsAuth(ApiRole.UserGroupAdmin)]
		public async Task SetAssignments(string buyerID, string locationID, [FromBody] HSCatalogAssignmentRequest assignmentRequest)
		{
			await _command.SetAssignments(buyerID, locationID, assignmentRequest.CatalogIDs, VerifiedUserContext.AccessToken);
		}

		[DocName("PUT Catalog")]
		[HttpPut, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Put([FromBody] HSCatalog obj, string buyerID, string catalogID)
		{
			return await _command.Put(buyerID, catalogID, obj, this.VerifiedUserContext);
		}

		[DocName("DELETE Catalog")]
		[HttpDelete, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task Delete(string buyerID, string catalogID)
		{
			await _command.Delete(buyerID, catalogID, VerifiedUserContext);
		}

        [DocName("SYNC User Catalogs Assignments")]
        [HttpPost, Route("{buyerID}/catalogs/user/{userID}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        public async Task SyncOnRemoveFromLocation(string buyerID, string userID)
        {
			await _command.SyncUserCatalogAssignments(buyerID, userID);
        }
    }
}
