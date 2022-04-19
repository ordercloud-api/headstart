using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.exchangerates;

namespace Headstart.API.Controllers
{
	[Route("exchangerates")]
	public class ExchangeRatesController : CatalystController
	{
		private readonly IExchangeRatesCommand _command;

		/// <summary>
		/// The IOC based constructor method for the ExchangeRatesController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		public ExchangeRatesController(IExchangeRatesCommand command) 
		{
			_command = command;
		}

		/// <summary>
		/// Gets the ListPage of OrderCloudIntegrationsConversionRate objects (GET method)
		/// </summary>
		/// <param name="rateArgs"></param>
		/// <param name="currency"></param>
		/// <returns>The ListPage of OrderCloudIntegrationsConversionRate objects</returns>
		[HttpGet, Route("{currency}")]
		public async Task<ListPage<OrderCloudIntegrationsConversionRate>> Get(ListArgs<OrderCloudIntegrationsConversionRate> rateArgs, CurrencySymbol currency)
		{
			return await _command.Get(rateArgs, currency);
		}

		/// <summary>
		/// Gets the ListPage of OrderCloudIntegrationsConversionRate objects (GET method)
		/// </summary>
		/// <returns>The ListPage of OrderCloudIntegrationsConversionRate objects</returns>
		[HttpGet, Route("supportedrates")]
		public async Task<ListPage<OrderCloudIntegrationsConversionRate>> GetRateList()
		{
			var list = await _command.GetRateList();
			return list;
		}
	}
}