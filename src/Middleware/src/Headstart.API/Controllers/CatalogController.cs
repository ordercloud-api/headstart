using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.API.Commands.Crud;

namespace Headstart.Common.Controllers
{
	[Route("buyers")]
	public class CatalogController : CatalystController
	{

		private readonly IHSCatalogCommand _command;

		/// <summary>
		/// The IOC based constructor method for the CatalogController with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		public CatalogController(IHSCatalogCommand command)
		{
			_command = command;
		}

		/// <summary>
		/// Gets the ListPage of HSCatalog objects by buyerID (GET method)
		/// </summary>
		/// <param name="args"></param>
		/// <param name="buyerID"></param>
		/// <returns>The ListPage of HSCatalog objects by buyerID</returns>
		[HttpGet, Route("{buyerID}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<HSCatalog>> List(ListArgs<HSCatalog> args, string buyerID)
		{
			return await _command.List(buyerID, args, UserContext);
		}

		/// <summary>
		/// Gets the single catalog by buyerID (GET method)
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="catalogID"></param>
		/// <returns>The single catalog by buyerID</returns>
		[HttpGet, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<HSCatalog> Get(string buyerID, string catalogID)
		{
			return await _command.Get(buyerID, catalogID, UserContext);
		}

		/// <summary>
		/// Creates a new Catalog by buyerID (POST method)
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="buyerID"></param>
		/// <returns>The newly created Catalog by buyerID</returns>
		[HttpPost, Route("{buyerID}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Post([FromBody] HSCatalog obj, string buyerID)
		{
			return await _command.Post(buyerID, obj, UserContext);
		}

		/// <summary>
		/// Gets the ListPage of Catalog location assignments objects (GET method)
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="catalogID"></param>
		/// <param name="locationID"></param>
		/// <returns>The ListPage of Catalog location assignment objects</returns>
		[HttpGet, Route("{buyerID}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerID, [FromQuery(Name = "catalogID")] string catalogID = "", [FromQuery(Name = "locationID")] string locationID = "")
		{
			return await _command.GetAssignments(buyerID, locationID, UserContext);
		}

		/// <summary>
		/// Sets the catalog assignments for a Buyer location (POST method)
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="locationID"></param>
		/// <param name="assignmentRequest"></param>
		/// <returns></returns>
		[HttpPost, Route("{buyerID}/{locationID}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
		public async Task SetAssignments(string buyerID, string locationID, [FromBody] HSCatalogAssignmentRequest assignmentRequest)
		{
			await _command.SetAssignments(buyerID, locationID, assignmentRequest.CatalogIDs, UserContext.AccessToken);
		}

		/// <summary>
		/// Updates the catalog for a Buyer (PUT method)
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="buyerID"></param>
		/// <param name="catalogID"></param>
		/// <returns>The catalog data by buyerID</returns>
		[HttpPut, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Put([FromBody] HSCatalog obj, string buyerID, string catalogID)
		{
			return await _command.Put(buyerID, catalogID, obj, UserContext);
		}


		/// <summary>
		/// Removes/Deletes the Catelog object (DELETE method)
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="catalogID"></param>
		/// <returns></returns>
		[HttpDelete, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task Delete(string buyerID, string catalogID)
		{
			await _command.Delete(buyerID, catalogID, UserContext);
		}

		/// <summary>
		/// Synchronizing User Catalogs Assignments (POST method)
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="userID"></param>
		/// <returns></returns>
		[HttpPost, Route("{buyerID}/catalogs/user/{userID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task SyncOnRemoveFromLocation(string buyerID, string userID)
        {
			await _command.SyncUserCatalogAssignments(buyerID, userID);
        }
    }
}