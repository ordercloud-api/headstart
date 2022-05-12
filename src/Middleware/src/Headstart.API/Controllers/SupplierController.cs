using Headstart.API.Commands;
using Headstart.Models;
using Headstart.Models.Headstart;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System.Threading.Tasks;

namespace Headstart.API.Controllers
{
    [Route("supplier")]
    public class SupplierController : CatalystController
	{
		private readonly IHSSupplierCommand _command;
		private readonly IOrderCloudClient _oc;

		/// <summary>
		/// The IOC based constructor method for the SupplierController class object with Dependency Injection
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
		/// <param name="supplierId"></param>
		/// <returns>The HSSupplier object by supplierID</returns>
		[HttpGet, Route("me/{supplierId}"), OrderCloudUserAuth]
		public async Task<HSSupplier> GetMySupplier(string supplierId)
		{
			return await _command.GetMySupplier(supplierId, UserContext);
		}

		/// <summary>
		/// Post action for Creating a new HSSupplier object (POST method)
		/// </summary>
		/// <param name="supplier"></param>
		/// <returns>The HSSupplier object from the post action for Creating a new HSSupplier object</returns>
		[HttpPost, OrderCloudUserAuth(ApiRole.SupplierAdmin)]
		public async Task<HSSupplier> Create([FromBody] HSSupplier supplier)
		{
			return await _command.Create(supplier, UserContext.AccessToken);
		}

		/// <summary>
		/// Get the boolean status whether the Location is deletable or not (GET method)
		/// </summary>
		/// <param name="locationId"></param>
		/// <returns>The boolean status whether the Location is deletable or not</returns>
		[HttpGet, Route("candelete/{locationId}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<bool> CanDeleteLocation(string locationId)
		{
			var productList = await _oc.Products.ListAsync(filters: $"ShipFromAddressID={locationId}");
			return productList.Items.Count == 0;
		}

		/// <summary>
		/// Update action to patch updated details for the HSSupplier data (PATCH method)
		/// </summary>
		/// <param name="supplierId"></param>
		/// <param name="supplier"></param>
		/// <returns>The HSSupplier object from the update action to patch updated details for the HSSupplier data</returns>
		[HttpPatch, Route("{supplierId}"), OrderCloudUserAuth]
		public async Task<HSSupplier> UpdateSupplier(string supplierId, [FromBody] PartialSupplier supplier)
		{
			return await _command.UpdateSupplier(supplierId, supplier, UserContext);
		}

		/// <summary>
		/// Gets the HSSupplierOrderData by supplierOrderID and orderType (GET method)
		/// </summary>
		/// <param name="supplierOrderId"></param>
		/// <param name="orderType"></param>
		/// <returns>The HSSupplierOrderData object by supplierOrderID and orderType</returns>
		[HttpGet, Route("orderdetails/{supplierOrderId}/{orderType}"), OrderCloudUserAuth(ApiRole.OrderAdmin, ApiRole.OrderReader)]
		public async Task<HSSupplierOrderData> GetSupplierOrder(string supplierOrderId, OrderType orderType)
		{
			return await _command.GetSupplierOrderData(supplierOrderId, orderType, UserContext);
		}
	}
}