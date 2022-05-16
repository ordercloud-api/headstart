using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Headstart.API.Commands.Zoho;
using Headstart.Common;
using Headstart.Common.Constants;
using Headstart.Common.Exceptions;
using Headstart.Common.Services;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Extended;
using Headstart.Models.Headstart;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;
using ITaxCalculator = OrderCloud.Integrations.Library.Interfaces.ITaxCalculator;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands
{
	public interface IPostSubmitCommand
	{
		Task<OrderSubmitResponse> HandleBuyerOrderSubmit(HSOrderWorksheet order);
		Task<OrderSubmitResponse> HandleZohoRetry(string orderID);
		Task<OrderSubmitResponse> HandleShippingValidate(string orderID, DecodedToken decodedToken);
	}

	public class PostSubmitCommand : IPostSubmitCommand
	{
		private readonly IOrderCloudClient oc;
        private readonly IZohoCommand zoho;
        private readonly ITaxCalculator taxCalculator;
        private readonly ISendgridService sendgridService;
        private readonly ILineItemCommand lineItemCommand;
        private readonly AppSettings settings;

		/// <summary>
		/// The IOC based constructor method for the PostSubmitCommand class object with Dependency Injection
		/// </summary>
		/// <param name="sendgridService"></param>
		/// <param name="taxCalculator"></param>
		/// <param name="oc"></param>
		/// <param name="zoho"></param>
		/// <param name="lineItemCommand"></param>
		/// <param name="settings"></param>
		public PostSubmitCommand(
			ISendgridService sendgridService, 
			ITaxCalculator taxCalculator, 
			IOrderCloudClient oc, 
			IZohoCommand zoho, 
			ILineItemCommand lineItemCommand, 
			AppSettings settings)
		{
			try
			{
	            this.settings = settings;
				this.oc = oc;
	            this.taxCalculator = taxCalculator;
	            this.zoho = zoho;
	            this.sendgridService = sendgridService;
	            this.lineItemCommand = lineItemCommand;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable HandleShippingValidate task method
		/// </summary>
		/// <param name="orderID"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The OrderSubmitResponse object from the HandleShippingValidate process</returns>
		public async Task<OrderSubmitResponse> HandleShippingValidate(string orderID, DecodedToken decodedToken)
		{
            var worksheet = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
			return await CreateOrderSubmitResponse(
                new List<ProcessResult>()
                {
                    new ProcessResult()
				{
					Type = ProcessType.Accounting,
                        Activity = new List<ProcessResultAction>()
                        {
                            await ProcessActivityCall(
						ProcessType.Shipping,
						"Validate Shipping",
                                ValidateShipping(worksheet)),
                        },
                    },
                },
				new List<HSOrder> { worksheet.Order });
		}

		/// <summary>
		/// Public re-usable HandleZohoRetry task method
		/// </summary>
		/// <param name="orderID"></param>
		/// <returns>The OrderSubmitResponse object from the HandleZohoRetry process</returns>
		public async Task<OrderSubmitResponse> HandleZohoRetry(string orderID)
		{
            var worksheet = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            var supplierOrders = await Throttler.RunAsync(worksheet.LineItems.GroupBy(g => g.SupplierID).Select(s => s.Key), 100, 10, item => oc.Orders.GetAsync<HSOrder>(
                OrderDirection.Outgoing,
				$"{worksheet.Order.ID}-{item}"));

			return await CreateOrderSubmitResponse(
				new List<ProcessResult>() { await this.PerformZohoTasks(worksheet, supplierOrders) }, 
				new List<HSOrder> { worksheet.Order });
		}

		/// <summary>
		/// Public re-usable HandleBuyerOrderSubmit task method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <returns>The OrderSubmitResponse object from the HandleBuyerOrderSubmit process</returns>
		public async Task<OrderSubmitResponse> HandleBuyerOrderSubmit(HSOrderWorksheet orderWorksheet)
		{
			var results = new List<ProcessResult>();

			// STEP 1
			var (supplierOrders, buyerOrder, activities) = await HandlingForwarding(orderWorksheet);
			results.Add(new ProcessResult()
			{
				Type = ProcessType.Forwarding,
                Activity = activities,
			});
			// step 1 failed. we don't want to attempt the integrations. return error for further action
			if (activities.Any(a => !a.Success))
            {
				return await CreateOrderSubmitResponse(results, new List<HSOrder> { orderWorksheet.Order });
            }
            
			// STEP 2 (integrations)
			var integrations = await HandleIntegrations(supplierOrders, buyerOrder);
			results.AddRange(integrations);

			// STEP 3: return OrderSubmitResponse
			return await CreateOrderSubmitResponse(results, new List<HSOrder> { orderWorksheet.Order });
		}
		
		/// <summary>
		/// Public re-usable CreateOrderRelationshipsAndTransferXP task method
		/// </summary>
		/// <param name="buyerOrder"></param>
		/// <param name="supplierOrders"></param>
		/// <returns>The list of HSOrder objects from the CreateOrderRelationshipsAndTransferXP process</returns>
		public  async Task<List<HSOrder>> CreateOrderRelationshipsAndTransferXP(HSOrderWorksheet buyerOrder, List<Order> supplierOrders)
		{
            var payment = (await oc.Payments.ListAsync(OrderDirection.Incoming, buyerOrder.Order.ID))?.Items?.FirstOrDefault();
			var updatedSupplierOrders = new List<HSOrder>();
			var supplierIDs = new List<string>();
            var lineItems = await oc.LineItems.ListAllAsync(OrderDirection.Incoming, buyerOrder.Order.ID);
			var shipFromAddressIDs = lineItems.DistinctBy(li => li.ShipFromAddressID).Select(li => li.ShipFromAddressID).ToList();

			foreach (var supplierOrder in supplierOrders)
			{
				supplierIDs.Add(supplierOrder.ToCompanyID);
				var shipFromAddressIDsForSupplierOrder = shipFromAddressIDs.Where(addressID => addressID != null && addressID.Contains(supplierOrder.ToCompanyID)).ToList();
                var supplier = await oc.Suppliers.GetAsync<HSSupplier>(supplierOrder.ToCompanyID);
				var suppliersShipEstimates = buyerOrder.ShipEstimateResponse?.ShipEstimates?.Where(se => se.xp.SupplierID == supplier.ID);
                var supplierOrderPatch = new PartialOrder()
                {
					ID = $"{buyerOrder.Order.ID}-{supplierOrder.ToCompanyID}",
                    xp = new OrderXp()
                    {
						ShipFromAddressIDs = shipFromAddressIDsForSupplierOrder,
						SupplierIDs = new List<string>() { supplier.ID },
						StopShipSync = false,
						OrderType = buyerOrder.Order.xp.OrderType,
						QuoteOrderInfo = buyerOrder.Order.xp.QuoteOrderInfo,
						Currency = supplier.xp.Currency,
						ClaimStatus = ClaimStatus.NoClaim,
						ShippingStatus = ShippingStatus.Processing,
						SubmittedOrderStatus = SubmittedOrderStatus.Open,
						SelectedShipMethodsSupplierView = suppliersShipEstimates != null ? MapSelectedShipMethod(suppliersShipEstimates) : null,
						// ShippingAddress needed for Purchase Order Detail Report
						ShippingAddress = new HSAddressBuyer()
						{
							ID = buyerOrder?.Order?.xp?.ShippingAddress?.ID,
							CompanyName = buyerOrder?.Order?.xp?.ShippingAddress?.CompanyName,
							FirstName = buyerOrder?.Order?.xp?.ShippingAddress?.FirstName,
							LastName = buyerOrder?.Order?.xp?.ShippingAddress?.LastName,
							Street1 = buyerOrder?.Order?.xp?.ShippingAddress?.Street1,
							Street2 = buyerOrder?.Order?.xp?.ShippingAddress?.Street2,
							City = buyerOrder?.Order?.xp?.ShippingAddress?.City,
							State = buyerOrder?.Order?.xp?.ShippingAddress?.State,
							Zip = buyerOrder?.Order?.xp?.ShippingAddress?.Zip,
							Country = buyerOrder?.Order?.xp?.ShippingAddress?.Country,
                        },
                    },
				};
                var updatedSupplierOrder = await oc.Orders.PatchAsync<HSOrder>(OrderDirection.Outgoing, supplierOrder.ID, supplierOrderPatch);
				var supplierLineItems = lineItems.Where(li => li.SupplierID == supplier.ID).ToList();
				await SaveShipMethodByLineItem(supplierLineItems, supplierOrderPatch.xp.SelectedShipMethodsSupplierView, buyerOrder.Order.ID);
				await OverrideOutgoingLineQuoteUnitPrice(updatedSupplierOrder.ID, supplierLineItems);
				updatedSupplierOrders.Add(updatedSupplierOrder);
			}

            await lineItemCommand.SetInitialSubmittedLineItemStatuses(buyerOrder.Order.ID);
			var sellerShipEstimates = buyerOrder.ShipEstimateResponse?.ShipEstimates?.Where(se => se.xp.SupplierID == null);

			//Patch Buyer Order after it has been submitted
            var buyerOrderPatch = new PartialOrder()
            {
                xp = new
                {
					ShipFromAddressIDs = shipFromAddressIDs,
					SupplierIDs = supplierIDs,
					ClaimStatus = ClaimStatus.NoClaim,
					ShippingStatus = ShippingStatus.Processing,
					SubmittedOrderStatus = SubmittedOrderStatus.Open,
					HasSellerProducts = buyerOrder.LineItems.Any(li => li.SupplierID == null),
					PaymentMethod = payment.Type == PaymentType.CreditCard ? "Credit Card" : "Purchase Order",
					//  If we have seller ship estimates for a seller owned product save selected method on buyer order.
					SelectedShipMethodsSupplierView = sellerShipEstimates != null ? MapSelectedShipMethod(sellerShipEstimates) : null,
                },
			};

            await oc.Orders.PatchAsync(OrderDirection.Incoming, buyerOrder.Order.ID, buyerOrderPatch);
			return updatedSupplierOrders;
		}
		
		/// <summary>
		/// Private re-usable ValidateShipping task method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		private static async Task ValidateShipping(HSOrderWorksheet orderWorksheet)
		{
			if(orderWorksheet.ShipEstimateResponse.HttpStatusCode != 200)
            {
				throw new Exception(orderWorksheet.ShipEstimateResponse.UnhandledErrorBody);
            }

			if(orderWorksheet.ShipEstimateResponse.ShipEstimates.Any(s => s.SelectedShipMethodID == ShippingConstants.NoRatesID))
            {
				throw new Exception("No shipping rates could be determined - fallback shipping rate of $20 3-day was used");
            }

			await Task.CompletedTask;
		}
		
		/// <summary>
		/// Private re-usable ProcessActivityCall task method
		/// </summary>
		/// <param name="type"></param>
		/// <param name="description"></param>
		/// <param name="func"></param>
		/// <returns>The ProcessResultAction object from the ProcessActivityCall process</returns>
		private static async Task<ProcessResultAction> ProcessActivityCall(ProcessType type, string description, Task func)
		{
			try
			{
				await func;
                return new ProcessResultAction()
                {
					ProcessType = type,
					Description = description,
                    Success = true,
				};
			}
			catch (CatalystBaseException integrationEx)
			{
				return new ProcessResultAction()
				{
					Description = description,
					ProcessType = type,
					Success = false,
                    Exception = new ProcessResultException(integrationEx),
				};
			}
			catch (FlurlHttpException flurlEx)
			{
				return new ProcessResultAction()
				{
					Description = description,
					ProcessType = type,
					Success = false,
                    Exception = new ProcessResultException(flurlEx),
				};
			}
			catch (Exception ex)
			{
                return new ProcessResultAction()
                {
					Description = description,
					ProcessType = type,
					Success = false,
                    Exception = new ProcessResultException(ex),
				};
			}
		}

		/// <summary>
		/// Private re-usable ProcessActivityCall task method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="description"></param>
		/// <param name="func"></param>
		/// <returns>The Tuple of ProcessResultAction objects from the ProcessActivityCall process</returns>
		private static async Task<Tuple<ProcessResultAction, T>> ProcessActivityCall<T>(ProcessType type, string description, Task<T> func) where T : class, new()
		{
			// T must be a class and be newable so the error response can be handled.
			try
			{
				return new Tuple<ProcessResultAction, T>(
					new ProcessResultAction()
					{
						ProcessType = type,
						Description = description,
                        Success = true,
					},
                    await func);
			}
			catch (CatalystBaseException integrationEx)
			{
                return new Tuple<ProcessResultAction, T>(
                    new ProcessResultAction()
				{
					Description = description,
					ProcessType = type,
					Success = false,
                        Exception = new ProcessResultException(integrationEx),
				}, new T());
			}
			catch (FlurlHttpException flurlEx)
			{
                return new Tuple<ProcessResultAction, T>(
                    new ProcessResultAction()
				{
					Description = description,
					ProcessType = type,
					Success = false,
                        Exception = new ProcessResultException(flurlEx),
				}, new T());
			}
			catch (Exception ex)
			{
                return new Tuple<ProcessResultAction, T>(
                    new ProcessResultAction()
				{
					Description = description,
					ProcessType = type,
					Success = false,
                        Exception = new ProcessResultException(ex),
				}, new T());
			}
		}
		
		/// <summary>
		/// Private re-usable PerformZohoTasks task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="supplierOrders"></param>
		/// <returns>The ProcessResult object from the PerformZohoTasks process</returns>
		private async Task<ProcessResult> PerformZohoTasks(HSOrderWorksheet worksheet, IList<HSOrder> supplierOrders)
		{
			var (salesAction, zohoSalesOrder) = await ProcessActivityCall(
				ProcessType.Accounting,
				"Create Zoho Sales Order",
                zoho.CreateSalesOrder(worksheet));

			var (poAction, zohoPurchaseOrder) = await ProcessActivityCall(
				ProcessType.Accounting,
				"Create Zoho Purchase Order",
                zoho.CreateOrUpdatePurchaseOrder(zohoSalesOrder, supplierOrders.ToList()));

			var (shippingAction, zohoShippingOrder) = await ProcessActivityCall(
				ProcessType.Accounting,
				"Create Zoho Shipping Purchase Order",
                zoho.CreateShippingPurchaseOrder(zohoSalesOrder, worksheet));
			return new ProcessResult()
			{
				Type = ProcessType.Accounting,
                Activity = new List<ProcessResultAction>() { salesAction, poAction, shippingAction },
			};
		}
		
		/// <summary>
		/// Private re-usable HandleIntegrations task method
		/// </summary>
		/// <param name="supplierOrders"></param>
		/// <param name="orderWorksheet"></param>
		/// <returns>The list of ProcessResult objects from the HandleIntegrations process</returns>
		private async Task<List<ProcessResult>> HandleIntegrations(List<HSOrder> supplierOrders, HSOrderWorksheet orderWorksheet)
		{
			// STEP 1: SendGrid notifications
			var results = new List<ProcessResult>();

			var notifications = await ProcessActivityCall(
				ProcessType.Notification,
				"Sending Order Submit Emails",
                sendgridService.SendOrderSubmitEmail(orderWorksheet));
			results.Add(new ProcessResult()
			{
				Type = ProcessType.Notification,
                Activity = new List<ProcessResultAction>() { notifications },
			});

			if (!orderWorksheet.IsStandardOrder())
            {
				return results;
            }

			// STEP 2: Tax transaction
			var tax = await ProcessActivityCall(
				ProcessType.Tax,
				"Creating Tax Transaction",
				HandleTaxTransactionCreationAsync(orderWorksheet.Reserialize<OrderWorksheet>()));
			results.Add(new ProcessResult()
			{
				Type = ProcessType.Tax,
                Activity = new List<ProcessResultAction>() { tax },
			});

			// STEP 3: Zoho orders
            if (settings.ZohoSettings.PerformOrderSubmitTasks)
            {
                results.Add(await this.PerformZohoTasks(orderWorksheet, supplierOrders));
            }

			// STEP 4: Validate shipping
			var shipping = await ProcessActivityCall(
				ProcessType.Shipping,
				"Validate Shipping",
				ValidateShipping(orderWorksheet));
			results.Add(new ProcessResult()
			{
				Type = ProcessType.Shipping,
                Activity = new List<ProcessResultAction>() { shipping },
			});

			return results;
		}

		/// <summary>
		/// Private re-usable CreateOrderSubmitResponse task method
		/// </summary>
		/// <param name="processResults"></param>
		/// <param name="ordersRelatingToProcess"></param>
		/// <returns>The OrderSubmitResponse object from the CreateOrderSubmitResponse process</returns>
		private async Task<OrderSubmitResponse> CreateOrderSubmitResponse(List<ProcessResult> processResults, List<HSOrder> ordersRelatingToProcess)
		{
			try
			{
				if (processResults.All(i => i.Activity.All(a => a.Success)))
				{
					await UpdateOrderNeedingAttention(ordersRelatingToProcess, false);
					return new OrderSubmitResponse()
					{
						HttpStatusCode = 200,
						xp = new OrderSubmitResponseXp()
						{
                            ProcessResults = processResults,
                        },
					};
				}
                    
				await UpdateOrderNeedingAttention(ordersRelatingToProcess, true); 
				return new OrderSubmitResponse()
				{
					HttpStatusCode = 500,
					xp = new OrderSubmitResponseXp()
					{
                        ProcessResults = processResults,
                    },
				};
			}
			catch (OrderCloudException ex)
			{
				return new OrderSubmitResponse()
				{
					HttpStatusCode = 500,
                    UnhandledErrorBody = JsonConvert.SerializeObject(ex.Errors),
				};
			}
		}

		/// <summary>
		/// Private re-usable UpdateOrderNeedingAttention task method
		/// </summary>
		/// <param name="orders"></param>
		/// <param name="isError"></param>
		/// <returns></returns>
		private async Task UpdateOrderNeedingAttention(IList<HSOrder> orders, bool isError)
		{
			var partialOrder = new PartialOrder() { xp = new { NeedsAttention = isError } };

			var orderInfos = new List<Tuple<OrderDirection, string>> { };

			var buyerOrder = orders.First();
			orderInfos.Add(new Tuple<OrderDirection, string>(OrderDirection.Incoming, buyerOrder.ID));
			orders.RemoveAt(0);
			orderInfos.AddRange(orders.Select(o => new Tuple<OrderDirection, string>(OrderDirection.Outgoing, o.ID)));

            await Throttler.RunAsync(orderInfos, 100, 3, (orderInfo) => oc.Orders.PatchAsync(orderInfo.Item1, orderInfo.Item2, partialOrder));
		}

		/// <summary>
		/// Private re-usable HandlingForwarding task method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <returns>The Tuple of HSOrder list, HsOrderWorksheet and ProcessResultAction list response objects from the HandlingForwarding process</returns>
		private async Task<Tuple<List<HSOrder>, HSOrderWorksheet, List<ProcessResultAction>>> HandlingForwarding(HSOrderWorksheet orderWorksheet)
		{
			var activities = new List<ProcessResultAction>();

			// forwarding
			var (forwardAction, forwardedOrders) = await ProcessActivityCall(
				ProcessType.Forwarding,
				"OrderCloud API Order.ForwardAsync",
                oc.Orders.ForwardAsync(OrderDirection.Incoming, orderWorksheet.Order.ID));
			activities.Add(forwardAction);

			var supplierOrders = forwardedOrders.OutgoingOrders.ToList();

			// creating relationship between the buyer order and the supplier order
			// no relationship exists currently in the platform
			var (updateAction, hsOrders) = await ProcessActivityCall(
                ProcessType.Forwarding,
                "Create Order Relationships And Transfer XP",
				CreateOrderRelationshipsAndTransferXP(orderWorksheet, supplierOrders));
			activities.Add(updateAction);

			// need to get fresh order worksheet because this process has changed things about the worksheet
			var (getAction, hsOrderWorksheet) = await ProcessActivityCall(
				ProcessType.Forwarding, 
				"Get Updated Order Worksheet",
                oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderWorksheet.Order.ID));
			activities.Add(getAction);

			return await Task.FromResult(new Tuple<List<HSOrder>, HSOrderWorksheet, List<ProcessResultAction>>(hsOrders, hsOrderWorksheet, activities));
		}

		/// <summary>
		/// Private re-usable MapSelectedShipMethod method
		/// </summary>
		/// <param name="shipEstimates"></param>
		/// <returns>The list of ShipMethodSupplierView objects from the MapSelectedShipMethod process</returns>
		private List<ShipMethodSupplierView> MapSelectedShipMethod(IEnumerable<HSShipEstimate> shipEstimates)
		{
			var selectedShipMethods = shipEstimates.Select(shipEstimate =>
			{
				var selected = shipEstimate.ShipMethods.FirstOrDefault(sm => sm.ID == shipEstimate.SelectedShipMethodID);
				return new ShipMethodSupplierView()
				{
					EstimatedTransitDays = selected.EstimatedTransitDays,
					Name = selected.Name,
                    ShipFromAddressID = shipEstimate.xp.ShipFromAddressID,
				};
			}).ToList();
			return selectedShipMethods;
		}

		/// <summary>
		/// Private re-usable HandleTaxTransactionCreationAsync task method
		/// </summary>
		/// <param name="orderWorksheet"></param>
		/// <returns></returns>
		private async Task HandleTaxTransactionCreationAsync(OrderWorksheet orderWorksheet)
		{
            var promotions = await oc.Orders.ListAllPromotionsAsync(OrderDirection.All, orderWorksheet.Order.ID);

            var taxCalculation = await taxCalculator.CommitTransactionAsync(orderWorksheet, promotions);
            await oc.Orders.PatchAsync<HSOrder>(OrderDirection.Incoming, orderWorksheet.Order.ID, new PartialOrder()
			{
				TaxCost = taxCalculation.TotalTax,  // Set this again just to make sure we have the most up to date info
                xp = new { ExternalTaxTransactionID = taxCalculation.ExternalTransactionID },
			});
		}

		/// <summary>
		/// Private re-usable SaveShipMethodByLineItem task method
		/// </summary>
		/// <param name="lineItems"></param>
		/// <param name="shipMethods"></param>
		/// <param name="buyerOrderID"></param>
		/// <returns></returns>
		private async Task SaveShipMethodByLineItem(List<LineItem> lineItems, List<ShipMethodSupplierView> shipMethods, string buyerOrderID)
		{
			if (shipMethods != null)
			{
				foreach (LineItem lineItem in lineItems)
				{
					string shipFromID = lineItem.ShipFromAddressID;
					if (shipFromID != null)
					{
						ShipMethodSupplierView shipMethod = shipMethods.Find(shipMethod => shipMethod.ShipFromAddressID == shipFromID);
						string readableShipMethod = shipMethod.Name.Replace("_", " ");
						PartialLineItem lineItemToPatch = new PartialLineItem { xp = new { ShipMethod = readableShipMethod } };
                        LineItem patchedLineItem = await oc.LineItems.PatchAsync(OrderDirection.Incoming, buyerOrderID, lineItem.ID, lineItemToPatch);
					}
				}
			}
		}

		/// <summary>
		/// Private re-usable OverrideOutgoingLineQuoteUnitPrice task method
		/// </summary>
		/// <param name="supplierOrderID"></param>
		/// <param name="supplierLineItems"></param>
		/// <returns></returns>
		private async Task OverrideOutgoingLineQuoteUnitPrice(string supplierOrderID, List<LineItem> supplierLineItems)
        {
            foreach (LineItem lineItem in supplierLineItems)
            {
                if (lineItem?.Product?.xp?.ProductType == ProductType.Quote.ToString())
                {
                    var patch = new PartialLineItem { UnitPrice = lineItem.UnitPrice };
                    await oc.LineItems.PatchAsync(OrderDirection.Outgoing, supplierOrderID, lineItem.ID, patch);
                }
            }
        }
    }
}
