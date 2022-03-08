using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.API.Commands.Crud;

namespace Headstart.API.Controllers
{
    [Route("buyers")]
    public class CatalogController : CatalystController
    {
        private readonly IHSCatalogCommand _hsCatalogCommand;

        /// <summary>
        /// The IOC based constructor method for the CatalogController class object with Dependency Injection
        /// </summary>
        /// <param name="command"></param>
        public CatalogController(IHSCatalogCommand command)
        {
            _hsCatalogCommand = command;
        }

        /// <summary>
        /// Gets the ListPage of HSCatalog objects by buyerId (GET method)
        /// </summary>
        /// <param name="args"></param>
        /// <param name="buyerId"></param>
        /// <returns>The ListPage of HSCatalog objects by buyerId</returns>
        [HttpGet, Route("{buyerId}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
        public async Task<ListPage<HSCatalog>> List(ListArgs<HSCatalog> args, string buyerId)
        {
            return await _hsCatalogCommand.List(buyerId, args, UserContext);
        }

        /// <summary>
        /// Gets the single catalog by buyerId (GET method)
        /// </summary>
        /// <param name="buyerId"></param>
        /// <param name="catalogId"></param>
        /// <returns>The single catalog by buyerId</returns>
        [HttpGet, Route("{buyerId}/catalogs/{catalogId}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
        public async Task<HSCatalog> Get(string buyerId, string catalogId)
        {
            return await _hsCatalogCommand.Get(buyerId, catalogId, UserContext);
        }

        /// <summary>
        /// Creates a new Catalog by buyerId (POST method)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="buyerId"></param>
        /// <returns>The newly created Catalog by buyerId</returns>
        [HttpPost, Route("{buyerId}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<HSCatalog> Post([FromBody] HSCatalog obj, string buyerId)
        {
            return await _hsCatalogCommand.Post(buyerId, obj, UserContext);
        }

        /// <summary>
        /// Gets the ListPage of Catalog location assignments objects (GET method)
        /// </summary>
        /// <param name="buyerId"></param>
        /// <param name="catalogId"></param>
        /// <param name="locationId"></param>
        /// <returns>The ListPage of Catalog location assignment objects</returns>
        [HttpGet, Route("{buyerId}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<ListPage<HSCatalogAssignment>> GetAssignments(string buyerId, [FromQuery(Name = "catalogId")] string catalogId = "", [FromQuery(Name = "locationId")] string locationId = "")
        {
            return await _hsCatalogCommand.GetAssignments(buyerId, locationId, UserContext);
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
            await _hsCatalogCommand.SetAssignments(buyerId, locationId, assignmentRequest.CatalogIDs, UserContext.AccessToken);
        }

        /// <summary>
        /// Updates the catalog for a Buyer (PUT method)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="buyerId"></param>
        /// <param name="catalogId"></param>
        /// <returns>The catalog data by buyerId</returns>
        [HttpPut, Route("{buyerId}/catalogs/{catalogId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<HSCatalog> Put([FromBody] HSCatalog obj, string buyerId, string catalogId)
        {
            return await _hsCatalogCommand.Put(buyerId, catalogId, obj, UserContext);
        }


        /// <summary>
        /// Removes/Deletes the Catelog object (DELETE method)
        /// </summary>
        /// <param name="buyerId"></param>
        /// <param name="catalogId"></param>
        /// <returns></returns>
        [HttpDelete, Route("{buyerId}/catalogs/{catalogId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task Delete(string buyerId, string catalogId)
        {
            await _hsCatalogCommand.Delete(buyerId, catalogId, UserContext);
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
			await _hsCatalogCommand.SyncUserCatalogAssignments(buyerId, userId);
        }
    }
}