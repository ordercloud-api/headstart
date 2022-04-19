using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.smartystreets;

namespace Headstart.API.Controllers
{ 
	public class ValidatedAddressController: CatalystController
	{
		private readonly ISmartyStreetsCommand _command;

		/// <summary>
		/// The IOC based constructor method for the ValidatedAddressController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		public ValidatedAddressController(ISmartyStreetsCommand command)
		{
			_command = command;
		}

		/// <summary>
		/// Creates a MeAddress/BuyerAddress object (POST method)
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPost, Route("me/addresses"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
		public async Task<BuyerAddress> CreateMeAddress([FromBody] BuyerAddress address)
		{
			return await _command.CreateMeAddress(address, UserContext);
		}

		/// <summary>
		/// Post action to validate a MeAddress/BuyerAddress object (POST method)
		/// </summary>
		/// <param name="address"></param>
		/// <returns>The boolean status from the post action to validate a BuyerAddress object</returns>
		[HttpPost, Route("me/addresses/validate")]
		public async Task<BuyerAddress> ValidateAddress([FromBody] BuyerAddress address)
		{
			var validation = await _command.ValidateAddress(address);
			return validation.ValidAddress;
		}

		/// <summary>
		/// Update action to save the MeAddress/BuyerAddress object data, using the addressID (PUT method)
		/// </summary>
		/// <param name="addressId"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("me/addresses/{addressId}"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
		public async Task<BuyerAddress> SaveMeAddress(string addressId, [FromBody] BuyerAddress address)
		{
			return await _command.SaveMeAddress(addressId, address, UserContext);
		}

		/// <summary>
		/// Patch action to path the MeAddress/BuyerAddress object data (PATCH method)
		/// </summary>
		/// <param name="addressId"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		[HttpPatch, Route("me/addresses/{addressId}"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
		public async Task PatchMeAddress(string addressId, [FromBody] BuyerAddress patch)
		{
			await _command.PatchMeAddress(addressId, patch, UserContext);
		}

		/// <summary>
		/// Creates an Address for the Buyer object, using the buyerID (POST method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPost, Route("buyers/{buyerId}/addresses"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
		public async Task<Address> CreateBuyerAddress(string buyerId, [FromBody] Address address)
		{
			return await _command.CreateBuyerAddress(buyerId, address, UserContext);
		}

		/// <summary>
		/// Update action to save the Address object data for the Buyer object, using the buyerID and addressID (PUT method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="addressId"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("buyers/{buyerId}/addresses/{addressId}"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
		public async Task<Address> SaveBuyerAddress(string buyerId, string addressId, [FromBody] Address address)
		{
			return await _command.SaveBuyerAddress(buyerId, addressId, address, UserContext);
		}

		/// <summary>
		/// Patch action to patch the Address object data for the Buyer object, using the buyerID and addressID (PATCH method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="addressId"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		[HttpPatch, Route("buyers/{buyerId}/addresses/{addressId}"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
		public async Task<Address> PatchBuyerAddress(string buyerId, string addressId, [FromBody] Address patch)
		{
			return await _command.PatchBuyerAddress(buyerId, addressId, patch, UserContext);
		}


		/// <summary>
		/// Creates an Address for the Supplier object, using the supplierID (POST method)
		/// </summary>
		/// <param name="supplierId"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPost, Route("suppliers/{supplierId}/addresses"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> CreateSupplierAddress(string supplierId, [FromBody] Address address)
		{
			return await _command.CreateSupplierAddress(supplierId, address, UserContext);
		}

		/// <summary>
		/// Update action to save the Address object data for the Supplier object, using the supplierID and addressID (PUT method)
		/// </summary>
		/// <param name="supplierId"></param>
		/// <param name="addressId"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("suppliers/{supplierId}/addresses/{addressId}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> SaveSupplierAddress(string supplierId, string addressId, [FromBody] Address address)
		{
			return await _command.SaveSupplierAddress(supplierId, addressId, address, UserContext);
		}

		/// <summary>
		/// Patch action to patch the Address object data for the Supplier object, using the supplierID and addressID (PATCH method)
		/// </summary>
		/// <param name="supplierId"></param>
		/// <param name="addressId"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		[HttpPatch, Route("suppliers/{supplierId}/addresses/{addressId}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> PatchSupplierAddress(string supplierId, string addressId, [FromBody] Address patch)
		{
			return await _command.PatchSupplierAddress(supplierId, addressId, patch, UserContext);
		}


		/// <summary>
		/// Creates an Address for the Admin User object (POST method)
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPost, Route("addresses"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> CreateAdminAddress([FromBody] Address address)
		{
			return await _command.CreateAdminAddress(address, UserContext);
		}

		/// <summary>
		/// Update action to save the Address object data for the Admin User object, using the addressID (PUT method)
		/// </summary>
		/// <param name="addressId"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("addresses/{addressId}"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> SaveAdminAddress(string addressId, [FromBody] Address address)
		{
			return await _command.SaveAdminAddress(addressId, address, UserContext);
		}

		/// <summary>
		/// Patch action to patch the Address object data for the Admin User object, using the addressID (PATCH method)
		/// </summary>
		/// <param name="addressId"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		[HttpPatch, Route("addresses/{addressId}"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> PatchAdminAddress(string addressId, [FromBody] Address patch)
		{
			return await _command.PatchAdminAddress(addressId, patch, UserContext);
		}

		/// <summary>
		/// Setting the Billing Address object order, using the orderID and direction (PUT method)
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="orderId"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("order/{direction}/{orderId}/billto"), OrderCloudUserAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
		public async Task<Order> SetBillingAddress(OrderDirection direction, string orderId, [FromBody] Address address)
		{
			return await _command.SetBillingAddress(direction, orderId, address, UserContext);
		}

		/// <summary>
		/// Setting the Shipping Address object order, using the orderID and direction (PUT method)
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="orderId"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("order/{direction}/{orderId}/shipto"), OrderCloudUserAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
		public async Task<Order> SetShippingAddress(OrderDirection direction, string orderId, [FromBody] Address address)
		{
			return await _command.SetShippingAddress(direction, orderId, address, UserContext);
		}
	}
}