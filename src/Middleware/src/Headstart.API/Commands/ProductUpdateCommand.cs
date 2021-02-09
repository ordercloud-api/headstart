using Headstart.Common;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using Headstart.Common.Services;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Headstart.API.Commands
{
    public interface IProductUpdateCommand
    {
        Task CleanUpProductHistoryData();
        Task SendAllProductUpdateEmails();
        ISheet SetHeaders(List<string> headers, ISheet worksheet);
        ISheet SetValues(IEnumerable<object> data, ISheet worksheet);
    }

    public class ProductUpdateCommand : IProductUpdateCommand
    {
        private readonly ResourceHistoryQuery<ProductHistory> _productQuery;
        private readonly ResourceHistoryQuery<PriceScheduleHistory> _priceScheduleQuery;
        private readonly IOrderCloudClient _oc;
        private readonly CloudBlobContainer _container;
        private readonly ISendgridService _sendgridService;
        public ProductUpdateCommand(
            IOrderCloudClient oc,
            ResourceHistoryQuery<ProductHistory> productQuery,
            ResourceHistoryQuery<PriceScheduleHistory> priceScheduleQuery,
            AppSettings settings, 
            ISendgridService sendGrid)
        {
            var blobService = new OrderCloudIntegrationsBlobService(new BlobServiceConfig()
            {
                ConnectionString = settings.BlobSettings.ConnectionString,
                Container = "productupdates",
                AccessType = BlobContainerPublicAccessType.Off
            });
            _productQuery = productQuery;
            _priceScheduleQuery = priceScheduleQuery;
            _oc = oc;
            _container = blobService.Container;
            _sendgridService = sendGrid;
    }

        public async Task SendAllProductUpdateEmails()
        {
            var productUpdateData = await BuildProductUpdateData();
            var excel = new XSSFWorkbook();
            var worksheet = excel.CreateSheet("ProductUpdate");
            var yesterday = DateTime.UtcNow.AddDays(-1).ToString("MMddyyyy");
            var fileName = $"ProductUpdate-{yesterday}.xlsx";
            var fileReference = _container.GetAppendBlobReference(fileName);
            var headers = typeof(ProductUpdateData).GetProperties().Select(p => p.Name).ToList(); //    Use the property names as column headers
            var worksheetWithHeaders = SetHeaders(headers, worksheet);
            var worksheetWithData = SetValues(productUpdateData, worksheetWithHeaders);
            //  get every person we are going to send the email to.
            //  send this as an attachment to each of them.
            using (Stream stream = await fileReference.OpenWriteAsync(true))
            {
                excel.Write(stream);
            }
            var usersToSend = await _oc.AdminUsers.ListAsync(filters: "xp.ProductEmails=true");
            var userEmailList = new List<EmailAddress>();
            foreach(var user in usersToSend.Items)
            {
                var userEmail = new EmailAddress()
                {
                    Email = user.Email,
                    Name = user.FirstName
                };
                userEmailList.Add(userEmail);
            }
            await _sendgridService.SendProductUpdateEmail(userEmailList, fileReference, fileName);
            
        }

        public ISheet SetHeaders(List<string> headers, ISheet worksheet)
        {
            var headerRow = worksheet.CreateRow(0);
            for (var i = 0; i < headers.Count(); i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
            }
            return worksheet;
        }

        public ISheet SetValues(IEnumerable<object> data, ISheet worksheet)
        {
            int i = 1;
            var colNumber = worksheet.GetRow(0).Count(); // number of values in first row (headers)
            foreach (var item in data)
            {
                var dataJSON = JObject.FromObject(item);
                IRow sheetRow = worksheet.CreateRow(i++);
                for (var j = 0; j < colNumber; j++)
                {
                    var rowCell = sheetRow.CreateCell(j);
                    var allValues = dataJSON.Values().ToList();
                    var val = allValues[j];
                    var value = val.ToString();
                    rowCell.SetCellValue(value);
                }
            }
            return worksheet;
        }

        public async Task<List<ProductUpdateData>> BuildProductUpdateData()
        {
            var yesterday = DateTime.Now.AddDays(-1);
            var startOfYesterday = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
            var yesterdaysProductUpdates = await _productQuery.ListByDate(startOfYesterday);
            var yesterdaysPriceScheduleUpdates = await _priceScheduleQuery.ListByDate(startOfYesterday);

            var filterString = String.Join("|", yesterdaysPriceScheduleUpdates.Select(p => p.ResourceID));
            var defaultPriceScheduleProducts = await _oc.Products.ListAsync(filters: $"DefaultPriceScheduleID={filterString}");

            var updatedProducts = yesterdaysProductUpdates.Select(p => p.Resource).Concat(defaultPriceScheduleProducts.Items.Where(p => !yesterdaysProductUpdates.Select(p => p.ResourceID).Contains(p.ID)));
            var dataToSend = new List<ProductUpdateData>();
            foreach (var product in updatedProducts)
            {
                var productUpdateRecord = yesterdaysProductUpdates.Find(p => p.ResourceID == product.ID);
                var updatedProduct = productUpdateRecord?.Resource;
                var priceScheduleUpdateRecord = yesterdaysPriceScheduleUpdates.Find(p => p.ResourceID == product.DefaultPriceScheduleID);
                var updatedPriceSchedule = priceScheduleUpdateRecord?.Resource;

                var oldProduct = (await _productQuery.GetVersionByDate(updatedProduct?.ID, startOfYesterday))?.Resource;
                var oldPriceSchedule = (await _priceScheduleQuery.GetVersionByDate(updatedPriceSchedule?.ID, startOfYesterday))?.Resource;

                var updateData = new ProductUpdateData()
                {
                    Supplier = product.OwnerID,
                    ProductID = product.ID,
                    ProductAction = productUpdateRecord != null ? Enum.GetName(typeof(ActionType), productUpdateRecord?.Action) : null,
                    DefaultPriceScheduleID = updatedPriceSchedule?.ID,
                    DefaultPriceScheduleAction = priceScheduleUpdateRecord !=null ? Enum.GetName(typeof(ActionType), priceScheduleUpdateRecord?.Action) : null,

                };

                if (updatedProduct?.xp?.ProductType != oldProduct?.xp?.ProductType)
                {
                    updateData.NewProductType = updatedProduct?.xp?.ProductType.ToString();
                    updateData.NewProductType = updatedProduct?.xp?.ProductType.ToString();
                }
                if (updatedProduct?.xp?.UnitOfMeasure?.Qty != oldProduct?.xp?.UnitOfMeasure?.Qty)
                {
                    updateData.NewUnitQty = updatedProduct?.xp?.UnitOfMeasure?.Qty;
                    updateData.OldUnitQty = oldProduct?.xp?.UnitOfMeasure?.Qty;
                }
                if (updatedProduct?.xp?.UnitOfMeasure?.Unit != oldProduct?.xp?.UnitOfMeasure?.Unit)
                {
                    updateData.NewUnitMeasure = updatedProduct?.xp?.UnitOfMeasure?.Unit;
                    updateData.OldUnitMeasure = oldProduct?.xp?.UnitOfMeasure?.Unit;
                }
                if (updatedProduct?.Active != oldProduct?.Active)
                {
                    updateData.NewActiveStatus = updatedProduct?.Active;
                    updateData.OldActiveStatus = oldProduct?.Active;
                }
                if (updatedPriceSchedule?.MaxQuantity != oldPriceSchedule?.MaxQuantity)
                {
                    updateData.NewMaxQty = updatedPriceSchedule?.MaxQuantity;
                    updateData.OldMaxQty = oldPriceSchedule?.MaxQuantity;
                }
                if (updatedPriceSchedule?.MinQuantity != oldPriceSchedule?.MinQuantity)
                {
                    updateData.NewMaxQty = updatedPriceSchedule?.MinQuantity;
                    updateData.OldMaxQty = oldPriceSchedule?.MinQuantity;
                }
                if (updatedPriceSchedule?.PriceBreaks != oldPriceSchedule?.PriceBreaks)
                {
                    var updatedBreaks = updatedPriceSchedule.PriceBreaks?.Select(p => JsonConvert.SerializeObject(p))?.ToList();
                    var oldBreaks = oldPriceSchedule?.PriceBreaks?.Select(p => JsonConvert.SerializeObject(p))?.ToList();
                    updateData.NewPriceBreak = updatedBreaks == null ? null : String.Join(",", updatedBreaks);
                    updateData.OldPriceBreak = oldBreaks == null ? null : String.Join(",", oldBreaks);
                }
                dataToSend.Add(updateData);
            }
            return dataToSend;
        }

        public async Task CleanUpProductHistoryData()
        {
            var yesterday = DateTime.Now.AddDays(-1);
            var startOfYesterday = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0);
            var yesterdaysProductUpdates = await _productQuery.ListByDate(startOfYesterday);
            var yesterdaysPriceScheduleUpdates = await _priceScheduleQuery.ListByDate(startOfYesterday);
            foreach(var prod in yesterdaysProductUpdates)
            {
                var updates = (await _productQuery.List(prod.ResourceID)).Where(p => p.id != prod.id).Select(p => p.id);
                await _productQuery.DeleteMany(updates.ToList());
            }
            foreach (var price in yesterdaysPriceScheduleUpdates)
            {
                var updates = (await _priceScheduleQuery.List(price.ResourceID)).Where(p => p.id != price.id).Select(p => p.id);
                await _priceScheduleQuery.DeleteMany(updates.ToList());
            }
        }
    }


}
