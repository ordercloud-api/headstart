using System;
using System.IO;
using System.Linq;
using Npoi.Mapper;
using System.Dynamic;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;
using Misc = Headstart.Common.Models.Misc;
using System.ComponentModel.DataAnnotations;
using Headstart.Common.Models.Headstart.Extended;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

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
		Task<SuperHsShipment> CreateShipment(SuperHsShipment superShipment, DecodedToken decodedToken);
		Task<BatchProcessResult> UploadShipments(IFormFile file, DecodedToken decodedToken);
	}

	public class ShipmentCommand : IShipmentCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly ILineItemCommand _lineItemCommand;
		private Dictionary<string, Shipment> _shipmentByTrackingNumber = new Dictionary<string, Shipment>();
		private readonly AppSettings _settings;
		private const string exampleSignifier = "//EXAMPLE//";

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
				_settings = settings;
				_oc = oc;
				_lineItemCommand = lineItemCommand;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable CreateShipment task method
		/// </summary>
		/// <param name="superShipment"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The SuperHsShipment response object from the CreateShipment process</returns>
		public async Task<SuperHsShipment> CreateShipment(SuperHsShipment superShipment, DecodedToken decodedToken)
		{
			var resp = new SuperHsShipment();
			try
			{
				var buyerId = string.Empty;
				var firstShipmentItem = superShipment.ShipmentItems.First();
				var supplierOrderId = firstShipmentItem.OrderID;
				var buyerOrderId = supplierOrderId.Split("-").First();

				// in the platform, in order to make sure the order has the proper Order.Status, you must 
				// create a shipment without a DateShipped and then patch the DateShipped after
				DateTimeOffset? dateShipped = superShipment.Shipment.DateShipped;
				superShipment.Shipment.DateShipped = null;
				await PatchLineItemStatuses(supplierOrderId, superShipment, OrderDirection.Incoming, decodedToken);
				if (decodedToken.CommerceRole != CommerceRole.Seller)
				{
					buyerId = await GetBuyerIDForSupplierOrder(firstShipmentItem.OrderID);
				}
				else
				{
					buyerId = superShipment.Shipment.xp.BuyerId;
				}
				superShipment.Shipment.BuyerID = buyerId;

				var ocShipment = await _oc.Shipments.CreateAsync<HsShipment>(superShipment.Shipment, accessToken: decodedToken.AccessToken);
				//  platform issue. Cant save new xp values onto shipment line item. Update order line item to have this value.
				var shipmentItemsWithComment = superShipment.ShipmentItems.Where(s => s.xp?.Comment != null);
				await Throttler.RunAsync(shipmentItemsWithComment, 100, 5, (shipmentItem) =>
				{
					dynamic comments = new ExpandoObject();
					var commentsByShipment = comments as IDictionary<string, object>;
					commentsByShipment[ocShipment.ID] = shipmentItem.xp?.Comment;
					return _oc.LineItems.PatchAsync(OrderDirection.Incoming, buyerOrderId, shipmentItem.LineItemID, new PartialLineItem()
					{
						xp = new
						{
							Comments = commentsByShipment
						}
					});
				});

				var shipmentItemResponses = await Throttler.RunAsync(superShipment.ShipmentItems, 100, 5, (shipmentItem) => _oc.Shipments.SaveItemAsync(ocShipment.ID, shipmentItem, accessToken: decodedToken.AccessToken));
				var ocShipmentWithDateShipped = await _oc.Shipments.PatchAsync<HsShipment>(ocShipment.ID, new PartialShipment() { DateShipped = dateShipped }, accessToken: decodedToken.AccessToken);
				resp = new SuperHsShipment()
				{
					Shipment = ocShipmentWithDateShipped,
					ShipmentItems = shipmentItemResponses.ToList()
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
		private async Task PatchLineItemStatuses(string supplierOrderID, SuperHsShipment superShipment, OrderDirection direction, DecodedToken decodedToken)
		{
			try
			{
				var lineItemStatusChanges = superShipment.ShipmentItems.Select(shipmentItem =>
				{
					return new LineItemStatusChange()
					{
						Quantity = shipmentItem.QuantityShipped,
						Id = shipmentItem.LineItemID
					};
				}).ToList();

				var lineItemStatusChange = new LineItemStatusChanges()
				{
					Changes = lineItemStatusChanges,
					Status = LineItemStatus.Complete,
					SuperShipment = superShipment
				};
				await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(IsSupplierUser(decodedToken) ? OrderDirection.Outgoing : direction, supplierOrderID, lineItemStatusChange);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable GetBuyerIDForSupplierOrder task method
		/// </summary>
		/// <param name="supplierOrderId"></param>
		/// <returns>The BuyerIDForSupplierOrder string value from the GetBuyerIDForSupplierOrder process</returns>
		private async Task<string> GetBuyerIDForSupplierOrder(string supplierOrderId)
		{
			var buyerId = string.Empty;
			try
			{
				var buyerOrderId = supplierOrderId.Split(@"-").First();
				var relatedBuyerOrder = await _oc.Orders.GetAsync(OrderDirection.Incoming, buyerOrderId);
				if (!string.IsNullOrEmpty(relatedBuyerOrder.FromCompanyID))
				{
					buyerId = relatedBuyerOrder.FromCompanyID;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}            
			return buyerId;
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
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
				var shipments = new Mapper(stream).Take<Misc.Shipment>(0, 1000).ToList();
				if (shipments == null)
				{
					var ex = new Exception("There were No shipments found in the sheet.");
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}

				shipments.RemoveRange(0, 2);
				var result = Validate(shipments);
				var processResults = await ProcessShipments(result, decodedToken);
				resp = await Task.FromResult(processResults);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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

				foreach (var shipment in importResult.Valid)
				{
					try
					{
						var isSuccessful = await ProcessShipment(shipment, processResult, decodedToken);
					}
					catch (Exception ex)
					{
						var failureDto = new BatchProcessFailure
						{
							Error = ex.Message,
							Shipment = shipment
						};
						processResult.ProcessFailureList.Add(failureDto);
						LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
				var partialOrder = new PartialOrder 
				{ 
					xp = new
					{
						ShippingStatus = shippingStatus, 
						SubmittedOrderStatus = orderStatus
					}
				};
				await _oc.Orders.PatchAsync(OrderDirection.Outgoing, ocOrder.ID, partialOrder);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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

				if (lineItemList?.Items != null)
				{
					foreach (var lineItem in lineItemList.Items)
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
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
			var failure = new BatchProcessFailure();
			try
			{
				if (ex?.Errors != null && ex.Errors.Length > 0)
				{
					failure.Error = $@"{ex.Message}: {((dynamic)ex?.Errors[0]?.Data).ToList()?[0]}";
				}
				else
				{
					failure.Error = @"Something went wrong.";
				}
			}
			catch (Exception ex1)
			{
				if (ex != null && !string.IsNullOrEmpty(ex.Message))
				{
					failure.Error = $@"{ex.Message}.";
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex1.Message}. {ex.Message}", ex1.StackTrace, this, true);
				}
				else
				{
					failure.Error = @"Something went wrong.";
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex1.Message, ex1.StackTrace, this, true);
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
					Id = lineItem.LineItemID
				});

				var lineItemStatusChange = new LineItemStatusChanges()
				{
					Changes = lineItemStatusChangeList,
					Status = LineItemStatus.Complete
				};
				await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection.Outgoing, supplierOrderID, lineItemStatusChange, null);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
					var ex = new Exception(@"This Shipment cannot be null.");
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}

				var ocOrder = await GetOutgoingOrder(shipment);
				var lineItem = await GetOutgoingLineItem(shipment);
				//Don't continue if attempting to ship more items than what's in the order.
				ValidateShipmentAmount(shipment, lineItem, ocOrder);
				var newShipmentItem = new ShipmentItem()
				{
					OrderID = shipment.OrderId,
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
					var processedShipment = await _oc.Shipments.PatchAsync(newShipment.ID, newShipment, decodedToken?.AccessToken);
					//Before updating shipment item, must post the shipment line item comment to the order line item due to OC bug.
					await PatchPartialLineItemComment(shipment, newShipment.ID);
					await PatchLineItemStatus(shipment.OrderId, newShipmentItem, decodedToken);
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
					await _oc.Shipments.SaveItemAsync(shipment.ShipmentId, newShipmentItem);
				}
				resp = true;
			}
			catch (OrderCloudException ex)
			{
				resp = false;
				result.ProcessFailureList.Add(CreateBatchProcessFailureItem(shipment, ex));
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			catch (Exception ex)
			{
				resp = false;
				result.ProcessFailureList.Add(new BatchProcessFailure()
				{
					Error = ex.Message,
					Shipment = shipment
				});
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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

				var newAmountShipped = (Convert.ToInt32(shipment.QuantityShipped) + lineItem.QuantityShipped);
				if (newAmountShipped > lineItem.Quantity)
				{
					var ex = new Exception($"Unable to ship {shipment.QuantityShipped} item(s). {lineItem.QuantityShipped} out of {lineItem.Quantity} items are already shipped. LineItemID: {lineItem.ID}.");
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
				if (shipment == null || shipment.LineItemId == null)
				{
					var ex = new Exception(@"No LineItemID provided for this shipment.");
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}
				resp = await _oc.LineItems.GetAsync(OrderDirection.Outgoing, shipment.OrderId, shipment.LineItemId);
			}
			catch (Exception ex)
			{
				var ex1 = new Exception($@"Unable to find LineItem for the LineItemID: {shipment.LineItemId}.", ex.InnerException);
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {ex1.Message}", ex.StackTrace, this, true);
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
				if (shipment == null || shipment.OrderId == null) 
				{
					var ex = new Exception(@"No OrderID provided for this shipment.");
					LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}
				resp = await _oc.Orders.GetAsync(OrderDirection.Outgoing, shipment.OrderId);
			}
			catch (Exception ex)
			{
				var ex1 = new Exception($@"Unable to find Order for the OrderID: {shipment.OrderId}.", ex.InnerException);
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {ex1.Message}", ex.StackTrace, this, true);
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
				var commentDictionary = new Dictionary<string, object>();
				//Add new comment
				if (string.IsNullOrEmpty(shipment?.ShipmentLineItemComment) || string.IsNullOrEmpty(shipment?.ShipmentLineItemComment))
				{
					return null;
				}

				commentDictionary.Add(newShipmentId, shipment?.ShipmentLineItemComment);
				var partialLineItem = new PartialLineItem()
				{
					xp = new
					{
						Comments = commentDictionary
					}
				};
				resp = await _oc.LineItems.PatchAsync(OrderDirection.Outgoing, shipment.OrderId, shipment.LineItemId, partialLineItem);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
				//If dictionary already contains shipment, return that shipment
				if (shipment != null && _shipmentByTrackingNumber.ContainsKey(shipment.TrackingNumber))
				{
					return _shipmentByTrackingNumber[shipment.TrackingNumber];
				}

				if (shipment?.ShipmentId != null)
				{
					//Get shipment if shipmentId is provided
					try
					{
						shipmentResponse = await _oc.Shipments.GetAsync(shipment.ShipmentId, accessToken);
					}
					catch (Exception ex)
					{
						var ex1 = new Exception($@"The ShipmentID: {shipment.ShipmentId} could not be found. If you are not patching an existing shipment, this field must be blank so that the system can generate a Shipment ID for you. Error Detail: {ex.Message}.");
						LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex1.Message, ex.StackTrace, this, true);
						throw ex1;
					}
					if (shipmentResponse != null)
					{
						//Add shipment to dictionary if it's found
						_shipmentByTrackingNumber.Add(shipmentResponse.TrackingNumber, shipmentResponse);
					}
				}
				else if (shipment?.ShipmentId == null)
				{
					var newShipment = PatchShipment(null, shipment);
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
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
				var isCreatingNew = false;
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
				newShipment.BuyerID = shipment.BuyerId;
				newShipment.Shipper = shipment.Shipper;
				newShipment.DateShipped = isCreatingNew ? null : shipment.DateShipped; //Must patch to null on new creation due to OC bug
				newShipment.DateDelivered = shipment.DateDelivered;
				newShipment.TrackingNumber = shipment.TrackingNumber;
				newShipment.Cost = Convert.ToDecimal(shipment.Cost);
				newShipment.Account = shipment.Account;
				newShipment.FromAddressID = shipment.FromAddressId;
				newShipment.ToAddressID = shipment.ToAddressId;
				newShipment.xp.Service = Convert.ToString(shipment.Service);
				newShipment.xp.Comment = Convert.ToString(shipment.ShipmentComment);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
				foreach (var row in rows)
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
						var results = new List<ValidationResult>();
						if (ShouldIgnoreRow(row.Value)) 
						{ 
							continue; 
						}

						if (Validator.TryValidateObject(row.Value, new ValidationContext(row.Value), results, true) == false)
						{
							result.Invalid.Add(new DocumentRowError()
							{
								ErrorMessage = $@"{results.FirstOrDefault()?.ErrorMessage}.",
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
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
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
				//Ignore if the row is empty, or if it's the example row.
				if (value == null)
				{
					return false;
				}
				if (value.OrderId?.ToUpper() == exampleSignifier)
				{
					return true;
				}

				var props = typeof(Misc.Shipment).GetProperties();
				foreach (var prop in props)
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
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}
	}
}