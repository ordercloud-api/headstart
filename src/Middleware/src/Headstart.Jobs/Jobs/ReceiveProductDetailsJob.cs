using System;
using System.Linq;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Common.Extensions;
using Headstart.Common.Repositories;
using ordercloud.integrations.library;
using Headstart.Common.Repositories.Models;

namespace Headstart.Jobs
{
	public class ReceiveProductDetailsJob : BaseTimerJob
	{
		private readonly IOrderCloudClient _oc;
		private readonly IProductDetailDataRepo _productDetailRepo;
		private readonly ILineItemDetailDataRepo _lineItemDetailDataRepo;

		public ReceiveProductDetailsJob(IOrderCloudClient oc, IProductDetailDataRepo productDetailRepo, ILineItemDetailDataRepo lineItemDetailDataRepo)
		{
			_oc = oc;
			_productDetailRepo = productDetailRepo;
			_lineItemDetailDataRepo = lineItemDetailDataRepo;
		}
		protected override bool ShouldRun => true;

		protected override async Task ProcessJob()
		{
			try
			{
				await UpsertProductDetail();
			}
			catch (Exception ex)
			{
				LogFailure($@"{ex.Message} {ex?.InnerException?.Message} {ex.StackTrace}");
			}
		}

		private async Task UpsertProductDetail()
		{
			var retrievedProductList = await _oc.Products.ListAllAsync<Product>( filters: @"Name=*");

			foreach (var product in retrievedProductList)
			{
				try
				{
					await ProcessProductAsync(product);
				}
				catch (Exception ex)
				{
					LogFailure(ex.Message);
				}
			}
		}

		private async Task ProcessProductAsync(Product product)
		{
			var lineItemData = await GetLineItemDataAsync(product.ID);
			var productDataList = await CreateProductDetailDataAsync(product, lineItemData?.Items);

			//Get current products in Cosmos to update/replace
			var requestOptions = BuildQueryRequestOptions();
			foreach (var productData in productDataList)
			{
				var queryable = _productDetailRepo.GetQueryable().Where(x => x.PartitionKey == "PartitionValue" && x.Data.SpecCombo == productData.Data.SpecCombo);

				var listOptions = BuildProductListOptions(productData.ProductId, productData.Data.SpecCombo);

				var currentProductDetailListPage = await _productDetailRepo.GetItemsAsync(queryable, requestOptions, listOptions);
				var cosmosId = string.Empty;
				if (currentProductDetailListPage.Items.Count == 1)
				{
					cosmosId = productData.id = currentProductDetailListPage.Items[0].id;
				}

				await _productDetailRepo.UpsertItemAsync(cosmosId, productData);
			}
		}

		private QueryRequestOptions BuildQueryRequestOptions()
		{
			return new QueryRequestOptions()
			{
				MaxItemCount = 1,
			};
		}
		private async Task<CosmosListPage<LineItemDetailData>> GetLineItemDataAsync(string productID)
		{
			var queryable = _lineItemDetailDataRepo
				.GetQueryable()
				.Where(order => order.Data.LineItems.Any(lineItem => lineItem.ProductID == productID) && order.Data.Order.DateCreated > DateTime.Now.AddMonths(-12));

			var requestOptions = new QueryRequestOptions()
			{
				MaxItemCount = 1
			};
			var listOptions = new CosmosListOptions()
			{
				PageSize = 100, ContinuationToken = null
			};
			var currentLineItemListPage = await _lineItemDetailDataRepo.GetItemsAsync(queryable, requestOptions, listOptions);
			return currentLineItemListPage;
		}

		public CosmosListOptions BuildProductListOptions(string productID, string specCombo)
		{
			ListFilter currentSpecComboFilter = null;
			var currentProductFilter = new ListFilter("ProductID", productID);
			if (specCombo != null)
			{
				currentSpecComboFilter = new ListFilter("Data.SpecCombo", specCombo);
				return new CosmosListOptions()
				{
					PageSize = 1,
					ContinuationToken = null,
					Filters =
					{
						currentProductFilter, currentSpecComboFilter
					}
				};
			}
			else
			{
				return new CosmosListOptions()
				{
					PageSize = 1,
					ContinuationToken = null,
					Filters =
					{
						currentProductFilter
					}
				};
			}
		}

