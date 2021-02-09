using System;
using System.Collections.Generic;
using ordercloud.integrations.library;

namespace Headstart.Models
{
    [SwaggerModel]
    public class SupplierFilterConfig
    {
        /// <summary>
        /// Text for UI display
        /// </summary>
        public string Display { get; set; }

        /// <summary>
        /// Path to filter value from root of object
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Potential Options for Field
        /// </summary>
        public List<Filter> Items { get; set; }

        /// <summary>
        /// should the supplier be able to edit the options on their own supplier
        /// </summary>
        public bool AllowSupplierEdit { get; set; }

        /// <summary>
        /// should the seller be able to edit the options on suppliers
        /// </summary>
        public bool AllowSellerEdit { get; set; }


        // Either SelectOption or NonUi
        // we can't use an enum because it comes through as int and json validator expects string
        public string BuyerAppFilterType { get; set; }
    }

    [SwaggerModel]
    public class Filter
    {
        public string Text {get; set;}
        public string Value {get; set; }
    }
}


