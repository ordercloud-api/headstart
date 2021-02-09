using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Headstart.Common.Exceptions;
using Headstart.Common.Helpers;
using Headstart.Common.Models;
using Newtonsoft.Json.Linq;
using Headstart.Common.Queries;
using OrderCloud.SDK;
using Headstart.Models;
using Headstart.Models.Extended;
using ordercloud.integrations.library;
using Headstart.Common.Services.CMS.Models;
using Headstart.Common.Services.CMS;
using Headstart.API.Commands;
using Headstart.Common;

namespace Headstart.Orchestration
{
    public class TemplateProductFlatSyncCommand : SyncCommand, IWorkItemCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly ICMSClient _cms;
        private readonly IOrderCloudIntegrationsFunctionToken _token;
        private VerifiedUserContext _user;

        public TemplateProductFlatSyncCommand(AppSettings settings, LogQuery log, IOrderCloudClient oc, ICMSClient cms) : base(settings, oc, cms, log)
        {
            _oc = oc;
            _cms = cms;
            _token = new OrderCloudIntegrationsFunctionToken(settings);
        }

        public async Task<JObject> CreateAsync(WorkItem wi)
        {
            var obj = wi.Current.ToObject<TemplateProductFlat>(OrchestrationSerializer.Serializer);
            _user ??= await _token.Authorize(wi.Token, new[] {ApiRole.ProductAdmin, ApiRole.PriceScheduleAdmin});
            try
            {
                obj.ID ??= wi.RecordId;
                var priceSchedule = await _oc.PriceSchedules.SaveAsync<HSPriceSchedule>(obj.ID, new HSPriceSchedule()
                {
                    ID = obj.ID,
                    ApplyShipping = obj.ApplyShipping,
                    ApplyTax = obj.ApplyTax,
                    MaxQuantity = obj.MaxQuantity,
                    MinQuantity = obj.MinQuantity,
                    Name = obj.Name,
                    PriceBreaks = new List<PriceBreak>()
                    {
                        new PriceBreak() {Price = obj.Price.To<decimal>(), Quantity = obj.QuantityMultiplier}
                    },
                    RestrictedQuantity = obj.RestrictedQuantity,
                    UseCumulativeQuantity = obj.UseCumulativeQuantity
                }, _user.AccessToken);
                var product = await _oc.Products.SaveAsync<HSProduct>(obj.ID, new HSProduct()
                {
                    Active = true,
                    AutoForward = false,
                    DefaultSupplierID = wi.ResourceId,
                    DefaultPriceScheduleID = priceSchedule.ID,
                    ShipFromAddressID = obj.ShipFromAddressID,
                    ID = obj.ID,
                    Name = obj.Name,
                    Description = obj.Description,
                    QuantityMultiplier = obj.QuantityMultiplier,
                    ShipWeight = obj.ShipWeight,
                    ShipLength = obj.ShipLength,
                    ShipHeight = obj.ShipHeight,
                    ShipWidth = obj.ShipWidth,
                    xp = new ProductXp()
                    {
                        IsResale = obj.IsResale,
                        Accessorials = new List<ProductAccessorial>(),
                        Tax = new TaxProperties()
                        {
                            Category = obj.TaxCategory,
                            Code = obj.TaxCode,
                            Description = obj.TaxDescription
                        },
                        UnitOfMeasure = new UnitOfMeasure()
                        {
                            Qty = obj.UnitOfMeasureQuantity,
                            Unit = obj.UnitOfMeasure
                        },
                        ProductType = obj.ProductType,
                        Currency = obj.Currency,
                        SizeTier = obj.SizeTier
                    }
                }, _user.AccessToken);
                Asset image = new Asset()
                {
                    ID = obj.ID,
                    Active = true,
                    FileName = obj.FileName,
                    Type = AssetType.Image,
                    Url = obj.Url,
                    Title = obj.ImageTitle,
                    Tags = obj.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                };
                if (obj.Url != null)
                {
                    image = await _cms.Assets.Save(obj.ID, image, _user.AccessToken);
                    await _cms.Assets.SaveAssetAssignment(new AssetAssignment()
                    {
                        AssetID = image.ID,
                        //ParentResourceID = product.ID,
                        ResourceType = ResourceType.Products,
                        ResourceID = product.ID
                    }, _user.AccessToken);
                }
                
                return JObject.FromObject(Map(product, priceSchedule, image), OrchestrationSerializer.Serializer);
            }
            catch (OrderCloudException exId) when (IdExists(exId))
            {
                // handle 409 errors by refreshing cache
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateExistsError,
                    Message = exId.Message,
                    Level = LogLevel.Error, 
                    OrderCloudErrors = exId.Errors
                });
                return await GetAsync(wi);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error, 
                    OrderCloudErrors = ex.Errors
                });
                throw new Exception(OrchestrationErrorType.CreateGeneralError.ToString(), ex);
            }
            catch (Exception e)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateGeneralError,
                    Message = e.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.CreateGeneralError.ToString(), e);
            }
        }

        public async Task<JObject> UpdateAsync(WorkItem wi)
        {
            var obj = wi.Current.ToObject<TemplateProductFlat>(OrchestrationSerializer.Serializer);
            _user ??= await _token.Authorize(wi.Token, new[] { ApiRole.ProductAdmin, ApiRole.PriceScheduleAdmin });
            try
            {
                obj.ID ??= wi.RecordId;
                var priceSchedule = await _oc.PriceSchedules.SaveAsync<HSPriceSchedule>(obj.ID, new HSPriceSchedule()
                {
                    ID = obj.ID,
                    ApplyShipping = obj.ApplyShipping,
                    ApplyTax = obj.ApplyTax,
                    MaxQuantity = obj.MaxQuantity,
                    MinQuantity = obj.MinQuantity,
                    Name = obj.Name,
                    PriceBreaks = new List<PriceBreak>()
                        {new PriceBreak() {Price = obj.Price.To<decimal>(), Quantity = obj.QuantityMultiplier}},
                    RestrictedQuantity = obj.RestrictedQuantity,
                    UseCumulativeQuantity = obj.UseCumulativeQuantity
                }, _user.AccessToken);
                var product = await _oc.Products.SaveAsync<HSProduct>(obj.ID, new HSProduct()
                {
                    Active = true,
                    AutoForward = false,
                    DefaultSupplierID = wi.ResourceId,
                    DefaultPriceScheduleID = priceSchedule.ID,
                    ShipFromAddressID = obj.ShipFromAddressID,
                    ID = obj.ID,
                    Name = obj.Name,
                    Description = obj.Description,
                    QuantityMultiplier = obj.QuantityMultiplier,
                    ShipWeight = obj.ShipWeight,
                    ShipLength = obj.ShipLength,
                    ShipHeight = obj.ShipHeight,
                    ShipWidth = obj.ShipWidth,
                    xp = new ProductXp()
                    {
                        IsResale = obj.IsResale,
                        Accessorials = new List<ProductAccessorial>(),
                        Tax = new TaxProperties()
                        {
                            Category = obj.TaxCategory,
                            Code = obj.TaxCode,
                            Description = obj.TaxDescription
                        },
                        UnitOfMeasure = new UnitOfMeasure()
                        {
                            Qty = obj.UnitOfMeasureQuantity,
                            Unit = obj.UnitOfMeasure
                        },
                        ProductType = obj.ProductType,
                        Currency = obj.Currency,
                        SizeTier = obj.SizeTier
                    }
                }, _user.AccessToken);
                Asset image = null;
                if (obj.Url != null)
                {
                    image = await _cms.Assets.Save(obj.ID, new Asset()
                    {
                        ID = obj.ID,
                        Active = true,
                        FileName = obj.FileName,
                        Type = AssetType.Image,
                        Url = obj.Url,
                        Title = obj.ImageTitle,
                        Tags = obj.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    }, _user.AccessToken);
                    await _cms.Assets.SaveAssetAssignment(new AssetAssignment()
                    {
                        AssetID = image.ID,
                        ParentResourceID = product.ID,
                        ResourceType = ResourceType.Products,
                        ResourceID = product.ID
                    }, _user.AccessToken);
                }

                return JObject.FromObject(Map(product, priceSchedule, image), OrchestrationSerializer.Serializer);
            }
            catch (OrderCloudException exId) when (IdExists(exId))
            {
                // handle 409 errors by refreshing cache
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateExistsError,
                    Message = exId.Message,
                    Level = LogLevel.Error,
                    OrderCloudErrors = exId.Errors
                });
                return await GetAsync(wi);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error,
                    OrderCloudErrors = ex.Errors
                });
                throw new Exception(OrchestrationErrorType.CreateGeneralError.ToString(), ex);
            }
            catch (Exception e)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateGeneralError,
                    Message = e.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.CreateGeneralError.ToString(), e);
            }
        }

        private static TemplateProductFlat Map(HSProduct product, HSPriceSchedule priceSchedule, Asset asset = null)
        {
            return new TemplateProductFlat
            {
                ID = product.ID,
                Active = product.Active,
                ApplyShipping = priceSchedule.ApplyShipping,
                ApplyTax = priceSchedule.ApplyTax,
                Description = product.Description,
                IsResale = product.xp.IsResale,
                MaxQuantity = priceSchedule.MaxQuantity,
                MinQuantity = priceSchedule.MinQuantity,
                Name = product.Name,
                Price = priceSchedule.PriceBreaks.FirstOrDefault()?.Price,
                ShipFromAddressID = product.ShipFromAddressID,
                ShipHeight = product.ShipHeight,
                ShipLength = product.ShipLength,
                ShipWidth = product.ShipWidth,
                ShipWeight = product.ShipWeight,
                QuantityMultiplier = product.QuantityMultiplier,
                RestrictedQuantity = priceSchedule.RestrictedQuantity,
                TaxCategory = product.xp.Tax.Category,
                TaxCode = product.xp.Tax.Code,
                TaxDescription = product.xp.Tax.Description,
                UseCumulativeQuantity = priceSchedule.UseCumulativeQuantity,
                UnitOfMeasure = product.xp.UnitOfMeasure.Unit,
                UnitOfMeasureQuantity = product.xp.UnitOfMeasure.Qty,
                ImageTitle = asset?.Title,
                Url = asset?.Url,
                Type = asset?.Type ?? AssetType.Image,
                Tags = asset?.Tags?.JoinString(","),
                FileName = asset?.FileName,
                ProductType = product.xp.ProductType,
                SizeTier = product.xp.SizeTier
            };
        }

        public async Task<JObject> PatchAsync(WorkItem wi)
        {
            var obj = wi.Current.ToObject<TemplateProductFlat>(OrchestrationSerializer.Serializer);
            _user ??= await _token.Authorize(wi.Token, new[] { ApiRole.ProductAdmin, ApiRole.PriceScheduleAdmin });
            try
            {
                obj.ID ??= wi.RecordId;
                var priceSchedule = await _oc.PriceSchedules.PatchAsync<HSPriceSchedule>(obj.ID, new PartialPriceSchedule()
                {
                    ID = obj.ID,
                    ApplyShipping = obj.ApplyShipping,
                    ApplyTax = obj.ApplyTax,
                    MaxQuantity = obj.MaxQuantity,
                    MinQuantity = obj.MinQuantity,
                    Name = obj.Name,
                    PriceBreaks = new List<PriceBreak>()
                        {new PriceBreak() {Price = obj.Price.To<decimal>(), Quantity = obj.QuantityMultiplier}},
                    RestrictedQuantity = obj.RestrictedQuantity,
                    UseCumulativeQuantity = obj.UseCumulativeQuantity
                }, _user.AccessToken);
                var product = await _oc.Products.PatchAsync<HSProduct>(obj.ID, new PartialProduct()
                {
                    Active = true,
                    AutoForward = false,
                    DefaultSupplierID = wi.ResourceId,
                    DefaultPriceScheduleID = priceSchedule.ID,
                    ShipFromAddressID = obj.ShipFromAddressID,
                    ID = obj.ID,
                    Name = obj.Name,
                    Description = obj.Description,
                    QuantityMultiplier = obj.QuantityMultiplier,
                    ShipWeight = obj.ShipWeight,
                    ShipLength = obj.ShipLength,
                    ShipHeight = obj.ShipHeight,
                    ShipWidth = obj.ShipWidth,
                    xp = new ProductXp()
                    {
                        IsResale = obj.IsResale,
                        Accessorials = new List<ProductAccessorial>(),
                        Tax = new TaxProperties()
                        {
                            Category = obj.TaxCategory,
                            Code = obj.TaxCode,
                            Description = obj.TaxDescription
                        },
                        UnitOfMeasure = new UnitOfMeasure()
                        {
                            Qty = obj.UnitOfMeasureQuantity,
                            Unit = obj.UnitOfMeasure
                        },
                        ProductType = obj.ProductType,
                        Currency = obj.Currency,
                        SizeTier = obj.SizeTier
                    }
                }, _user.AccessToken);
                Asset image = null;
                if (obj.Url != null)
                {
                    image = await _cms.Assets.Save(obj.ID, new Asset()
                    {
                        ID = obj.ID,
                        Active = true,
                        FileName = obj.FileName,
                        Type = AssetType.Image,
                        Url = obj.Url,
                        Title = obj.ImageTitle,
                        Tags = obj.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    }, _user.AccessToken);
                    await _cms.Assets.SaveAssetAssignment(new AssetAssignment()
                    {
                        AssetID = image.ID,
                        ParentResourceID = product.ID,
                        ResourceType = ResourceType.Products,
                        ResourceID = product.ID
                    }, _user.AccessToken);
                }

                return JObject.FromObject(Map(product, priceSchedule, image), OrchestrationSerializer.Serializer);
            }
            catch (OrderCloudException exId) when (IdExists(exId))
            {
                // handle 409 errors by refreshing cache
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateExistsError,
                    Message = exId.Message,
                    Level = LogLevel.Error,
                    OrderCloudErrors = exId.Errors
                });
                return await GetAsync(wi);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error,
                    OrderCloudErrors = ex.Errors
                });
                throw new Exception(OrchestrationErrorType.CreateGeneralError.ToString(), ex);
            }
            catch (Exception e)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateGeneralError,
                    Message = e.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.CreateGeneralError.ToString(), e);
            }
        }
      
        public Task<JObject> DeleteAsync(WorkItem wi)
        {
            throw new NotImplementedException();
        }

        public async Task<JObject> GetAsync(WorkItem wi)
        {
            _user ??= await _token.Authorize(wi.Token, new[] { ApiRole.ProductAdmin, ApiRole.PriceScheduleAdmin });
            try
            {
                var product = await _oc.Products.GetAsync<HSProduct>(wi.RecordId, wi.Token);
                var priceSchedule = await _oc.PriceSchedules.GetAsync<HSPriceSchedule>(product.DefaultPriceScheduleID, wi.Token);
                var assn = await _cms.Assets.ListAssets(ResourceType.Products, product.ID, new ListArgsPageOnly() { PageSize = 1 } , wi.Token);
                Asset image = null;
                if (assn.Items.Count > 0)
                    image = await _cms.Assets.Get(wi.RecordId, _user.AccessToken);

                return JObject.FromObject(Map(product, priceSchedule, image), OrchestrationSerializer.Serializer);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.GetGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.GetGeneralError.ToString(), ex);
            }
        }
    }
}
