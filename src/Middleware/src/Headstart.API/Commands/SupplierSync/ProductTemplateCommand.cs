using System;
using System.Linq;
using Npoi.Mapper;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Newtonsoft.Json.Linq;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Headstart.Common.Models.Base;
using ordercloud.integrations.library;
using ordercloud.integrations.easypost;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Services.CMS.Models;
using System.ComponentModel.DataAnnotations;
using ordercloud.integrations.exchangerates;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;
using IPartial = ordercloud.integrations.library.IPartial;

namespace Headstart.API.Commands.SupplierSync
{
	public interface IProductTemplateCommand
	{
		Task<List<TemplateHydratedProduct>> ParseProductTemplate(IFormFile file, DecodedToken decodedToken);
		Task<TemplateProductResult> ParseProductTemplateFlat(IFormFile file, DecodedToken decodedToken);
	}

	public class ProductTemplateCommand : IProductTemplateCommand
	{
		private readonly AppSettings _settings;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The Default constructor method for the ProductTemplateCommand class object
		/// </summary>
		/// <param name="settings"></param>
		public ProductTemplateCommand(AppSettings settings)
		{            
			try
			{
				_settings = settings;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ParseProductTemplateFlat task method
		/// </summary>
		/// <param name="file"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The TemplateProductResult response object for the Parse Product Template Flat process</returns>
		public async Task<TemplateProductResult> ParseProductTemplateFlat(IFormFile file, DecodedToken decodedToken)
		{
			var resp = new TemplateProductResult();
			try
			{
				using var stream = file.OpenReadStream();
				var products = new Mapper(stream).Take<TemplateProductFlat>("TemplateFlat", 1000).ToList();
				var result = Validate(products.Where(p => p.Value?.ID != null).Select(p => p).ToList());
				resp = await Task.FromResult(result);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable Validate task method
		/// </summary>
		/// <param name="rows"></param>
		/// <returns>The TemplateProductResult response object for the Validate process</returns>
		public TemplateProductResult Validate(List<RowInfo<TemplateProductFlat>> rows)
		{
			var result = new TemplateProductResult()
			{
				Invalid = new List<TemplateRowError>(),
				Valid = new List<TemplateProductFlat>()
			};
			try
			{
				foreach (var row in rows)
				{
					if (row.ErrorColumnIndex > -1)
					{
						result.Invalid.Add(new TemplateRowError()
						{
							ErrorMessage = row.ErrorMessage,
							Row = row.RowNumber++
						});
					}
					else
					{
						var results = new List<ValidationResult>();
						if (Validator.TryValidateObject(row.Value, new ValidationContext(row.Value), results, true) == false)
						{
							result.Invalid.Add(new TemplateRowError()
							{
								ErrorMessage = $"{results.FirstOrDefault()?.ErrorMessage}",
								Row = row.RowNumber++
							});
						}
						else
						{
							result.Valid.Add(row.Value);
						}
					}
				}

				result.Meta = new TemplateParseSummary()
				{
					InvalidCount = result.Invalid.Count,
					ValidCount = result.Valid.Count,
					TotalCount = rows.Count
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}            
			return result;
		}

		/// <summary>
		/// Public re-usable ParseProductTemplate task method
		/// </summary>
		/// <param name="file"></param>
		/// <param name="decodedToken"></param>
		/// <returns>Thelist of TemplateProductResult response objects for the Parse Product Template process</returns>
		public async Task<List<TemplateHydratedProduct>> ParseProductTemplate(IFormFile file, DecodedToken decodedToken)
		{
			var list = new List<TemplateHydratedProduct>();
			try
			{
				using var stream = file.OpenReadStream();
				var mapper = new Mapper(stream);
				var products = mapper.Take<TemplateProduct>("Products").Select(s => s.Value).ToList();
				var prices = mapper.Take<TemplatePriceSchedule>("PriceSchedules").Select(s => s.Value).ToList().AsReadOnly();
				var specs = mapper.Take<TemplateSpec>("Specs").Select(s => s.Value).ToList().AsReadOnly();
				var specoptions = mapper.Take<TemplateSpecOption>("SpecOptions").Select(s => s.Value).ToList().AsReadOnly();
				var images = mapper.Take<TemplateAsset>("Images").Select(s => s.Value).ToList().AsReadOnly();
				var attachments = mapper.Take<TemplateAsset>("Attachments").Select(s => s.Value).ToList().AsReadOnly();

				foreach (var product in products)
				{
					var p = new TemplateHydratedProduct();
					p.Product = product;
					p.PriceSchedule = prices.FirstOrDefault(row => row.ProductID == product.ID);
					p.Images = images.Where(i => i.ProductID == product.ID).Select(o => o).ToList();
					p.Attachments = attachments.Where(s => s.ProductID == product.ID).Select(o => o).ToList();
					p.Specs = specs.Where(s => s.ProductID == product.ID).Select(s => s).ToList();
					// 1:38
					//var o = from options in specoptions
					//    join sp in specs on options.SpecID equals sp.ID
					//    join pp in products on sp.ProductID equals pp.ID
					//        select options;
					// :53
					var o = specoptions.Where(x => p.Specs.Any(s => s.ID == x.SpecID));
					p.SpecOptions = o.ToList();
					list.Add(p);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return await Task.FromResult(list.ToList());
		}
	}

	public class TemplateHydratedProduct
	{
		public TemplateProduct Product { get; set; }
		public TemplatePriceSchedule PriceSchedule { get; set; }
		public IList<TemplateSpec> Specs { get; set; }
		public IList<TemplateSpecOption> SpecOptions { get; set; }
		public IList<TemplateAsset> Images { get; set; }
		public IList<TemplateAsset> Attachments { get; set; }
	}

	public class TemplateProductResult
	{
		public TemplateParseSummary Meta { get; set; }
		public List<TemplateProductFlat> Valid = new List<TemplateProductFlat>();
		public List<TemplateRowError> Invalid = new List<TemplateRowError>();
	}

	public class TemplateParseSummary
	{
		public int TotalCount { get; set; }
		public int ValidCount { get; set; }
		public int InvalidCount { get; set; }
	}

	public class TemplateRowError
	{
		public int Row { get; set; }
		public string ErrorMessage { get; set; }
	}

	public class PartialTemplateProductFlat : TemplateProductFlat, IPartial
	{
		public JObject Values { get; set; }
	}
    
	public class TemplateProductFlat : HsBaseObject
	{
		[OrderCloud.SDK.Required]
		[RegularExpression("^[a-zA-Z0-9-_]*$", ErrorMessage = "IDs must have at least 8 characters and no more than 100, are required and can only contain characters Aa-Zz, 0-9, -, and _")]
		public string ID { get; set; }
		public bool Active { get; set; }
		public ProductType ProductType { get; set; }

		[OrderCloud.SDK.Required]
		public string Name { get; set; }
		[MaxSize(2000)]
		public string Description { get; set; }
		[MinValue(1)]
		public int QuantityMultiplier { get; set; }

		public string ShipFromAddressID { get; set; }
		public decimal? ShipWeight { get; set; }
		public decimal? ShipHeight { get; set; }
		public decimal? ShipWidth { get; set; }
		public decimal? ShipLength { get; set; }
		[OrderCloud.SDK.Required]
		public string TaxCategory { get; set; }
		[OrderCloud.SDK.Required]
		public string TaxCode { get; set; }
		public string TaxDescription { get; set; }
		public int UnitOfMeasureQuantity { get; set; }
		public string UnitOfMeasure { get; set; }
		public bool IsResale { get; set; }
		public bool ApplyTax { get; set; }
		public bool ApplyShipping { get; set; }
		public int? MinQuantity { get; set; }
		public int? MaxQuantity { get; set; }
		public bool UseCumulativeQuantity { get; set; }
		public bool RestrictedQuantity { get; set; }
		[OrderCloud.SDK.Required]
		public decimal? Price { get; set; }
		public CurrencySymbol Currency { get; set; }
		public string ImageTitle { get; set; }
		public string Url { get; set; }
		public AssetType Type { get; set; }
		public string Tags { get; set; }
		public string FileName { get; set; }
		public SizeTier SizeTier { get; set; }
	}

	public class TemplateProduct
	{
		public string ID { get; set; }
		public bool Active { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int QuantityMultiplier { get; set; }
		public decimal? ShipWeight { get; set; }
		public decimal? ShipHeight { get; set; }
		public decimal? ShipWidth { get; set; }
		public decimal? ShipLength { get; set; }
		public string TaxCategory { get; set; }
		public string TaxCode { get; set; }
		public string TaxDescription { get; set; }
		public string UnitOfMeasureQty { get; set; }
		public string UnitOfMeasure { get; set; }
		public bool IsResale { get; set; }
		public CurrencySymbol Currency { get; set; }
		public SizeTier SizeTier { get; set; }
	}

	public class TemplatePriceSchedule
	{
		public string ID { get; set; }
		public string ProductID { get; set; }
		public string Name { get; set; }
		public bool ApplyTax { get; set; }
		public bool ApplyShipping { get; set; }
		public int MinQuantity { get; set; }
		public int MaxQuantity { get; set; }
		public bool UseCumulativeQuantity { get; set; }
		public bool RestrictedQuantity { get; set; }
	}

	public class TemplateSpec
	{
		public string ID { get; set; }
		public string ProductID { get; set; }
		public string Name { get; set; }
		public int ListOrder { get; set; }
		public string DefaultValue { get; set; }
		public bool Required { get; set; }
		public bool AllowOpenText { get; set; }
		public string DefaultOptionID { get; set; }
		public bool DefinesVariant { get; set; }
	}

	public class TemplateSpecOption
	{
		public string ID { get; set; }
		public string SpecID { get; set; }
		public string Value { get; set; }
		public int ListOrder { get; set; }
		public bool IsOpenText { get; set; }
		public PriceMarkupType PriceMarkupType { get; set; }
		public decimal? PriceMarkup { get; set; }
		public string Description { get; set; }
	}

	public class TemplateAsset
	{
		public string ID { get; set; }
		public string ProductID { get; set; }
		public string Title { get; set; }
		public bool Active { get; set; }
		public string Url { get; set; }
		public AssetType Type { get; set; }
		public List<string> Tags { get; set; } = new List<string>();
		public string FileName { get; set; }
	}
}