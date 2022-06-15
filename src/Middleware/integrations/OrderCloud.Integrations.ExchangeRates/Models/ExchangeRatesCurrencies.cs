using System.Collections.Generic;
using Headstart.Common.Models;

namespace OrderCloud.Integrations.ExchangeRates.Models
{
    public static class CurrencyLookup
    {
        public static readonly IDictionary<CurrencyCode, CurrencyDisplay> CurrencyCodeLookup = new Dictionary<CurrencyCode, CurrencyDisplay>()
        {
            { CurrencyCode.AUD, new CurrencyDisplay() { Name = "Australian Dollar", Symbol = "$" } },
            { CurrencyCode.BGN, new CurrencyDisplay() { Name = "Bulgaria Lev", Symbol = "лв" } },
            { CurrencyCode.RON, new CurrencyDisplay() { Name = "Romanian Leu", Symbol = "lei" } },
            { CurrencyCode.BRL, new CurrencyDisplay() { Name = "Brazil Real", Symbol = "R$" } },
            { CurrencyCode.CAD, new CurrencyDisplay() { Name = "Canada Dollar", Symbol = "$" } },
            { CurrencyCode.HKD, new CurrencyDisplay() { Name = "Hong Kong Dollar", Symbol = "$" } },
            { CurrencyCode.ISK, new CurrencyDisplay() { Name = "Iceland Krona", Symbol = "kr" } },
            { CurrencyCode.PHP, new CurrencyDisplay() { Name = "Phillipines Peso", Symbol = "₱" } },
            { CurrencyCode.DKK, new CurrencyDisplay() { Name = "Denmark Krone", Symbol = "kr" } },
            { CurrencyCode.HUF, new CurrencyDisplay() { Name = "Hungary Forint", Symbol = "ft" } },
            { CurrencyCode.CZK, new CurrencyDisplay() { Name = "Czech Republic Koruna", Symbol = "Kč" } },
            { CurrencyCode.GBP, new CurrencyDisplay() { Name = "United Kingdom Pound", Symbol = "£" } },
            { CurrencyCode.SEK, new CurrencyDisplay() { Name = "Sweden Krona", Symbol = "kr" } },
            { CurrencyCode.IDR, new CurrencyDisplay() { Name = "Indonesia Rupiah", Symbol = "Rp" } },
            { CurrencyCode.INR, new CurrencyDisplay() { Name = "India Rupee", Symbol = "₹" } },
            { CurrencyCode.RUB, new CurrencyDisplay() { Name = "Russia Ruble", Symbol = "₽" } },
            { CurrencyCode.HRK, new CurrencyDisplay() { Name = "Croatia Kuna", Symbol = "kn" } },
            { CurrencyCode.JPY, new CurrencyDisplay() { Name = "Japan Yen", Symbol = "¥" } },
            { CurrencyCode.THB, new CurrencyDisplay() { Name = "Thailand Baht", Symbol = "฿" } },
            { CurrencyCode.CHF, new CurrencyDisplay() { Name = "Switzerland Franc", Symbol = "CHF" } },
            { CurrencyCode.EUR, new CurrencyDisplay() { Name = "Euro Member Countries", Symbol = "€" } },
            { CurrencyCode.MYR, new CurrencyDisplay() { Name = "Malaysia Ringgit", Symbol = "RM" } },
            { CurrencyCode.TRY, new CurrencyDisplay() { Name = "Turkey Lira", Symbol = "₺" } },
            { CurrencyCode.CNY, new CurrencyDisplay() { Name = "China Yuan Renminbi", Symbol = "/元" } },
            { CurrencyCode.NOK, new CurrencyDisplay() { Name = "Norway Krone", Symbol = "kr" } },
            { CurrencyCode.NZD, new CurrencyDisplay() { Name = "New Zealand Dollar", Symbol = "$" } },
            { CurrencyCode.ZAR, new CurrencyDisplay() { Name = "South Africa Rand", Symbol = "R" } },
            { CurrencyCode.USD, new CurrencyDisplay() { Name = "United States Dollar", Symbol = "$" } },
            { CurrencyCode.MXN, new CurrencyDisplay() { Name = "Mexico Peso", Symbol = "$" } },
            { CurrencyCode.SGD, new CurrencyDisplay() { Name = "Singapore Dollar", Symbol = "$" } },
            { CurrencyCode.ILS, new CurrencyDisplay() { Name = "Israel Shekel", Symbol = "₪" } },
            { CurrencyCode.KRW, new CurrencyDisplay() { Name = "Korea (South) Won", Symbol = "₩" } },
            { CurrencyCode.PLN, new CurrencyDisplay() { Name = "Poland Zloty", Symbol = "zł" } },
        };
    }

    public class CurrencyDisplay
    {
        public string Name { get; set; }

        public string Symbol { get; set; }
    }
}
