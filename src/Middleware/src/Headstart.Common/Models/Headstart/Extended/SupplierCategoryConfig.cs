using System;
using System.Collections.Generic;
using Cosmonaut.Attributes;
using ordercloud.integrations.library;

namespace Headstart.Models.Extended
{
	[CosmosCollection("suppliercategoryconfigs")]
	public class SupplierCategoryConfig : ICosmosObject
    {
        public SupplierCategoryConfig()
        {
        }

        public string id { get; set; }
        public DateTimeOffset timeStamp { get; set; }
        public string HSName { get; set; }
        public IEnumerable<SupplierCategoriesFilter> Filters { get; set; }
    }

    public class SupplierCategoriesFilter
    {
        public string Display { get; set; }
        public string Path { get; set; }
        public IEnumerable<SupplierCategoriesFilterItem> Items { get; set; }
    }

    public class SupplierCategoriesFilterItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}