		private async Task<List<ProductDetailData>> CreateProductDetailDataAsync(Product product, List<LineItemDetailData> lineItemList)
		{
			List<ProductDetailData> resultList = new List<ProductDetailData>();
			Task<List<Spec>> specList = _oc.Products.ListAllSpecsAsync(product.ID);
			Task<List<Variant>> variantList = _oc.Products.ListAllVariantsAsync(product.ID);
			await Task.WhenAll(specList, variantList);
			Supplier supplier = new Supplier();
			try
			{
				supplier = await _oc.Suppliers.GetAsync(product.DefaultSupplierID);
			} 
			catch (Exception ex)
			{
				LogFailure(ex.Message);
			}

			if (product.Inventory != null && product.Inventory.VariantLevelTracking)
			{
				foreach (var variant in variantList.Result)
				{
					var data = new ProductDetailData()
					{
						ProductId = product.ID,
						PartitionKey = @"PartitionValue",
						ProductSales = GetProductSales(product.ID, lineItemList),
						Product = product,
						Data = await FlattenProductDataDetailAsync(product, supplier, variant)
					};
					resultList.Add(data);
				}
			}
			else
			{
				resultList.Add(new ProductDetailData
				{
					ProductId = product.ID,
					PartitionKey = @"PartitionValue",
					ProductSales = GetProductSales(product.ID, lineItemList),
					Product = product,
					Data = await FlattenProductDataDetailAsync(product, supplier)
				});
			}
           
			return resultList;
		}

		private async Task<HSProductDetail> FlattenProductDataDetailAsync(Product product, Supplier supplier, Variant variant = null)
		{
			var result = new HSProductDetail();
			var productXp = product.xp;
			var schedule = await GetPriceSchedule(product.DefaultPriceScheduleID);
			result.SizeTier = string.Empty;
			result.Active = product?.Active.ToString();
			if (PropertyExists(productXp, @"Status"))
			{
				result.Status = productXp.Status?.ToString();
			}

			if (PropertyExists(productXp, @"Note"))
			{
				result.Note = productXp.Note;
			}
			if (PropertyExists(productXp, @"Tax"))
			{
				result.TaxCode = productXp?.Tax?.Code;
				result.TaxDescription = productXp?.Tax?.Description;
				result.TaxCategory = productXp?.Tax?.Category;
			}
			if (PropertyExists(productXp, @"UnitOfMeasure"))
			{
				result.UnitOfMeasureQty = productXp?.UnitOfMeasure?.Qty;
				result.UnitOfMeasure = productXp?.UnitOfMeasure?.Unit;
			}

			if (PropertyExists(productXp, @"ProductType"))
			{
				result.ProductType = productXp.ProductType;
			}

			if (PropertyExists(productXp, @"SizeTier"))
			{
				result.SizeTier = productXp.SizeTier;
			}

			if (PropertyExists(productXp, @"IsResale"))
			{
				result.Resale = productXp.IsResale;
			}

			if (PropertyExists(productXp, @"Currency"))
			{
				result.Currency = productXp.Currency;
			}

			if (PropertyExists(productXp, @"ArtworkRequired"))
			{
				result.ArtworkRequired = productXp.ArtworkRequired;
			}
			if (supplier != null)
			{
				result.SupplierId = supplier?.ID;
				result.SupplierName = supplier?.Name;
			}
          
			var price = GetPrice(schedule, variant);
			result.Price = price * (decimal)1.06; //SEB markup of 6%
			result.Cost = price;
			if (product.Inventory != null)
			{
				result.VariantLevelTracking = product.Inventory.VariantLevelTracking;
				result.InventoryOrderCanExceed = product.Inventory.OrderCanExceed;
			}
			if (schedule != null)
			{
				result.PriceScheduleId = schedule?.ID;
				result.PriceScheduleName = schedule?.Name;
			}

			if (variant != null)
			{
				result.VariantId = variant.ID;
				result.VariantName = variant.Name;
				result.VariantActive = variant.Active;
				result.SpecCombo = variant.xp?.SpecCombo;
				result.SpecName = variant.xp?.SpecCombo;
				result.SpecOptionValue = GetSpecOptionValue(variant.xp?.SpecValues).Trim();
				result.SpecPriceMarkup = GetSpecPriceMarkup(variant.xp?.SpecValues).Trim();
				result.InventoryQuantity = variant.Inventory?.QuantityAvailable;
				result.InventoryLastUpdated = variant.Inventory?.LastUpdated;
			}
			else
			{
				result.InventoryQuantity = product.Inventory?.QuantityAvailable;
				result.InventoryLastUpdated = product.Inventory?.LastUpdated;
			}
			return result;
		}

