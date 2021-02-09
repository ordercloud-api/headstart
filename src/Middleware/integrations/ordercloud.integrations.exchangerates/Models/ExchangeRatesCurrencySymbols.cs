using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ordercloud.integrations.exchangerates
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CurrencySymbol
    {
        CAD,
        HKD,
        ISK,
        PHP,
        DKK,
        HUF,
        CZK,
        GBP,
        RON,
        SEK,
        IDR,
        INR,
        BRL,
        RUB,
        HRK,
        JPY,
        THB,
        CHF,
        EUR,
        MYR,
        BGN,
        TRY,
        CNY,
        NOK,
        NZD,
        ZAR,
        USD,
        MXN,
        SGD,
        AUD,
        ILS,
        KRW,
        PLN
    }

    public class CurrencyDisplay
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
    public static class SymbolLookup
    {
        public static IDictionary<CurrencySymbol, CurrencyDisplay> CurrencySymbolLookup = new Dictionary<CurrencySymbol, CurrencyDisplay>()
        {
            { CurrencySymbol.AUD, new CurrencyDisplay() { Name = "Australian Dollar", Symbol = "$"}},
            { CurrencySymbol.BGN, new CurrencyDisplay() { Name = "Bulgaria Lev", Symbol = "лв"}},
            { CurrencySymbol.RON, new CurrencyDisplay() { Name = "Romanian Leu", Symbol = "lei"}},
            { CurrencySymbol.BRL, new CurrencyDisplay() { Name = "Brazil Real", Symbol = "R$"}},
            { CurrencySymbol.CAD, new CurrencyDisplay() { Name = "Canada Dollar", Symbol = "$"}},
            { CurrencySymbol.HKD, new CurrencyDisplay() { Name = "Hong Kong Dollar", Symbol = "$"}},
            { CurrencySymbol.ISK, new CurrencyDisplay() { Name = "Iceland Krona", Symbol = "kr"}},
            { CurrencySymbol.PHP, new CurrencyDisplay() { Name = "Phillipines Peso", Symbol = "₱"}},
            { CurrencySymbol.DKK, new CurrencyDisplay() { Name = "Denmark Krone", Symbol = "kr"}},
            { CurrencySymbol.HUF, new CurrencyDisplay() { Name = "Hungary Forint", Symbol = "ft"}},
            { CurrencySymbol.CZK, new CurrencyDisplay() { Name = "Czech Republic Koruna", Symbol = "Kč"}},
            { CurrencySymbol.GBP, new CurrencyDisplay() { Name = "United Kingdom Pound", Symbol = "£"}},
            { CurrencySymbol.SEK, new CurrencyDisplay() { Name = "Sweden Krona", Symbol = "kr"}},
            { CurrencySymbol.IDR, new CurrencyDisplay() { Name = "Indonesia Rupiah", Symbol = "Rp"}},
            { CurrencySymbol.INR, new CurrencyDisplay() { Name = "India Rupee", Symbol = "₹"}},
            { CurrencySymbol.RUB, new CurrencyDisplay() { Name = "Russia Ruble", Symbol = "₽"}},
            { CurrencySymbol.HRK, new CurrencyDisplay() { Name = "Croatia Kuna", Symbol = "kn"}},
            { CurrencySymbol.JPY, new CurrencyDisplay() { Name = "Japan Yen", Symbol = "¥"}},
            { CurrencySymbol.THB, new CurrencyDisplay() { Name = "Thailand Baht", Symbol = "฿"}},
            { CurrencySymbol.CHF, new CurrencyDisplay() { Name = "Switzerland Franc", Symbol = "CHF"}},
            { CurrencySymbol.EUR, new CurrencyDisplay() { Name = "Euro Member Countries", Symbol = "€"}},
            { CurrencySymbol.MYR, new CurrencyDisplay() { Name = "Malaysia Ringgit", Symbol = "RM"}},
            { CurrencySymbol.TRY, new CurrencyDisplay() { Name = "Turkey Lira", Symbol = "₺"}},
            { CurrencySymbol.CNY, new CurrencyDisplay() { Name = "China Yuan Renminbi", Symbol = "/元"}},
            { CurrencySymbol.NOK, new CurrencyDisplay() { Name = "Norway Krone", Symbol = "kr"}},
            { CurrencySymbol.NZD, new CurrencyDisplay() { Name = "New Zealand Dollar", Symbol = "$"}},
            { CurrencySymbol.ZAR, new CurrencyDisplay() { Name = "South Africa Rand", Symbol = "R"}},
            { CurrencySymbol.USD, new CurrencyDisplay() { Name = "United States Dollar", Symbol = "$"}},
            { CurrencySymbol.MXN, new CurrencyDisplay() { Name = "Mexico Peso", Symbol = "$"}},
            { CurrencySymbol.SGD, new CurrencyDisplay() { Name = "Singapore Dollar", Symbol = "$"}},
            { CurrencySymbol.ILS, new CurrencyDisplay() { Name = "Israel Shekel", Symbol = "₪"}},
            { CurrencySymbol.KRW, new CurrencyDisplay() { Name = "Korea (South) Won", Symbol = "₩"}},
            { CurrencySymbol.PLN, new CurrencyDisplay() { Name = "Poland Zloty", Symbol = "zł"}},
        };
    }
}
