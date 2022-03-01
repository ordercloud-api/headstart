using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.smartystreets;

namespace Headstart.Common.Controllers
{
	public class ValidatedAddressController: CatalystController
	{
		private readonly ISmartyStreetsCommand _command;

		/// <summary>
		/// The IOC based constructor method for the ValidatedAddressController with Dependency Injection
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
		public async Task<BuyerAddress> CreateMeAddress([FromBody] BuyerAddress address) =>
			await _command.CreateMeAddress(address, UserContext);

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
		/// <param name="addressID"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("me/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
		public async Task<BuyerAddress> SaveMeAddress(string addressID, [FromBody] BuyerAddress address) =>
			await _command.SaveMeAddress(addressID, address, UserContext);

		/// <summary>
		/// Patch action to path the MeAddress/BuyerAddress object data (PATCH method)
		/// </summary>
		/// <param name="addressID"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		[HttpPatch, Route("me/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
		public async Task PatchMeAddress(string addressID, [FromBody] BuyerAddress patch) =>
			await _command.PatchMeAddress(addressID, patch, UserContext);


		/// <summary>
		/// Creates an Address for the Buyer object, using the buyerID (POST method)
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPost, Route("buyers/{buyerID}/addresses"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
		public async Task<Address> CreateBuyerAddress(string buyerID, [FromBody] Address address) =>
			await _command.CreateBuyerAddress(buyerID, address, UserContext);

		/// <summary>
		/// Update action to save the Address object data for the Buyer object, using the buyerID and addressID (PUT method)
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="addressID"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("buyers/{buyerID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
		public async Task<Address> SaveBuyerAddress(string buyerID, string addressID, [FromBody] Address address) =>
			await _command.SaveBuyerAddress(buyerID, addressID, address, UserContext);

		/// <summary>
		/// Patch action to patch the Address object data for the Buyer object, using the buyerID and addressID (PATCH method)
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="addressID"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		[HttpPatch, Route("buyers/{buyerID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
		public async Task<Address> PatchBuyerAddress(string buyerID, string addressID, [FromBody] Address patch) =>
			await _command.PatchBuyerAddress(buyerID, addressID, patch, UserContext);


		/// <summary>
		/// Creates an Address for the Supplier object, using the supplierID (POST method)
		/// </summary>
		/// <param name="supplierID"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPost, Route("suppliers/{supplierID}/addresses"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> CreateSupplierAddress(string supplierID, [FromBody] Address address) =>
			await _command.CreateSupplierAddress(supplierID, address, UserContext);

		/// <summary>
		/// Update action to save the Address object data for the Supplier object, using the supplierID and addressID (PUT method)
		/// </summary>
		/// <param name="supplierID"></param>
		/// <param name="addressID"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("suppliers/{supplierID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> SaveSupplierAddress(string supplierID, string addressID, [FromBody] Address address) =>
			await _command.SaveSupplierAddress(supplierID, addressID, address, UserContext);

		/// <summary>
		/// Patch action to patch the Address object data for the Supplier object, using the supplierID and addressID (PATCH method)
		/// </summary>
		/// <param name="supplierID"></param>
		/// <param name="addressID"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		[HttpPatch, Route("suppliers/{supplierID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> PatchSupplierAddress(string supplierID, string addressID, [FromBody] Address patch) =>
			await _command.PatchSupplierAddress(supplierID, addressID, patch, UserContext);


		/// <summary>
		/// Creates an Address for the Admin User object (POST method)
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPost, Route("addresses"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> CreateAdminAddress([FromBody] Address address) =>
			await _command.CreateAdminAddress(address, UserContext);

		/// <summary>
		/// Update action to save the Address object data for the Admin User object, using the addressID (PUT method)
		/// </summary>
		/// <param name="addressID"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> SaveAdminAddress(string addressID, [FromBody] Address address) =>
			await _command.SaveAdminAddress(addressID, address, UserContext);

		/// <summary>
		/// Patch action to patch the Address object data for the Admin User object, using the addressID (PATCH method)
		/// </summary>
		/// <param name="addressID"></param>
		/// <param name="patch"></param>
		/// <returns></returns>
		[HttpPatch, Route("addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> PatchAdminAddress(string addressID, [FromBody] Address patch) =>
			await _command.PatchAdminAddress(addressID, patch, UserContext);


		/// <summary>
		/// Setting the Billing Address object order, using the orderID and direction (PUT method)
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="orderID"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("order/{direction}/{orderID}/billto"), OrderCloudUserAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
		public async Task<Order> SetBillingAddress(OrderDirection direction, string orderID, [FromBody] Address address) =>
			await _command.SetBillingAddress(direction, orderID, address, UserContext);

		/// <summary>
		/// Setting the Shipping Address object order, using the orderID and direction (PUT method)
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="orderID"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		[HttpPut, Route("order/{direction}/{orderID}/shipto"), OrderCloudUserAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
		public async Task<Order> SetShippingAddress(OrderDirection direction, string orderID, [FromBody] Address address) =>
			await _command.SetShippingAddress(direction, orderID, address, UserContext);

	}
}