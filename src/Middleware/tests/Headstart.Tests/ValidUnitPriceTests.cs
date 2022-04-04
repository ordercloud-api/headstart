﻿using Headstart.API.Commands;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Models.Headstart;

namespace Headstart.Tests
{
	class ValidUnitPriceTests
	{
		private IOrderCloudClient _oc;
		private LineItemCommand _commandSub;
		private List<HsLineItem> _existingLineItems;

		[SetUp]
		public void Setup()
		{
			_oc = Substitute.For<IOrderCloudClient>();
			_commandSub = Substitute.ForPartsOf<LineItemCommand>(default, _oc, default, default, default, default);
			_existingLineItems = BuildMockExistingLineItemData(); // Mock data consists of two total line items for one product (with different specs)
			Substitute.For<ILineItemsResource>().PatchAsync<HsLineItem>(OrderDirection.Incoming, default, default, default).ReturnsForAnyArgs((Task)null);
		}

		[Test]
		public async Task GetUnitPrice_FirstPriceBreak_NoMarkups_CumulativeQtyFalse()
		{
			SuperHsMeProduct product = BuildMockProductData(false);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(4, 0); // Existing line items with different specs (quantity 2) cannot combine with this quantity (4).  Does not hit discount price break (minimum quantity 5).

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 5);
		}

