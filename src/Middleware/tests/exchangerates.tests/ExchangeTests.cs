using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using NSubstitute;
using NUnit.Framework;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using AutoFixture;
using Flurl.Http.Configuration;
using LazyCache.Mocks;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace exchangerates.tests
{
    public class ExchangeTests
    {
        private HttpTest _http;
        private IExchangeRatesCommand _command;
        private IOrderCloudIntegrationsBlobService _blob;
        private ISimpleCache _simpleCache;

        [SetUp]
        public void Setup()
        {
            _http = new HttpTest();
            _http.RespondWith(@"{'rates':{'CAD':1.5231,'HKD':8.3693,'ISK':157.5,'PHP':54.778,'DKK':7.4576,'HUF':354.7,'CZK':27.589,'AUD':1.6805,'RON':4.84,'SEK':10.6695,'IDR':16127.82,'INR':81.9885,'BRL':6.3172,'RUB':79.6208,'HRK':7.5693,'JPY':115.53,'THB':34.656,'CHF':1.0513,'SGD':1.5397,'PLN':4.565,'BGN':1.9558,'TRY':7.4689,'CNY':7.6759,'NOK':11.0568,'NZD':1.8145,'ZAR':20.0761,'USD':1.0798,'MXN':25.8966,'ILS':3.8178,'GBP':0.88738,'KRW':1332.6,'MYR':4.6982},'base':'EUR','date':'2020-05-15'}");
            //_http.RespondWith(@"{'rates':{'MYR':4.6982},'base':'EUR','date':'2020-05-15'}");
            _blob = Substitute.For<IOrderCloudIntegrationsBlobService>();
            _simpleCache = Substitute.For<ISimpleCache>();
            _command = new ExchangeRatesCommand(_blob, new PerBaseUrlFlurlClientFactory(), _simpleCache);
        }

        private OrderCloudIntegrationsExchangeRate GetExchangeRate(CurrencySymbol baseCurrency, CurrencySymbol toCurrency, double returnedRate)
        {
            Fixture fixture = new Fixture();
            return new OrderCloudIntegrationsExchangeRate()
            {
                BaseSymbol = baseCurrency,
                Rates = new List<OrderCloudIntegrationsConversionRate>()
                {
                    new OrderCloudIntegrationsConversionRate()
                    {
                        Currency = toCurrency,
                        Rate = returnedRate
                    },
                    fixture.Create<OrderCloudIntegrationsConversionRate>(),
                    fixture.Create<OrderCloudIntegrationsConversionRate>()
                }
            };

        }

        private List<OrderCloudIntegrationsConversionRate> GetRates()
        {
            return new List<OrderCloudIntegrationsConversionRate>()
                {
                    new OrderCloudIntegrationsConversionRate() { Currency = CurrencySymbol.EUR, Icon = "", Name = "EUR", Rate = 1, Symbol = "€"},
                    new OrderCloudIntegrationsConversionRate() { Currency = CurrencySymbol.USD, Icon = "", Name = "USD", Rate = 1.01456, Symbol = "$"},
                    new OrderCloudIntegrationsConversionRate() { Currency = CurrencySymbol.CAD, Icon = "", Name = "CAD", Rate = 2.65487, Symbol = "$"},
                    new OrderCloudIntegrationsConversionRate() { Currency = CurrencySymbol.BGN, Icon = "", Name = "BGN", Rate = 31.357, Symbol = "лв"}
                };
        }

        [Test]
        public async Task map_raw_api_rates_test()
        {
            var rates = await _command.Get(CurrencySymbol.EUR);
            Assert.IsTrue(rates.BaseSymbol == CurrencySymbol.EUR);
            Assert.IsTrue(rates.Rates.Count(r => r.Rate == 0) == 0); // make sure any errors in returned data (null rate for EUR) is set to 1
            Assert.IsFalse(rates.Rates.Any(r => r.Icon == null));
        }

        [Test]
        public void has_filtered_symbols()
        {
            var args = new ListArgs<OrderCloudIntegrationsConversionRate>()
            {
                Filters = new List<ListFilter>()
                {
                    new ListFilter("Symbol", "CAD|USD")
                }
            };
            var rates = new OrderCloudIntegrationsExchangeRate()
            {
                BaseSymbol = CurrencySymbol.EUR,
                Rates = GetRates()
            };
            var filtered = _command.Filter(args, rates);
            Assert.IsTrue(filtered.Meta.TotalCount == 2);
            Assert.IsTrue(filtered.Items.Count == 2);
            Assert.IsTrue(filtered.Items.Any(i => i.Currency == CurrencySymbol.USD));
            Assert.IsTrue(filtered.Items.Any(i => i.Currency == CurrencySymbol.CAD));
            Assert.IsFalse(filtered.Items.Any(i => i.Currency == CurrencySymbol.BGN));
        }

        [Test]
        public async Task conversion_rate_by_currency()
        {
            //Arrange
            var baseCurrency = CurrencySymbol.EUR;
            var toCurrency = CurrencySymbol.MYR;
            double returnedRate = 4.6982;

            _simpleCache.GetOrAddAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<Func<Task<OrderCloudIntegrationsExchangeRate>>>())
                .ReturnsForAnyArgs(
                    GetExchangeRate(baseCurrency, toCurrency, returnedRate)
                ); ;


            //Act
            var rate = await _command.ConvertCurrency(baseCurrency, toCurrency, 1.33);

            //Assert
            Assert.IsTrue(rate == (1.33 * returnedRate));
        }
    }
}