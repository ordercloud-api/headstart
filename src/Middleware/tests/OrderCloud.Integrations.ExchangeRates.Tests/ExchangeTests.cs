using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Flurl.Http.Testing;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Headstart.Common.Services;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.ExchangeRates.Tests
{
    public class ExchangeTests
    {
        private HttpTest http;
        private IOrderCloudClient oc;
        private ICurrencyConversionCommand command;
        private ICurrencyConversionService currencyConversionService;
        private IOrderCloudIntegrationsBlobService blob;
        private ISimpleCache simpleCache;

        [SetUp]
        public void Setup()
        {
            http = new HttpTest();
            http.RespondWith(@"{'rates':{'CAD':1.5231,'HKD':8.3693,'ISK':157.5,'PHP':54.778,'DKK':7.4576,'HUF':354.7,'CZK':27.589,'AUD':1.6805,'RON':4.84,'SEK':10.6695,'IDR':16127.82,'INR':81.9885,'BRL':6.3172,'RUB':79.6208,'HRK':7.5693,'JPY':115.53,'THB':34.656,'CHF':1.0513,'SGD':1.5397,'PLN':4.565,'BGN':1.9558,'TRY':7.4689,'CNY':7.6759,'NOK':11.0568,'NZD':1.8145,'ZAR':20.0761,'USD':1.0798,'MXN':25.8966,'ILS':3.8178,'GBP':0.88738,'KRW':1332.6,'MYR':4.6982},'base':'EUR','date':'2020-05-15'}");

            // _http.RespondWith(@"{'rates':{'MYR':4.6982},'base':'EUR','date':'2020-05-15'}");
            oc = Substitute.For<IOrderCloudClient>();
            blob = Substitute.For<IOrderCloudIntegrationsBlobService>();
            currencyConversionService = Substitute.For<ICurrencyConversionService>();
            simpleCache = Substitute.For<ISimpleCache>();
            command = new ExchangeRatesCommand(oc, blob, currencyConversionService, simpleCache);
        }

        [Test]
        public async Task Get_WithEURCurrency_ReturnsMappedRates()
        {
            // Arrange

            // Act
            var rates = await currencyConversionService.Get(CurrencyCode.EUR);

            // Assert
            Assert.IsTrue(rates.BaseCode == CurrencyCode.EUR);
            Assert.IsTrue(rates.Rates.Count(r => r.Rate == 0) == 0); // make sure any errors in returned data (null rate for EUR) is set to 1
            Assert.IsFalse(rates.Rates.Any(r => r.Icon == null));
        }

        [Test]
        public void Filter_WithValidArguments_ReturnsFilteredResults()
        {
            // Arrange
            var args = new ListArgs<ConversionRate>()
            {
                Filters = new List<ListFilter>()
                {
                    new ListFilter("CurrencyCode", "CAD|USD"),
                },
            };
            var rates = new ConversionRates()
            {
                BaseCode = CurrencyCode.EUR,
                Rates = GetRates(),
            };

            // Act
            var filtered = command.Filter(args, rates);

            // Assert
            Assert.IsTrue(filtered.Meta.TotalCount == 2);
            Assert.IsTrue(filtered.Items.Count == 2);
            Assert.IsTrue(filtered.Items.Any(i => i.Currency == CurrencyCode.USD));
            Assert.IsTrue(filtered.Items.Any(i => i.Currency == CurrencyCode.CAD));
            Assert.IsFalse(filtered.Items.Any(i => i.Currency == CurrencyCode.BGN));
        }

        [Test]
        public async Task ConvertCurrency_WithValidArguments_ReturnsCalculatedValue()
        {
            // Arrange
            var baseCurrency = CurrencyCode.EUR;
            var toCurrency = CurrencyCode.MYR;
            double returnedRate = 4.6982;

            simpleCache.GetOrAddAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<Func<Task<ConversionRates>>>())
                .ReturnsForAnyArgs(
                    GetExchangeRate(baseCurrency, toCurrency, returnedRate));

            // Act
            var rate = await command.ConvertCurrency(baseCurrency, toCurrency, 1.33);

            // Assert
            Assert.IsTrue(rate == (1.33 * returnedRate));
        }

        private ConversionRates GetExchangeRate(CurrencyCode baseCurrency, CurrencyCode toCurrency, double returnedRate)
        {
            Fixture fixture = new Fixture();
            return new ConversionRates()
            {
                BaseCode = baseCurrency,
                Rates = new List<ConversionRate>()
                {
                    new ConversionRate()
                    {
                        Currency = toCurrency,
                        Rate = returnedRate,
                    },
                    fixture.Create<ConversionRate>(),
                    fixture.Create<ConversionRate>(),
                },
            };
        }

        private List<ConversionRate> GetRates()
        {
            return new List<ConversionRate>()
                {
                    new ConversionRate() { Currency = CurrencyCode.EUR, Icon = string.Empty, Name = "EUR", Rate = 1, Symbol = "€" },
                    new ConversionRate() { Currency = CurrencyCode.USD, Icon = string.Empty, Name = "USD", Rate = 1.01456, Symbol = "$" },
                    new ConversionRate() { Currency = CurrencyCode.CAD, Icon = string.Empty, Name = "CAD", Rate = 2.65487, Symbol = "$" },
                    new ConversionRate() { Currency = CurrencyCode.BGN, Icon = string.Empty, Name = "BGN", Rate = 31.357, Symbol = "лв" },
                };
        }
    }
}