		private decimal GetPrice(PriceSchedule schedule, Variant variant)
		{
			decimal totalPrice = 0;
			if (variant == null || !variant.Specs.HasItem())
			{
				if (!schedule.PriceBreaks.HasItem())
				{
					return totalPrice;
				}
				return schedule.PriceBreaks[0].Price;
			}
			foreach (var spec in variant.Specs)
			{
				if (spec.PriceMarkup == null)
				{
					continue;
				}
				switch (spec.PriceMarkupType)
				{
					case PriceMarkupType.AmountPerQuantity:
						totalPrice += (decimal)spec.PriceMarkup;
						break;
					case PriceMarkupType.AmountTotal:
						totalPrice += (decimal)spec.PriceMarkup;
						break;
					case PriceMarkupType.NoMarkup:
						break;
					default:
						totalPrice += (decimal)spec.PriceMarkup;
						break;
				}
			}
			return totalPrice;
		}

		private bool PropertyExists(dynamic obj, string property)
		{
			var objDictionary = ((IDictionary<string, object>) obj);
			return objDictionary.ContainsKey(property);
		}
		private async Task<PriceSchedule> GetPriceSchedule(string defaultPriceScheduleID)
		{
			return await _oc.PriceSchedules.GetAsync(defaultPriceScheduleID);
		}

		private string GetSpecPriceMarkup(List<dynamic> specValues)
		{
			decimal totalPriceMarkup = 0;
			foreach (dynamic specValue in specValues)
			{
				if (specValue?.PriceMarkup?.Trim() != "")
				{
					decimal numericMarkup = 0;
					bool isNumeric = decimal.TryParse(specValue?.PriceMarkup?.Trim(), out numericMarkup);
					if (isNumeric)
					{
						totalPriceMarkup += numericMarkup;
					}
				}
			}
			return totalPriceMarkup.ToString();
		}

		private string GetSpecOptionValue(List<dynamic> specValues)
		{
			string specOptionValue = "";
			foreach (dynamic specValue in specValues)
			{
				if (specValue?.SpecOptionValue != "")
				{
					specOptionValue = specOptionValue + " " + specValue?.SpecOptionValue + " ";
				}
			}
			return specOptionValue;
		}

		private ProductSaleDetail GetProductSales(string productID, List<LineItemDetailData> lineItemList)
		{
			var result = new ProductSaleDetail();
			if (!lineItemList.HasItem())
			{
				return result;
			}
			var twelveMonthLineItems = lineItemList?.Where(x => x.Data.LineItems.Any(y => y.Product.ID == productID)).ToList();
			var sixMonthLineItems = twelveMonthLineItems.Where(x => x.Data.Order.DateCreated > DateTime.Now.AddMonths(-6)).ToList();
			var threeMonthLineItems = twelveMonthLineItems.Where(x => x.Data.Order.DateCreated > DateTime.Now.AddMonths(-3)).ToList();

			//3MO sales
			foreach (var lineItemDetail in threeMonthLineItems)
			{
				result.ThreeMonthQuantity = lineItemDetail.Data.LineItems.Sum(x => x.Quantity);
				result.ThreeMonthTotal = lineItemDetail.Data.LineItems.Sum(x => x.LineSubtotal);
			}
           
			//6MO sales
			foreach (var lineItemDetail in sixMonthLineItems)
			{
				result.SixMonthQuantity = lineItemDetail.Data.LineItems.Sum(x => x.Quantity);
				result.SixMonthTotal = lineItemDetail.Data.LineItems.Sum(x => x.LineSubtotal);
			}

			//12MO sales
			foreach (var lineItemDetail in twelveMonthLineItems)
			{
				result.TwelveMonthQuantity = lineItemDetail.Data.LineItems.Sum(x => x.Quantity);
				result.TwelveMonthTotal = lineItemDetail.Data.LineItems.Sum(x => x.LineSubtotal);
			}
			return result;
		}
	}
}
