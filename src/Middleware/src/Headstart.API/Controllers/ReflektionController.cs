using Headstart.Common.Services;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System.Threading.Tasks;

namespace Headstart.API.Controllers
{

	[Route("reflektion")]
	public class ReflektionController : CatalystController
	{

		private readonly IReflektionService _command;
		public ReflektionController(IReflektionService command)
		{
			_command = command;
		}
		/// <summary>
		/// GET Refelktion token
		/// </summary>
		[HttpGet, Route("token"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<ReflektionAccessToken> Get()
		{
			return await _command.GetAccessToken();
		}
	}
}
