using Headstart.Models.Headstart;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.Models;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
	/// <summary>
	/// Suppliers
	/// </summary>
    [Route("supplier")]
    public class SupplierController: CatalystController
    {

		private readonly IHSSupplierCommand _command;
        private readonly IOrderCloudClient _oc;
        public SupplierController(IHSSupplierCommand command, IOrderCloudClient oc)
        {
            _command = command;
			_oc = oc;
        }
		/// <summary>
		/// GET HSSupplier
		/// </summary>
		[HttpGet, Route("me/{supplierID}"), OrderCloudUserAuth]
		public async Task<HSSupplier> GetMySupplier(string supplierID)
		{
			return await _command.GetMySupplier(supplierID, UserContext);
		}
		/// <summary>
		/// POST Headstart Supplier
		/// </summary>
		[HttpPost, OrderCloudUserAuth(ApiRole.SupplierAdmin)]
		public async Task<HSSupplier> Create([FromBody] HSSupplier supplier)
		{
			return await _command.Create(supplier, UserContext.AccessToken);
		}
		/// <summary>
		/// GET If Location Deletable
		/// </summary>
		[HttpGet, Route("candelete/{locationID}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<bool> CanDeleteLocation(string locationID)
		{
			var productList = await _oc.Products.ListAsync(filters: $"ShipFromAddressID={locationID}");
			return productList.Items.Count == 0;
		}
		/// <summary>
		/// PATCH Supplier
		/// </summary>
		[HttpPatch, Route("{supplierID}"), OrderCloudUserAuth]
		public async Task<HSSupplier> UpdateSupplier(string supplierID, [FromBody] PartialSupplier supplier)
		{
			return await _command.UpdateSupplier(supplierID, supplier, UserContext);
		}
		/// <summary>
		/// GET Supplier Order Details
		/// </summary>
		[HttpGet, Route("orderdetails/{supplierOrderID}/{orderType}"), OrderCloudUserAuth(ApiRole.OrderAdmin, ApiRole.OrderReader)]
		public async Task<HSSupplierOrderData> GetSupplierOrder(string supplierOrderID, OrderType orderType)
        {
			return await _command.GetSupplierOrderData(supplierOrderID, orderType, UserContext);
        }

	}
}
