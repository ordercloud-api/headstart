using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using Headstart.API.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Headstart.API.Controllers
{
	[Route("buyer")] 
	public class BuyerController : CatalystController
	{
		private readonly IHSBuyerCommand _command;
		private readonly IOrderCloudClient _oc;

		/// <summary>
		/// The IOC based constructor method for the BuyerController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		/// <param name="oc"></param>
		public BuyerController(IHSBuyerCommand command, IOrderCloudClient oc)
		{
			_command = command;
			_oc = oc;
		}

		/// <summary>
		/// Creates a Buyer action (POST method)
		/// </summary>
		/// <param name="buyer"></param>
		/// <returns>The newly created SuperHSBuyer object</returns>
		[HttpPost, OrderCloudUserAuth(ApiRole.BuyerAdmin)]
		public async Task<SuperHSBuyer> Create([FromBody] SuperHSBuyer buyer)
		{
			return await _command.Create(buyer);
		}

		/// <summary>
		/// Updates the Buyer action (PUT method)
		/// </summary>
		/// <param name="superBuyer"></param>
		/// <param name="buyerId"></param>
		/// <returns>The newly updated SuperHSBuyer object</returns>
		[HttpPut, Route("{buyerId}"), OrderCloudUserAuth(ApiRole.BuyerAdmin)]
		public async Task<SuperHSBuyer> Put([FromBody] SuperHSBuyer superBuyer, string buyerId)
		{
			return await _command.Update(buyerId, superBuyer);
		}

		/// <summary>
		/// Gets a Buyer action (GET method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <returns>The SuperHSBuyer object by buyerID</returns>
		[HttpGet, Route("{buyerId}"), OrderCloudUserAuth(ApiRole.BuyerAdmin)]
		public async Task<SuperHSBuyer> Get(string buyerId)
		{
			return await _command.Get(buyerId);
		}
	}
}