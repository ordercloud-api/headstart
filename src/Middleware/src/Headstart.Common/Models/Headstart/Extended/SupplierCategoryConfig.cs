using System;
using Cosmonaut.Attributes;
using System.Collections.Generic;
using ordercloud.integrations.library;

namespace Headstart.Common.Models.Headstart.Extended
{
	[CosmosCollection("suppliercategoryconfigs")]
	public class SupplierCategoryConfig : ICosmosObject
	{
		public SupplierCategoryConfig()
		{
		}

		public string id { get; set; } = string.Empty;

		public string HSName { get; set; } = string.Empty;

		public DateTimeOffset timeStamp { get; set; }

		public IEnumerable<SupplierCategoriesFilter> Filters { get; set; } = new List<SupplierCategoriesFilter>();
	}

	public class SupplierCategoriesFilter
	{
		public string Path { get; set; } = string.Empty;

		public string Display { get; set; } = string.Empty;

		public IEnumerable<SupplierCategoriesFilterItem> Items { get; set; } = new List<SupplierCategoriesFilterItem>();
	}

	public class SupplierCategoriesFilterItem
	{
		public string Text { get; set; } = string.Empty;

		public string Value { get; set; } = string.Empty;
	}
}