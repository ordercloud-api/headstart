using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Exceptions;
using Headstart.Common.Helpers;
using Headstart.Common.Models;
using Newtonsoft.Json.Linq;
using Headstart.Common.Queries;
using OrderCloud.SDK;
using Headstart.Models;
using ordercloud.integrations.library;
using Headstart.Common;

namespace Headstart.Orchestration
{
    public class HydratedProductSyncCommand : SyncCommand, IWorkItemCommand
    {
        private readonly IOrderCloudClient _oc;
        public HydratedProductSyncCommand(AppSettings settings, LogQuery log, IOrderCloudClient oc) : base(settings, oc, log)
        {
            _oc = oc;
        }

        public async Task<JObject> CreateAsync(WorkItem wi)
        {
            var obj = wi.Current.ToObject<SuperHSProduct>();
            try
            {
                obj.ID = wi.RecordId;
                var ps = await _oc.PriceSchedules.CreateAsync(obj.PriceSchedule, wi.Token);
                var product = await _oc.Products.CreateAsync<HSProduct>(obj.Product, wi.Token);
               await _oc.Products.SaveAssignmentAsync(new ProductAssignment()
                {
                    ProductID = product.ID,
                    PriceScheduleID = ps.ID
                }, wi.Token);
                var specs = await Throttler.RunAsync(obj.Specs, 250, 20, spec => _oc.Specs.CreateAsync(spec, wi.Token));
                await Throttler.RunAsync(specs, 250, 20, spec => _oc.Specs.SaveProductAssignmentAsync(
                     new SpecProductAssignment()
                     {
                         ProductID = product.ID,
                         SpecID = spec.ID
                     }, wi.Token));
                var list = obj.Specs.SelectMany(spec => spec.Options).ToList();
                var options = Throttler.RunAsync(list, 100, 40, option => _oc.Specs.CreateOptionAsync(option.xp.SpecID, option, wi.Token));
                // attempt to generate variants if option is set
                if (specs.Any(s => s.DefinesVariant))
                {
                    await _oc.Products.GenerateVariantsAsync<HSProduct>(product.ID, true, wi.Token);
                }
                var variants = await _oc.Products.ListVariantsAsync<HSVariant>(product.ID, null, null, null, 1, 100, null, wi.Token);
                //var _images = GetProductImages(id, user);
                //var _attachments = GetProductAttachments(id, user);
                return JObject.FromObject(new SuperHSProduct
                {
                    Product = product,
                    PriceSchedule = ps,
                    Specs = specs,
                    Variants = variants.Items
                });
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
            var obj = wi.Current.ToObject<SuperHSProduct>(OrchestrationSerializer.Serializer);
            try
            {
                if (obj.ID == null) obj.ID = wi.RecordId;
                var response = await _oc.Products.SaveAsync(wi.RecordId, obj.Product, wi.Token);
                return JObject.FromObject(response);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.UpdateGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.UpdateGeneralError.ToString(), ex);
            }
        }

        public async Task<JObject> PatchAsync(WorkItem wi)
        {
            var obj = wi.Diff.ToObject<SuperHSProduct>(OrchestrationSerializer.Serializer);
            try
            {
                //TODO: partial mapping
                var response = await _oc.Products.PatchAsync(wi.RecordId, new PartialProduct(), wi.Token);
                return JObject.FromObject(response);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.PatchGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.PatchGeneralError.ToString(), ex);
            }
        }

        public Task<JObject> DeleteAsync(WorkItem wi)
        {
            throw new NotImplementedException();
        }

        public async Task<JObject> GetAsync(WorkItem wi)
        {
            try
            {
                var product = await _oc.Products.GetAsync<HSProduct>(wi.RecordId, wi.Token);
                var priceSchedule = _oc.PriceSchedules.GetAsync<PriceSchedule>(product.DefaultPriceScheduleID, wi.Token);
                var specs = _oc.Products.ListSpecsAsync(product.ID, null, null, null, 1, 100, null, wi.Token);
                var variants = _oc.Products.ListVariantsAsync<HSVariant>(product.ID, null, null, null, 1, 100, null, wi.Token);
                //var _images = GetProductImages(id, user);
                //var _attachments = GetProductAttachments(id, user);
                return JObject.FromObject(new SuperHSProduct
                {
                    Product = product,
                    PriceSchedule = await priceSchedule,
                    Specs = (await specs).Items,
                    Variants = (await variants).Items,
                    //Images = await images,
                    //Attachments = await attachments
                });
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
