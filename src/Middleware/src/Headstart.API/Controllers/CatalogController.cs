using Headstart.API.Commands;
using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System.Threading.Tasks;

namespace Headstart.API.Controllers
{
	[Route("buyers")]
	public class CatalogController : CatalystController
	{
		private readonly IHSCatalogCommand _command;

		/// <summary>
		/// The IOC based constructor method for the CatalogController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		public CatalogController(IHSCatalogCommand command)
		{
			_command = command;
		}

		/// <summary>
		/// Gets the ListPage of HsCatalog objects by buyerId (GET method)
		/// </summary>
		/// <param name="args"></param>
		/// <param name="buyerId"></param>
		/// <returns>The ListPage of HSCatalog objects by buyerId</returns>
		[HttpGet, Route("{buyerId}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<HSCatalog>> List(ListArgs<HSCatalog> args, string buyerId)
		{
			return await _command.List(buyerId, args, UserContext);
		}

		/// <summary>
		/// Gets the single catalog by buyerId (GET method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="catalogId"></param>
		/// <returns>The single HSCatalog by buyerId</returns>
		[HttpGet, Route("{buyerId}/catalogs/{catalogId}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<HSCatalog> Get(string buyerId, string catalogId)
		{
			return await _command.Get(buyerId, catalogId, UserContext);
		}

		/// <summary>
		/// Creates a new Catalog by buyerId (POST method)
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="buyerId"></param>
		/// <returns>The newly created HSCatalog by buyerId</returns>
		[HttpPost, Route("{buyerId}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Post([FromBody] HSCatalog obj, string buyerId)
		{
			return await _command.Post(buyerId, obj, UserContext);
		}

		/// <summary>
		/// Gets the ListPage of Catalog location assignments objects (GET method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="catalogId"></param>
		/// <param name="locationId"></param>
		/// <returns>The ListPage of HSCatalogAssignment objects</returns>
		[HttpGet, Route("{buyerId}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerId, [FromQuery(Name = "catalogId")] string catalogId = "", [FromQuery(Name = "locationId")] string locationId = "")
		{
			return await _command.GetAssignments(buyerId, locationId, UserContext);
		}

		/// <summary>
		/// Sets the catalog assignments for a Buyer location (POST method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="locationId"></param>
		/// <param name="assignmentRequest"></param>
		/// <returns></returns>
		[HttpPost, Route("{buyerId}/{locationId}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
		public async Task SetAssignments(string buyerId, string locationId, [FromBody] HSCatalogAssignmentRequest assignmentRequest)
		{
			await _command.SetAssignments(buyerId, locationId, assignmentRequest.CatalogIDs, UserContext.AccessToken);
		}

		/// <summary>
		/// Updates the HsCatalog for a Buyer (PUT method)
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="buyerId"></param>
		/// <param name="catalogId"></param>
		/// <returns>The HSCatalog data by buyerId</returns>
		[HttpPut, Route("{buyerId}/catalogs/{catalogId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSCatalog> Put([FromBody] HSCatalog obj, string buyerId, string catalogId)
		{
			return await _command.Put(buyerId, catalogId, obj, UserContext);
		}

		/// <summary>
		/// Removes/Deletes the Catalog object (DELETE method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="catalogId"></param>
		/// <returns></returns>
		[HttpDelete, Route("{buyerId}/catalogs/{catalogId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task Delete(string buyerId, string catalogId)
		{
			await _command.Delete(buyerId, catalogId, UserContext);
		}

		/// <summary>
		/// Synchronizing User Catalogs Assignments (POST method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		[HttpPost, Route("{buyerId}/catalogs/user/{userId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task SyncOnRemoveFromLocation(string buyerId, string userId)
		{
			await _command.SyncUserCatalogAssignments(buyerId, userId);
		}
	}
}