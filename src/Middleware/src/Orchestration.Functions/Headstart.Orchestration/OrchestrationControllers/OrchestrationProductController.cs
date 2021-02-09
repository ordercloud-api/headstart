using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using Headstart.API.Controllers;
using Headstart.Common;
using Headstart.API.Commands;

namespace Headstart.Orchestration
{
    [DocComments("\"Orchestration\" represents objects exposed for orchestration control")]
    [HSSection.Orchestration(ListOrder = 1)]
    [Route("orchestration")]
    public class OrchestrationProductController : BaseController
    {
        private readonly IOrchestrationCommand _command;

        public OrchestrationProductController(AppSettings settings, IOrchestrationCommand command) : base(settings)
        {
            _command = command;
        }

        [DocName("POST FlatProduct")]
        [HttpPost, Route("templateproduct"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        public async Task<TemplateProductFlat> PostTemplateFlatProduct([FromBody] TemplateProductFlat obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST FlatProducts")]
        [DocIgnore]
        [HttpPost, Route("templateproducts"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        public async Task<List<TemplateProductFlat>> PostTemplateFlatProducts([FromBody] List<TemplateProductFlat> obj)
        {
            var result = await Throttler.RunAsync(obj, 100, 100, p => _command.SaveToQueue(p, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID));
            return result.ToList();
        }

        [DocName("POST SuperProduct")]
        [HttpPost, Route("hydrated"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        public async Task<SuperHSProduct> PostHydratedProduct([FromBody] SuperHSProduct obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST Catalog")]
        [HttpPost, Route("catalog"), OrderCloudIntegrationsAuth(ApiRole.CatalogAdmin)]
        public async Task<HSCatalog> PostCatalog([FromBody] HSCatalog obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST Product")]
        [HttpPost, Route("product"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        public async Task<HSProduct> PostProduct([FromBody] HSProduct obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST Product Facet")]
        [HttpPost, Route("productfacet"), OrderCloudIntegrationsAuth(ApiRole.ProductFacetAdmin)]
        public async Task<HSProductFacet> PostProductFacet([FromBody] HSProductFacet obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST Price Schedule")]
        [HttpPost, Route("priceschedule"), OrderCloudIntegrationsAuth(ApiRole.PriceScheduleAdmin)]
        public async Task<HSPriceSchedule> PostPriceSchedule([FromBody] HSPriceSchedule obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST Product Assignment")]
        [HttpPost, Route("productassignment"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        public async Task<HSProductAssignment> PostProductAssignment([FromBody] HSProductAssignment obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST Spec")]
        [HttpPost, Route("spec"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        public async Task<HSSpec> PostSpec([FromBody] HSSpec obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST Spec Option")]
        [HttpPost, Route("specoption"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        public async Task<HSSpecOption> PostSpecOption([FromBody] HSSpecOption obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST Spec Product Assignment")]
        [HttpPost, Route("specproductassignment"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        public async Task<HSSpecProductAssignment> PostSpecProductAssignment([FromBody] HSSpecProductAssignment obj, string clientId)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }

        [DocName("POST Catalog Product Assignment")]
        [HttpPost, Route("catalogproductassignment"), OrderCloudIntegrationsAuth(ApiRole.CatalogAdmin)]
        public async Task<HSCatalogAssignment> PostCatalogProductAssignment([FromBody] HSCatalogAssignment obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, this.VerifiedUserContext.SupplierID);
        }
    }
}
