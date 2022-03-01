using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models.Headstart;

namespace Headstart.Common.Controllers
{
	[Route("supplier")]
    public class SupplierController: CatalystController
    {

		private readonly IHSSupplierCommand _command;
        private readonly IOrderCloudClient _oc;

		/// <summary>
		/// The IOC based constructor method for the SupplierController with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		/// <param name="oc"></param>
		public SupplierController(IHSSupplierCommand command, IOrderCloudClient oc)
        {
            _command = command;
			_oc = oc;
        }

		/// <summary>
		/// Gets the HSSupplier object by the supplierID (GET method)
		/// </summary>
		/// <param name="supplierID"></param>
		/// <returns>The HSSupplier object by supplierID</returns>
		[HttpGet, Route("me/{supplierID}"), OrderCloudUserAuth]
		public async Task<HSSupplier> GetMySupplier(string supplierID)
		{
			return await _command.GetMySupplier(supplierID, UserContext);
		}

		/// <summary>
		/// Post action for Creating a new HSSupplier object (POST method)
		/// </summary>
		/// <param name="supplier"></param>
		/// <returns>The response from the post action for Creating a new HSSupplier object</returns>
		[HttpPost, OrderCloudUserAuth(ApiRole.SupplierAdmin)]
		public async Task<HSSupplier> Create([FromBody] HSSupplier supplier)
		{
			return await _command.Create(supplier, UserContext.AccessToken);
		}

		/// <summary>
		/// Get the boolean status whether the Location is deletable or not (GET method)
		/// </summary>
		/// <param name="locationID"></param>
		/// <returns>The boolean status whether the Location is deletable or not</returns>
		[HttpGet, Route("candelete/{locationID}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<bool> CanDeleteLocation(string locationID)
		{
			var productList = await _oc.Products.ListAsync(filters: $"ShipFromAddressID={locationID}");
			return productList.Items.Count == 0;
		}

		/// <summary>
		/// Update action to patch updated details for the HSSupplier data (PATCH method)
		/// </summary>
		/// <param name="supplierID"></param>
		/// <param name="supplier"></param>
		/// <returns>The response from the update action to patch updated details for the HSSupplier data</returns>
		[HttpPatch, Route("{supplierID}"), OrderCloudUserAuth]
		public async Task<HSSupplier> UpdateSupplier(string supplierID, [FromBody] PartialSupplier supplier)
		{
			return await _command.UpdateSupplier(supplierID, supplier, UserContext);
		}

		/// <summary>
		/// Gets the HSSupplierOrderData by supplierOrderID and orderType (GET method)
		/// </summary>
		/// <param name="supplierOrderID"></param>
		/// <param name="orderType"></param>
		/// <returns>The HSSupplierOrderData by supplierOrderID and orderType</returns>
		[HttpGet, Route("orderdetails/{supplierOrderID}/{orderType}"), OrderCloudUserAuth(ApiRole.OrderAdmin, ApiRole.OrderReader)]
		public async Task<HSSupplierOrderData> GetSupplierOrder(string supplierOrderID, OrderType orderType)
        {
			return await _command.GetSupplierOrderData(supplierOrderID, orderType, UserContext);
        }
	}
}