		[Test]
		public async Task GetUnitPrice_SecondPriceBreak_NoMarkups_CumulativeQtyFalse()
		{
			SuperHsMeProduct product = BuildMockProductData(false);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(5, 0);

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 3.5);
		}

		[Test]
		public async Task GetUnitPrice_FirstPriceBreak_OneMarkup_CumulativeQtyFalse()
		{
			SuperHsMeProduct product = BuildMockProductData(false);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(4, 1); // Existing line items with different specs (quantity 2) cannot combine with this quantity (4).  Does not hit discount price break (minimum quantity 5).

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 7.25);
		}

		[Test]
		public async Task GetUnitPrice_SecondPriceBreak_OneMarkup_CumulativeQtyFalse()
		{
			SuperHsMeProduct product = BuildMockProductData(false);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(5, 1);

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 5.75);
		}

		[Test]
		public async Task GetUnitPrice_FirstPriceBreak_TwoMarkups_CumulativeQtyFalse()
		{
			SuperHsMeProduct product = BuildMockProductData(false);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(4, 2); // Existing line items with different specs (quantity 2) cannot combine with this quantity (4).  Does not hit discount price break (minimum quantity 5).

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 11.25);
		}

		[Test]
		public async Task GetUnitPrice_SecondPriceBreak_TwoMarkups_CumulativeQtyFalse()
		{
			SuperHsMeProduct product = BuildMockProductData(false);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(5, 2);

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 9.75);
		}

		[Test]
		public async Task GetUnitPrice_FirstPriceBreak_NoMarkups_CumulativeQtyTrue()
		{
			SuperHsMeProduct product = BuildMockProductData(true);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(2, 0); // Does not hit discount price break (minimum quantity 5) when adding existing line item quantity (2)

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 5);
		}

		[Test]
		public async Task GetUnitPrice_SecondPriceBreak_NoMarkups_CumulativeQtyTrue()
		{
			SuperHsMeProduct product = BuildMockProductData(true);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(3, 0); // Hits discount price break (minimum quantity 5) when adding existing line item quantity (2)

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 5m);
		}

		[Test]
		public async Task GetUnitPrice_FirstPriceBreak_OneMarkup_CumulativeQtyTrue()
		{
			SuperHsMeProduct product = BuildMockProductData(true);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(2, 1); // Does not hit discount price break (minimum quantity 5) when adding existing line item quantity (2)

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 7.25);
		}

		[Test]
		public async Task GetUnitPrice_SecondPriceBreak_OneMarkup_CumulativeQtyTrue()
		{
			SuperHsMeProduct product = BuildMockProductData(true);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(3, 1);  // Hits discount price break (minimum quantity 5) when adding existing line item quantity (2)

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 7.25m);
		}

		[Test]
		public async Task GetUnitPrice_FirstPriceBreak_TwoMarkups_CumulativeQtyTrue()
		{
			SuperHsMeProduct product = BuildMockProductData(true);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(2, 2); // Does not hit discount price break (minimum quantity 5) when adding existing line item quantity (2)

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 11.25);
		}

		[Test]
		public async Task GetUnitPrice_SecondPriceBreak_TwoMarkups_CumulativeQtyTrue()
		{
			SuperHsMeProduct product = BuildMockProductData(true);

			HsLineItem lineItem = SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(3, 2);  // Hits discount price break (minimum quantity 5) when adding existing line item quantity (2)

			decimal lineItemTotal = await _commandSub.ValidateLineItemUnitCost(default, product, _existingLineItems, lineItem);

			Assert.AreEqual(lineItemTotal, 11.25m);
		}

		private SuperHsMeProduct BuildMockProductData(bool UseCumulativeQty)
		{
			SuperHsMeProduct product = Substitute.For<SuperHsMeProduct>();
			product.PriceSchedule = Substitute.For<PriceSchedule>();
			product.PriceSchedule.UseCumulativeQuantity = UseCumulativeQty;

			PriceBreak priceBreak1 = Substitute.For<PriceBreak>();
			priceBreak1.Quantity = 1;
			priceBreak1.Price = 5;

			PriceBreak priceBreak2 = Substitute.For<PriceBreak>();
			priceBreak2.Quantity = 5;
			priceBreak2.Price = 3.5M;

			product.PriceSchedule.PriceBreaks = new List<PriceBreak> { priceBreak1, priceBreak2 };

			Spec prodSpecSize = Substitute.For<Spec>();
			prodSpecSize.ID = "Size";
			prodSpecSize.Options = new List<SpecOption>() { new PartialSpecOption { ID = "Small", Value = "Small" }, new PartialSpecOption { ID = "Medium", Value = "Medium" }, new PartialSpecOption { ID = "Large", Value = "Large", PriceMarkup = 2.25M } };

			Spec prodSpecColor = Substitute.For<Spec>();
			prodSpecColor.ID = "Color";
			prodSpecColor.Options = new List<SpecOption>() { new PartialSpecOption { ID = "Blue", Value = "Blue" }, new PartialSpecOption { ID = "Red", Value = "Red" }, new PartialSpecOption { ID = "Green", Value = "Green", PriceMarkup = 4 } };

			product.Specs = new List<Spec> { prodSpecSize, prodSpecColor };

			return product;
		}

		private List<HsLineItem> BuildMockExistingLineItemData()
		{
			HsLineItem existingLineItem1 = Substitute.For<HsLineItem>();
			existingLineItem1.Quantity = 1;
			existingLineItem1.xp = new LineItemXp()
			{
				PrintArtworkUrl = null
			};

			HsLineItem existingLineItem2 = Substitute.For<HsLineItem>();
			existingLineItem2.Quantity = 1;
			existingLineItem2.xp = new LineItemXp()
			{
				PrintArtworkUrl = null
			};

			LineItemSpec liSpecSizeSmall = Substitute.For<LineItemSpec>();
			liSpecSizeSmall.SpecID = "Size";
			liSpecSizeSmall.OptionID = "Small";
			liSpecSizeSmall.Value = "Small";

			LineItemSpec liSpecSizeMedium = Substitute.For<LineItemSpec>();
			liSpecSizeMedium.SpecID = "Size";
			liSpecSizeMedium.OptionID = "Medium";
			liSpecSizeMedium.Value = "Medium";

			LineItemSpec liSpecColorRed = Substitute.For<LineItemSpec>();
			liSpecColorRed.SpecID = "Color";
			liSpecColorRed.OptionID = "Red";
			liSpecColorRed.Value = "Red";

			existingLineItem1.Specs = new List<LineItemSpec> { liSpecSizeSmall, liSpecColorRed };
			existingLineItem2.Specs = new List<LineItemSpec> { liSpecSizeMedium, liSpecColorRed };

			List<HsLineItem> existingLineItems = new List<HsLineItem> { existingLineItem1, existingLineItem2 };

			return existingLineItems;
		}

		private HsLineItem SetMockLineItemQtyAndMockNumberOfMarkedUpSpecs(int quantity, int numberOfMarkedUpSpecs)
		{
			HsLineItem lineItem = Substitute.For<HsLineItem>();

			lineItem.Quantity = quantity;

			LineItemSpec liSpecSizeSmall = Substitute.For<LineItemSpec>();
			liSpecSizeSmall.SpecID = "Size";
			liSpecSizeSmall.OptionID = "Small";

			LineItemSpec liSpecSizeLarge = Substitute.For<LineItemSpec>();
			liSpecSizeLarge.SpecID = "Size";
			liSpecSizeLarge.OptionID = "Large";

			LineItemSpec liSpecColorBlue = Substitute.For<LineItemSpec>();
			liSpecColorBlue.SpecID = "Color";
			liSpecColorBlue.OptionID = "Blue";

			LineItemSpec liSpecColorGreen = Substitute.For<LineItemSpec>();
			liSpecColorGreen.SpecID = "Color";
			liSpecColorGreen.OptionID = "Green";

			if (numberOfMarkedUpSpecs == 0)
			{
				lineItem.Specs = new List<LineItemSpec> { liSpecSizeSmall, liSpecColorBlue };
			} else if (numberOfMarkedUpSpecs == 1)
			{
				lineItem.Specs = new List<LineItemSpec> { liSpecSizeLarge, liSpecColorBlue };
			} else if (numberOfMarkedUpSpecs == 2)
			{
				lineItem.Specs = new List<LineItemSpec> { liSpecSizeLarge, liSpecColorGreen };
			} else
			{
				throw new Exception("The number of marked up specs for this unit test must be 0, 1, or 2");
			}

			return lineItem;
		}
	}

}
