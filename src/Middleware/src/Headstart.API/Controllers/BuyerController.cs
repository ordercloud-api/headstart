using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using Headstart.Common.Models.Headstart;

namespace Headstart.API.Controllers
{
	[Route("buyer")] 
	public class BuyerController : CatalystController
	{
		private readonly IHsBuyerCommand _command;
		private readonly IOrderCloudClient _oc;

		/// <summary>
		/// The IOC based constructor method for the BuyerController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		/// <param name="oc"></param>
		public BuyerController(IHsBuyerCommand command, IOrderCloudClient oc)
		{
			_command = command;
			_oc = oc;
		}

		/// <summary>
		/// Creates a Buyer action (POST method)
		/// </summary>
		/// <param name="buyer"></param>
		/// <returns>The newly created SuperHsBuyer object</returns>
		[HttpPost, OrderCloudUserAuth(ApiRole.BuyerAdmin)]
		public async Task<SuperHsBuyer> Create([FromBody] SuperHsBuyer buyer)
		{
			return await _command.Create(buyer);
		}

		/// <summary>
		/// Updates the Buyer action (PUT method)
		/// </summary>
		/// <param name="superBuyer"></param>
		/// <param name="buyerId"></param>
		/// <returns>The newly updated SuperHsBuyer object</returns>
		[HttpPut, Route("{buyerId}"), OrderCloudUserAuth(ApiRole.BuyerAdmin)]
		public async Task<SuperHsBuyer> Put([FromBody] SuperHsBuyer superBuyer, string buyerId)
		{
			return await _command.Update(buyerId, superBuyer);
		}

		/// <summary>
		/// Gets a Buyer action (GET method)
		/// </summary>
		/// <param name="buyerId"></param>
		/// <returns>The SuperHsBuyer object by buyerId</returns>
		[HttpGet, Route("{buyerId}"), OrderCloudUserAuth(ApiRole.BuyerAdmin)]
		public async Task<SuperHsBuyer> Get(string buyerId)
		{
			return await _command.Get(buyerId);
		}
	}
}