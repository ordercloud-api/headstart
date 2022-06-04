using System.Text.Json.Serialization;

namespace Headstart.Common.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CurrencyCode
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
        PLN,
    }
}
