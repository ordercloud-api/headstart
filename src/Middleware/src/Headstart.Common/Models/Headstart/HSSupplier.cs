using System;
using OrderCloud.SDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Headstart.Models.Headstart.Extended;
using ordercloud.integrations.exchangerates;

namespace Headstart.Models.Headstart
{
    public class HSSupplier : Supplier<SupplierXp>, IHSObject { }
    
    public class SupplierXp
    {
        public string Description { get; set; } = string.Empty;

        public Contact SupportContact { get; set; } = new Contact();

        public bool SyncFreightPop { get; set; }

        public string ApiClientID { get; set; } = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
		public CurrencySymbol? Currency { get; set; }

        public List<ProductType> ProductTypes { get; set; } = new List<ProductType>();

        public List<string> CountriesServicing { get; set; } = new List<string>();

        public List<string> BuyersServicing { get; set; } = new List<string>();

        public List<SupplierCategory> Categories { get; set; } = new List<SupplierCategory>();


        public List<string> NotificationRcpts { get; set; } = new List<string>();

        public Nullable<int> FreeShippingThreshold { get; set; }

        public ImageAsset Image { get; set; } = new ImageAsset();
    }

    
    public class SupplierCategory
    {
        public string ServiceCategory { get; set; } = string.Empty;

        public string VendorLevel { get; set; } = string.Empty;
    }
}