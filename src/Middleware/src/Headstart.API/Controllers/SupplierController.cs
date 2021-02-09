using Headstart.Models.Headstart;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.Models;
using Headstart.API.Controllers;
using Headstart.API.Commands;

namespace Headstart.Common.Controllers
{
    [DocComments("\"Headstart Suppliers\" represents Supplier in Headstart")]
    [HSSection.Headstart(ListOrder = 2)]
    [Route("supplier")]
    public class SupplierController: BaseController
    {

		private readonly IHeadstartSupplierCommand _command;
        private readonly IOrderCloudClient _oc;
        public SupplierController(IHeadstartSupplierCommand command, IOrderCloudClient oc, AppSettings settings) : base(settings)
        {
            _command = command;
			_oc = oc;
        }

		[DocName("GET HSSupplier")]
		[HttpGet, Route("me/{supplierID}"), OrderCloudIntegrationsAuth]
		public async Task<HSSupplier> GetMySupplier(string supplierID)
		{
			return await _command.GetMySupplier(supplierID, VerifiedUserContext);
		}

		[DocName("POST Headstart Supplier")]
		[HttpPost, OrderCloudIntegrationsAuth(ApiRole.SupplierAdmin)]
		public async Task<HSSupplier> Create([FromBody] HSSupplier supplier)
		{
			return await _command.Create(supplier, VerifiedUserContext);
		}

		[DocName("GET If Location Deletable")]
		[HttpGet, Route("candelete/{locationID}"), OrderCloudIntegrationsAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<bool> CanDeleteLocation(string locationID)
		{
			var productList = await _oc.Products.ListAsync(filters: $"ShipFromAddressID={locationID}");
			return productList.Items.Count == 0;
		}

		[DocName("PATCH Supplier")]
		[DocIgnore] // PartialSupplier throws an openapi error?
		[HttpPatch, Route("{supplierID}"), OrderCloudIntegrationsAuth]
		public async Task<HSSupplier> UpdateSupplier(string supplierID, [FromBody] PartialSupplier supplier)
		{
			return await _command.UpdateSupplier(supplierID, supplier, VerifiedUserContext);
		}

		[DocName("GET Supplier Order Details")]
		[HttpGet, Route("orderdetails/{supplierOrderID}"), OrderCloudIntegrationsAuth(ApiRole.OrderAdmin, ApiRole.OrderReader)]
		public async Task<HSSupplierOrderData> GetSupplierOrder(string supplierOrderID)
        {
			return await _command.GetSupplierOrderData(supplierOrderID, VerifiedUserContext);
        }

	}
}
