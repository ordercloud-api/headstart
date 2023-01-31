using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// Catalogs for product groupings and visibility in Headstart.
    /// </summary>
    [Route("buyers")]
    public class CatalogController : CatalystController
    {
        private readonly ICatalogCommand catalogCommand;

        public CatalogController(ICatalogCommand catalogCommand)
        {
            this.catalogCommand = catalogCommand;
        }

        /// <summary>
        /// GET a list of Catalogs.
        /// </summary>
        [HttpGet, Route("{buyerID}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
        public async Task<ListPage<HSCatalog>> List(ListArgs<HSCatalog> args, string buyerID)
        {
            return await catalogCommand.List(buyerID, args, UserContext);
        }

        /// <summary>
        /// GET a single Catalog.
        /// </summary>
        [HttpGet, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
        public async Task<HSCatalog> Get(string buyerID, string catalogID)
        {
            return await catalogCommand.Get(buyerID, catalogID, UserContext);
        }

        /// <summary>
        /// Create a new Catalog.
        /// </summary>
        [HttpPost, Route("{buyerID}/catalogs"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<HSCatalog> Create([FromBody] HSCatalog obj, string buyerID)
        {
            return await catalogCommand.Create(buyerID, obj, UserContext);
        }

        /// <summary>
        /// Get a list of catalog location assignments.
        /// </summary>
        [HttpGet, Route("{buyerID}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<ListPage<HSCatalogAssignmentResponse>> GetAssignments(string buyerID, [FromQuery(Name = "catalogID")] string catalogID = "", [FromQuery(Name = "locationID")] string locationID = "")
        {
            return await catalogCommand.GetAssignments(buyerID, locationID, UserContext);
        }

        /// <summary>
        /// Set catalog assignments for a location.
        /// </summary>
        [HttpPost, Route("{buyerID}/{locationID}/catalogs/assignments"), OrderCloudUserAuth(ApiRole.UserGroupAdmin)]
        public async Task SetAssignments(string buyerID, string locationID, [FromBody] HSCatalogAssignmentRequest assignmentRequest)
        {
            await catalogCommand.SetAssignments(buyerID, locationID, assignmentRequest.CatalogIDs, UserContext.AccessToken);
        }

        /// <summary>
        /// Save Catalog.
        /// </summary>
        [HttpPut, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<HSCatalog> Save([FromBody] HSCatalog obj, string buyerID, string catalogID)
        {
            return await catalogCommand.Save(buyerID, catalogID, obj, UserContext);
        }

        /// <summary>
        /// DELETE Catalog.
        /// </summary>
        [HttpDelete, Route("{buyerID}/catalogs/{catalogID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task Delete(string buyerID, string catalogID)
        {
            await catalogCommand.Delete(buyerID, catalogID, UserContext);
        }

        /// <summary>
        /// SYNC User Catalogs Assignments.
        /// </summary>
        [HttpPost, Route("{buyerID}/catalogs/user/{userID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task SyncUserCatalogAssignments(string buyerID, string userID)
        {
            await catalogCommand.SyncUserCatalogAssignments(buyerID, userID);
        }
    }
}
