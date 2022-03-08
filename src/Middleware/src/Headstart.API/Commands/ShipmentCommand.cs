using System;
using System.IO;
using System.Linq;
using Npoi.Mapper;
using System.Dynamic;
using OrderCloud.SDK;
using Headstart.Common;
using System.Reflection;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Headstart.Models.Extended;
using Headstart.Models.Headstart;
using System.Collections.Generic;
using Misc = Headstart.Common.Models.Misc;
using System.ComponentModel.DataAnnotations;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Headstart.Common.Services.ShippingIntegration.Models;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
    public class DocumentRowError
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class DocumentImportSummary
    {
        public int TotalCount { get; set; }
        public int ValidCount { get; set; }
        public int InvalidCount { get; set; }
    }

    public class DocumentImportResult
    {
        public DocumentImportSummary Meta { get; set; } = new DocumentImportSummary();
        public List<Misc.Shipment> Valid = new List<Misc.Shipment>();
        public List<DocumentRowError> Invalid = new List<DocumentRowError>();
    }
    
    public class BatchProcessSummary
    {
        public int TotalCount { get; set; }
        public int SuccessfulCount { get; set; }
        public int ProcessFailureListCount { get; set; }
        public int DocumentFailureListCount { get; set; }
    }
    
    public class BatchProcessFailure
    {
        public Misc.Shipment Shipment { get; set; } = new Misc.Shipment();
        public string Error { get; set; } = string.Empty;
    }
    
    public class BatchProcessResult
    {
        public BatchProcessSummary Meta { get; set; } = new BatchProcessSummary();
        public List<Shipment> SuccessfulList = new List<Shipment>();
        public List<BatchProcessFailure> ProcessFailureList = new List<BatchProcessFailure>();
        public List<DocumentRowError> InvalidRowFailureList = new List<DocumentRowError>();
    }

    public interface IShipmentCommand
    {
        Task<SuperHSShipment> CreateShipment(SuperHSShipment superShipment, DecodedToken decodedToken);
        Task<BatchProcessResult> UploadShipments(IFormFile file, DecodedToken decodedToken);
    }

    public class ShipmentCommand : IShipmentCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly ILineItemCommand _lineItemCommand;
        private Dictionary<string, Shipment> _shipmentByTrackingNumber = new Dictionary<string, Shipment>();
        private readonly WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

        /// <summary>
        /// The IOC based constructor method for the ShipmentCommand class object with Dependency Injection
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="oc"></param>
        /// <param name="lineItemCommand"></param>
        public ShipmentCommand(AppSettings settings, IOrderCloudClient oc, ILineItemCommand lineItemCommand)
        {
            try
            {
                _oc = oc;
                _lineItemCommand = lineItemCommand;
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Public re-usable CreateShipment task method
        /// </summary>
        /// <param name="superShipment"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The SuperHSShipment response object from the CreateShipment process</returns>
        public async Task<SuperHSShipment> CreateShipment(SuperHSShipment superShipment, DecodedToken decodedToken)
        {
            var resp = new SuperHSShipment();
            try
            {
                string buyerID = string.Empty;
                ShipmentItem firstShipmentItem = superShipment.ShipmentItems.First();
                string supplierOrderID = firstShipmentItem.OrderID;
                string buyerOrderID = supplierOrderID.Split("-").First();

                // in the platform, in order to make sure the order has the proper Order.Status, you must 
                // create a shipment without a DateShipped and then patch the DateShipped after
                DateTimeOffset? dateShipped = superShipment.Shipment.DateShipped;
                superShipment.Shipment.DateShipped = null;
                await PatchLineItemStatuses(supplierOrderID, superShipment, OrderDirection.Incoming, decodedToken);
                if (decodedToken.CommerceRole != CommerceRole.Seller)
                {
                    buyerID = await GetBuyerIDForSupplierOrder(firstShipmentItem.OrderID);
                }
                else
                {
                    buyerID = superShipment.Shipment.xp.BuyerID;
                }
                superShipment.Shipment.BuyerID = buyerID;

                HSShipment ocShipment = await _oc.Shipments.CreateAsync<HSShipment>(superShipment.Shipment, accessToken: decodedToken.AccessToken);
                //  platform issue. Cant save new xp values onto shipment line item. Update order line item to have this value.
                var shipmentItemsWithComment = superShipment.ShipmentItems.Where(s => s.xp?.Comment != null);
                await Throttler.RunAsync(shipmentItemsWithComment, 100, 5, (shipmentItem) =>
                {
                    dynamic comments = new ExpandoObject();
                    var commentsByShipment = comments as IDictionary<string, object>;
                    commentsByShipment[ocShipment.ID] = shipmentItem.xp?.Comment;
                    return _oc.LineItems.PatchAsync(OrderDirection.Incoming, buyerOrderID, shipmentItem.LineItemID, new PartialLineItem()
                    {
                        xp = new
                        {
                            Comments = commentsByShipment
                        }
                    });
                });

                IList<ShipmentItem> shipmentItemResponses = await Throttler.RunAsync(superShipment.ShipmentItems, 100, 5, (shipmentItem) => _oc.Shipments.SaveItemAsync(ocShipment.ID, shipmentItem, accessToken: decodedToken.AccessToken));
                HSShipment ocShipmentWithDateShipped = await _oc.Shipments.PatchAsync<HSShipment>(ocShipment.ID, new PartialShipment() { DateShipped = dateShipped }, accessToken: decodedToken.AccessToken);
                resp = new SuperHSShipment()
                {
                    Shipment = ocShipmentWithDateShipped,
                    ShipmentItems = shipmentItemResponses.ToList()
                };
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable PatchLineItemStatuses task method
        /// </summary>
        /// <param name="supplierOrderID"></param>
        /// <param name="superShipment"></param>
        /// <param name="direction"></param>
        /// <param name="decodedToken"></param>
        /// <returns></returns>
        private async Task PatchLineItemStatuses(string supplierOrderID, SuperHSShipment superShipment, OrderDirection direction, DecodedToken decodedToken)
        {
            try
            {
                List<LineItemStatusChange> lineItemStatusChanges = superShipment.ShipmentItems.Select(shipmentItem =>
                {
                    return new LineItemStatusChange()
                    {
                        Quantity = shipmentItem.QuantityShipped,
                        ID = shipmentItem.LineItemID
                    };
                }).ToList();

                LineItemStatusChanges lineItemStatusChange = new LineItemStatusChanges()
                {
                    Changes = lineItemStatusChanges,
                    Status = LineItemStatus.Complete,
                    SuperShipment = superShipment
                };
                await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(IsSupplierUser(decodedToken) ? OrderDirection.Outgoing : direction, supplierOrderID, lineItemStatusChange);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Private re-usable IsSupplierUser method
        /// </summary>
        /// <param name="decodedToken"></param>
        /// <returns>The IsSupplierUser boolean status value</returns>
        private bool IsSupplierUser(DecodedToken decodedToken)
        {
            var resp = false;
            try
            {
                resp = (decodedToken.CommerceRole == CommerceRole.Supplier);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable GetBuyerIDForSupplierOrder task method
        /// </summary>
        /// <param name="supplierOrderID"></param>
        /// <returns>The BuyerIDForSupplierOrder string value from the GetBuyerIDForSupplierOrder process</returns>
        private async Task<string> GetBuyerIDForSupplierOrder(string supplierOrderID)
        {
            var buyerID = string.Empty;
            try
            {
                string buyerOrderID = supplierOrderID.Split($@"-").First();
                Order relatedBuyerOrder = await _oc.Orders.GetAsync(OrderDirection.Incoming, buyerOrderID);
                if (!string.IsNullOrEmpty(relatedBuyerOrder.FromCompanyID))
                {
                    buyerID = relatedBuyerOrder.FromCompanyID;
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }            
            return buyerID;
        }

        /// <summary>
        /// Public re-usable UploadShipments task method
        /// </summary>
        /// <param name="file"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The BatchProcessResult response object from the UploadShipments process</returns>
        public async Task<BatchProcessResult> UploadShipments(IFormFile file, DecodedToken decodedToken)
        {
            var documentImportResult = new BatchProcessResult();
            try
            {
                documentImportResult = await GetShipmentListFromFile(file, decodedToken);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return documentImportResult;
        }

        /// <summary>
        /// Private re-usable GetShipmentListFromFile task method
        /// </summary>
        /// <param name="file"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The BatchProcessResult response object from the GetShipmentListFromFile process</returns>
        /// <exception cref="Exception"></exception>
        private async Task<BatchProcessResult> GetShipmentListFromFile(IFormFile file, DecodedToken decodedToken)
        {
            var resp = new BatchProcessResult();
            try
            {
                if (file == null)
                {
                    return resp;
                }
                using Stream stream = file.OpenReadStream();
                List<RowInfo<Misc.Shipment>> shipments = new Mapper(stream).Take<Misc.Shipment>(0, 1000).ToList();

                if (shipments == null)
                {
                    var ex = new Exception("There were No shipments found in the sheet.");
                    LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                    throw ex;
                }
                shipments.RemoveRange(0, 2);
                DocumentImportResult result = Validate(shipments);
                var processResults = await ProcessShipments(result, decodedToken);
                resp = await Task.FromResult(processResults);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable ProcessShipments task method
        /// </summary>
        /// <param name="importResult"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The BatchProcessResult response object from the ProcessShipments process</returns>
        private async Task<BatchProcessResult> ProcessShipments(DocumentImportResult importResult, DecodedToken decodedToken)
        {
            var processResult = new BatchProcessResult();
            try
            {
                if (importResult == null)
                {
                    return null;
                }
                if (importResult.Valid?.Count < 0)
                {
                    return null;
                }

                foreach (Misc.Shipment shipment in importResult.Valid)
                {
                    try
                    {
                        bool isSuccessful = await ProcessShipment(shipment, processResult, decodedToken);
                    }
                    catch (Exception ex)
                    {
                        BatchProcessFailure failureDto = new BatchProcessFailure();
                        failureDto.Error = ex.Message;
                        failureDto.Shipment = shipment;
                        processResult.ProcessFailureList.Add(failureDto);
                        LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                    }

                }
                processResult.InvalidRowFailureList.AddRange(importResult.Invalid);
                processResult.Meta = new BatchProcessSummary()
                {
                    ProcessFailureListCount = processResult.ProcessFailureList.Count(),
                    DocumentFailureListCount = processResult.InvalidRowFailureList.Count(),
                    SuccessfulCount = processResult.SuccessfulList.Count(),
                    TotalCount = importResult.Meta.TotalCount
                };
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return processResult;
        }

        /// <summary>
        /// Private re-usable PatchOrderStatus method
        /// </summary>
        /// <param name="ocOrder"></param>
        /// <param name="shippingStatus"></param>
        /// <param name="orderStatus"></param>
        private async void PatchOrderStatus(Order ocOrder, ShippingStatus shippingStatus, OrderStatus orderStatus)
        {
            try
            {
                var partialOrder = new PartialOrder { xp = new { ShippingStatus = shippingStatus, SubmittedOrderStatus = orderStatus } };
                await _oc.Orders.PatchAsync(OrderDirection.Outgoing, ocOrder.ID, partialOrder);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Private re-usable PatchOrderStatus method
        /// </summary>
        /// <param name="lineItemList"></param>
        /// <returns>The ValidateLineItemCounts boolean status value</returns>
        private bool ValidateLineItemCounts(ListPage<LineItem> lineItemList)
        {
            var resp = true;
            try
            {
                if (lineItemList == null || lineItemList?.Items?.Count < 1)
                {
                    return false;
                }

                foreach (LineItem lineItem in lineItemList.Items)
                {
                    if (lineItem.Quantity > lineItem.QuantityShipped)
                    {
                        return false;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable CreateBatchProcessFailureItem method
        /// </summary>
        /// <param name="shipment"></param>
        /// <param name="ex"></param>
        /// <returns>The BatchProcessFailure response object from the CreateBatchProcessFailureItem process</returns>
        private BatchProcessFailure CreateBatchProcessFailureItem(Misc.Shipment shipment, OrderCloudException ex)
        {
            BatchProcessFailure failure = new BatchProcessFailure();
            try
            {
                if (ex != null && ex.Errors != null && ex.Errors.Length > 0)
                {
                    failure.Error = $@"{ex.Message}: {((dynamic)ex?.Errors[0]?.Data).ToList()?[0]}";
                }
                else
                {
                    failure.Error = $@"Something went wrong";
                }
            }
            catch (Exception ex1)
            {
                if (ex != null && !string.IsNullOrEmpty(ex.Message))
                {
                    failure.Error = $@"{ex.Message}.";
                    LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex1.Message}. {ex.Message}", ex1.StackTrace, this, true);
                }
                else
                {
                    failure.Error = $@"Something went wrong.";
                    LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex1.Message, ex1.StackTrace, this, true);
                }
            }
            failure.Shipment = shipment;
            return failure;
        }

        /// <summary>
        /// Private re-usable PatchLineItemStatus task method
        /// </summary>
        /// <param name="supplierOrderID"></param>
        /// <param name="lineItem"></param>
        /// <param name="decodedToken"></param>
        /// <returns></returns>
        private async Task PatchLineItemStatus(string supplierOrderID, ShipmentItem lineItem, DecodedToken decodedToken)
        {
            try
            {
                var lineItemStatusChangeList = new List<LineItemStatusChange>();
                lineItemStatusChangeList.Add(new LineItemStatusChange()
                {
                    Quantity = lineItem.QuantityShipped,
                    ID = lineItem.LineItemID
                });

                LineItemStatusChanges lineItemStatusChange = new LineItemStatusChanges()
                {
                    Changes = lineItemStatusChangeList,
                    Status = LineItemStatus.Complete
                };
                await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection.Outgoing, supplierOrderID, lineItemStatusChange, null);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Private re-usable ProcessShipment task method
        /// </summary>
        /// <param name="shipment"></param>
        /// <param name="result"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The boolean status value from the ProcessShipment process</returns>
        private async Task<bool> ProcessShipment(Misc.Shipment shipment, BatchProcessResult result, DecodedToken decodedToken)
        {
            var resp = false;
            PartialShipment newShipment = null;
            try
            {
                if (shipment == null) 
                { 
                    var ex = new Exception("This Shipment cannot be null.");
                    LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                    throw ex;
                }

                Order ocOrder = await GetOutgoingOrder(shipment);
                LineItem lineItem = await GetOutgoingLineItem(shipment);
                //Don't continue if attempting to ship more items than what's in the order.
                ValidateShipmentAmount(shipment, lineItem, ocOrder);
                ShipmentItem newShipmentItem = new ShipmentItem()
                {
                    OrderID = shipment.OrderID,
                    LineItemID = lineItem.ID,
                    QuantityShipped = Convert.ToInt32(shipment.QuantityShipped),
                    UnitPrice = Convert.ToDecimal(shipment.Cost)
                    
                };

                var ocShipment = await GetShipmentByTrackingNumber(shipment, decodedToken?.AccessToken);
                //If a user included a ShipmentID in the spreadsheet, find that shipment and patch it with the information on that row
                if (ocShipment != null)
                {
                    newShipment = PatchShipment(ocShipment, shipment);
                }
                if (newShipment != null)
                {
                    Shipment processedShipment = await _oc.Shipments.PatchAsync(newShipment.ID, newShipment, decodedToken?.AccessToken);
                    //Before updating shipment item, must post the shipment line item comment to the order line item due to OC bug.
                    await PatchPartialLineItemComment(shipment, newShipment.ID);
                    await PatchLineItemStatus(shipment.OrderID, newShipmentItem, decodedToken);
                    //POST a shipment item, passing it a Shipment ID parameter, and a request body of Order ID, Line Item ID, and Quantity Shipped
                    await _oc.Shipments.SaveItemAsync(newShipment.ID, newShipmentItem, accessToken: decodedToken?.AccessToken);
                    //Re-patch the shipment adding the date shipped now due to oc bug
                    var repatchedShipment = PatchShipment(ocShipment, shipment);
                    await _oc.Shipments.PatchAsync(newShipment.ID, repatchedShipment);
                    result.SuccessfulList.Add(processedShipment);
                }

                if (lineItem?.ID == null)
                {
                    //Before updating shipment item, must post the shipment line item comment to the order line item due to OC bug.
                    await PatchPartialLineItemComment(shipment, newShipment.ID);
                    //Create new lineItem
                    await _oc.Shipments.SaveItemAsync(shipment.ShipmentID, newShipmentItem);
                }
                resp = true;
            }
            catch (OrderCloudException ex)
            {
                resp = false;
                result.ProcessFailureList.Add(CreateBatchProcessFailureItem(shipment, ex));
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            catch (Exception ex)
            {
                resp = false;
                result.ProcessFailureList.Add(new BatchProcessFailure()
                {
                    Error = ex.Message,
                    Shipment = shipment
                });
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable ValidateShipmentAmount method
        /// </summary>
        /// <param name="shipment"></param>
        /// <param name="lineItem"></param>
        /// <param name="ocOrder"></param>
        /// <exception cref="Exception"></exception>
        private void ValidateShipmentAmount(Misc.Shipment shipment, LineItem lineItem, Order ocOrder)
        {
            try
            {
                if (shipment == null || lineItem == null)
                {
                    return;
                }

                int newAmountShipped = (Convert.ToInt32(shipment.QuantityShipped) + lineItem.QuantityShipped);
                if (newAmountShipped > lineItem.Quantity)
                {
                    var ex = new Exception($"Unable to ship {shipment.QuantityShipped} item(s). {lineItem.QuantityShipped} out of {lineItem.Quantity} items are already shipped. LineItemID: {lineItem.ID}.");
                    LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }


        /// <summary>
        /// Private re-usable GetOutgoingLineItem task method
        /// </summary>
        /// <param name="shipment"></param>
        /// <returns>The LineItem response object from the GetOutgoingLineItem process</returns>
        /// <exception cref="Exception"></exception>
        private async Task<LineItem> GetOutgoingLineItem(Misc.Shipment shipment)
        {
            var resp = new LineItem();
            try
            {
                if (shipment == null || shipment.LineItemID == null)
                {
                    var ex = new Exception("No LineItemID provided for this shipment.");
                    LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                    throw ex;
                }
                resp = await _oc.LineItems.GetAsync(OrderDirection.Outgoing, shipment.OrderID, shipment.LineItemID);
            }
            catch (Exception ex)
            {
                var ex1 = new Exception($"Unable to find LineItem for the LineItemID: {shipment.LineItemID}.", ex.InnerException);
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {ex1.Message}", ex.StackTrace, this, true);
                throw ex1;
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable GetOutgoingOrder task method
        /// </summary>
        /// <param name="shipment"></param>
        /// <returns>The Order response object from the GetOutgoingOrder process</returns>
        /// <exception cref="Exception"></exception>
        private async Task<Order> GetOutgoingOrder(Misc.Shipment shipment)
        {
            var resp = new Order();
            try
            {
                if (shipment == null || shipment.OrderID == null) 
                {
                    var ex = new Exception("No OrderID provided for this shipment.");
                    LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                    throw ex;
                }
                resp = await _oc.Orders.GetAsync(OrderDirection.Outgoing, shipment.OrderID);
            }
            catch (Exception ex)
            {
                var ex1 = new Exception($"Unable to find Order for the OrderID: {shipment.OrderID}.", ex.InnerException);
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {ex1.Message}", ex.StackTrace, this, true);
                throw ex1;
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable PatchPartialLineItemComment task method
        /// </summary>
        /// <param name="shipment"></param>
        /// <param name="newShipmentId"></param>
        /// <returns>The LineItem response object from the PatchPartialLineItemComment process</returns>
        private async Task<LineItem> PatchPartialLineItemComment(Misc.Shipment shipment, string newShipmentId)
        {
            var resp = new LineItem();
            try
            {
                PartialLineItem partialLineItem = new PartialLineItem();
                Dictionary<string, object> commentDictonary = new Dictionary<string, object>();
                //Add new comment
                if (string.IsNullOrEmpty(shipment?.ShipmentLineItemComment) || string.IsNullOrEmpty(shipment?.ShipmentLineItemComment))
                {
                    return null;
                }

                commentDictonary.Add(newShipmentId, shipment?.ShipmentLineItemComment);
                partialLineItem = new PartialLineItem()
                {
                    xp = new
                    {
                        Comments = commentDictonary
                    }
                };
                resp = await _oc.LineItems.PatchAsync(OrderDirection.Outgoing, shipment.OrderID, shipment.LineItemID, partialLineItem);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable GetShipmentByTrackingNumber task method
        /// </summary>
        /// <param name="shipment"></param>
        /// <param name="accessToken"></param>
        /// <returns>The Shipment response object from the GetShipmentByTrackingNumber process</returns>
        /// <exception cref="Exception"></exception>
        private async Task<Shipment> GetShipmentByTrackingNumber(Misc.Shipment shipment, string accessToken)
        {
            var shipmentResponse = new Shipment();
            try
            {
                //if dictionary already contains shipment, return that shipment
                if (shipment != null && _shipmentByTrackingNumber.ContainsKey(shipment.TrackingNumber))
                {
                    return _shipmentByTrackingNumber[shipment.TrackingNumber];
                }

                if (shipment?.ShipmentID != null)
                {
                    //get shipment if shipmentId is provided
                    try
                    {
                        shipmentResponse = await _oc.Shipments.GetAsync(shipment.ShipmentID, accessToken);
                    }
                    catch (Exception ex)
                    {
                        var ex1 = new Exception($@"The ShipmentID: {shipment.ShipmentID} could not be found. If you are not patching an existing shipment, this field must be blank so that the system can generate a Shipment ID for you. Error Detail: {ex.Message}.");
                        LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex1.Message, ex.StackTrace, this, true);
                        throw ex1;
                    }
                    if (shipmentResponse != null)
                    {
                        //add shipment to dictionary if it's found
                        _shipmentByTrackingNumber.Add(shipmentResponse.TrackingNumber, shipmentResponse);
                    }
                }
                else if (shipment?.ShipmentID == null)
                {
                    PartialShipment newShipment = PatchShipment(null, shipment);
                    //Create shipment for tracking number provided if shipmentId wasn't included
                    shipmentResponse = await _oc.Shipments.CreateAsync(newShipment, accessToken);
                    if (shipmentResponse != null)
                    {
                        _shipmentByTrackingNumber.Add(shipmentResponse.TrackingNumber, shipmentResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return shipmentResponse;
        }

        /// <summary>
        /// Private re-usable PatchShipment method
        /// </summary>
        /// <param name="ocShipment"></param>
        /// <param name="shipment"></param>
        /// <returns>The PartialShipment response object from the PatchShipment process</returns>
        private PartialShipment PatchShipment(Shipment ocShipment, Misc.Shipment shipment)
        {
            var newShipment = new PartialShipment();
            try
            {
                bool isCreatingNew = false;
                if (ocShipment == null)
                {
                    isCreatingNew = true;
                }
                else
                {
                    newShipment.ID = ocShipment?.ID.Trim();
                    newShipment.xp = ocShipment?.xp;
                    newShipment.FromAddress = ocShipment?.FromAddress;
                    newShipment.ToAddress = ocShipment?.ToAddress;
                }

                if (newShipment.xp == null)
                {
                    newShipment.xp = new ShipmentXp();
                }
                newShipment.BuyerID = shipment.BuyerID;
                newShipment.Shipper = shipment.Shipper;
                newShipment.DateShipped = isCreatingNew ? null : shipment.DateShipped; //Must patch to null on new creation due to OC bug
                newShipment.DateDelivered = shipment.DateDelivered;
                newShipment.TrackingNumber = shipment.TrackingNumber;
                newShipment.Cost = Convert.ToDecimal(shipment.Cost);
                newShipment.Account = shipment.Account;
                newShipment.FromAddressID = shipment.FromAddressID;
                newShipment.ToAddressID = shipment.ToAddressID;
                newShipment.xp.Service = Convert.ToString(shipment.Service);
                newShipment.xp.Comment = Convert.ToString(shipment.ShipmentComment);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return newShipment;
        }

        /// <summary>
        /// Public re-usable Validate method to Validate list of Shipments records
        /// </summary>
        /// <param name="rows"></param>
        /// <returns>The DocumentImportResult response object from the Validate list of Shipments records process</returns>
        public DocumentImportResult Validate(List<RowInfo<Misc.Shipment>> rows)
        {
            var result = new DocumentImportResult();
            try
            {
                foreach (RowInfo<Misc.Shipment> row in rows)
                {
                    if (row.ErrorColumnIndex > -1)
                    {
                        result.Invalid.Add(new DocumentRowError()
                        {
                            ErrorMessage = row.ErrorMessage,
                            Row = row.RowNumber++,
                            Column = row.ErrorColumnIndex
                        });
                    }
                    else
                    {
                        List<ValidationResult> results = new List<ValidationResult>();
                        if (ShouldIgnoreRow(row.Value)) 
                        { 
                            continue; 
                        }

                        if (Validator.TryValidateObject(row.Value, new ValidationContext(row.Value), results, true) == false)
                        {
                            result.Invalid.Add(new DocumentRowError()
                            {
                                ErrorMessage = $@"{results.FirstOrDefault()?.ErrorMessage}",
                                Row = row.RowNumber++
                            });
                        }
                        else
                        {
                            result.Valid.Add(row.Value);
                        }
                    }
                }
                result.Meta = new DocumentImportSummary()
                {
                    InvalidCount = result.Invalid.Count,
                    ValidCount = result.Valid.Count,
                    TotalCount = rows.Count
                };
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return result;
        }

        /// <summary>
        /// Private re-usable ShouldIgnoreRow method
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The ShouldIgnoreRow boolean status value</returns>
        private bool ShouldIgnoreRow(Misc.Shipment value)
        {
            var resp = false;
            try
            {
                string exampleSignifier = "//EXAMPLE//";
                //Ignore if the row is empty, or if it's the example row.
                if (value == null)
                {
                    return false;
                }
                if (value.OrderID?.ToUpper() == exampleSignifier)
                {
                    return true;
                }

                PropertyInfo[] props = typeof(Misc.Shipment).GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    if (prop.GetValue(value) != null)
                    {
                        return false;
                    }
                }
                resp = true;
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }
    }
}