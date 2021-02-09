using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoListCurrencyList : ZohoListResponse
    {
        [JsonProperty(PropertyName = "currencies")]
        public List<ZohoCurrency> Items { get; set; }
    }

    public class ZohoCurrency
    {
        /// <summary>
        /// Gets or sets the currency_id.
        /// </summary>
        /// <value>The currency_id.</value>
        public string currency_id { get; set; }
        /// <summary>
        /// Gets or sets the currency_code.
        /// </summary>
        /// <value>The currency_code.</value>
        public string currency_code { get; set; }
        /// <summary>
        /// Gets or sets the currency_name.
        /// </summary>
        /// <value>The currency_name.</value>
        public string currency_name { get; set; }
        /// <summary>
        /// Gets or sets the currency_symbol.
        /// </summary>
        /// <value>The currency_symbol.</value>
        public string currency_symbol { get; set; }
        /// <summary>
        /// Gets or sets the price_precision.
        /// </summary>
        /// <value>The price_precision.</value>
        public int price_precision { get; set; }
        /// <summary>
        /// Gets or sets the currency_format.
        /// </summary>
        /// <value>The currency_format.</value>
        public string currency_format { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ZohoCurrency"/> is is_base_currency.
        /// </summary>
        /// <value><c>true</c> if is_base_currency; otherwise, <c>false</c>.</value>
        public bool is_base_currency { get; set; }
        /// <summary>
        /// Gets or sets the exchange_rate.
        /// </summary>
        /// <value>The exchange_rate.</value>
        public double exchange_rate { get; set; }
        /// <summary>
        /// Gets or sets the effective_date.
        /// </summary>
        /// <value>The effective_date.</value>
        public string effective_date { get; set; }

    }
}
