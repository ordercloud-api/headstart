using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.API.Controllers
{
    /// <summary>
    /// Suppliers.
    /// </summary>
    [Route("supplier")]
    public class SupplierController : CatalystController
    {
        private readonly ISupplierCommand supplierCommand;
        private readonly IOrderCloudClient oc;

        public SupplierController(ISupplierCommand supplierCommand, IOrderCloudClient oc)
        {
            this.supplierCommand = supplierCommand;
            this.oc = oc;
        }

        /// <summary>
        /// GET HSSupplier.
        /// </summary>
        [HttpGet, Route("me/{supplierID}"), OrderCloudUserAuth]
        public async Task<HSSupplier> GetMySupplier(string supplierID)
        {
            return await supplierCommand.GetMySupplier(supplierID, UserContext);
        }

        /// <summary>
        /// POST Headstart Supplier.
        /// </summary>
        [HttpPost, OrderCloudUserAuth(ApiRole.SupplierAdmin)]
        public async Task<HSSupplier> Create([FromBody] HSSupplier supplier)
        {
            return await supplierCommand.Create(supplier, UserContext.AccessToken);
        }

        /// <summary>
        /// GET If Location Deletable.
        /// </summary>
        [HttpGet, Route("candelete/{locationID}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
        public async Task<bool> CanDeleteLocation(string locationID)
        {
            var productList = await oc.Products.ListAsync(filters: $"ShipFromAddressID={locationID}");
            return productList.Items.Count == 0;
        }

        /// <summary>
        /// PATCH Supplier.
        /// </summary>
        [HttpPatch, Route("{supplierID}"), OrderCloudUserAuth]
        public async Task<HSSupplier> UpdateSupplier(string supplierID, [FromBody] PartialSupplier supplier)
        {
            return await supplierCommand.UpdateSupplier(supplierID, supplier, UserContext);
        }

        /// <summary>
        /// GET Supplier Order Details.
        /// </summary>
        [HttpGet, Route("orderdetails/{supplierOrderID}/{orderType}"), OrderCloudUserAuth(ApiRole.OrderAdmin, ApiRole.OrderReader)]
        public async Task<HSSupplierOrderData> GetSupplierOrder(string supplierOrderID, OrderType orderType)
        {
            return await supplierCommand.GetSupplierOrderData(supplierOrderID, orderType, UserContext);
        }
    }
}
