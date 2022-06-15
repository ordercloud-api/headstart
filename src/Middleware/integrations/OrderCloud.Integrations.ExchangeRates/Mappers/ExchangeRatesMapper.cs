using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using OrderCloud.Integrations.ExchangeRates.Models;

namespace OrderCloud.Integrations.ExchangeRates.Mappers
{
    public static class ExchangeRatesMapper
    {
        public static List<ConversionRate> MapRates(ExchangeRatesValues ratesValues = null)
        {
            return Enum.GetValues(typeof(CurrencyCode)).Cast<CurrencyCode>().Select(currencyCode => new ConversionRate()
            {
                Currency = currencyCode,
                Icon = GetIcon(currencyCode),
                Symbol = CurrencyLookup.CurrencyCodeLookup.FirstOrDefault(s => s.Key == currencyCode).Value.Symbol,
                Name = CurrencyLookup.CurrencyCodeLookup.FirstOrDefault(s => s.Key == currencyCode).Value.Name,
                Rate = FixRate(ratesValues, currencyCode),
            }).ToList();
        }

        private static string GetIcon(CurrencyCode currencyCode)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"OrderCloud.Integrations.ExchangeRates.Icons.{currencyCode}.gif");
            if (stream == null)
            {
                return null;
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return $"data:image/jpg;base64,{Convert.ToBase64String(ms.ToArray())}";
        }

        private static double? FixRate(ExchangeRatesValues values, CurrencyCode e)
        {
            var t = values?.GetType().GetProperty($"{e}")?.GetValue(values, null).To<double?>();
            if (!t.HasValue)
            {
                return 1;
            }

            return t.Value == 0 ? 1 : t.Value;
        }
    }
}
