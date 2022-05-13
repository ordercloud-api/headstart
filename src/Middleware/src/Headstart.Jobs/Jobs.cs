using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Services;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Headstart.Jobs
{
    // check out the README.md at the root of this project to get started
    public class Jobs
    {
        private readonly PaymentCaptureJob paymentCapture;
        private readonly SendRecentOrdersJob sendRecentOrdersJob;
        private readonly ReceiveRecentSalesOrdersJob receiveRecentSalesOrdersJob;
        private readonly ReceiveRecentPurchaseOrdersJob receiveRecentPurchaseOrdersJob;
        private readonly ReceiveRecentLineItemsJob receiveRecentLineItemsJob;
        private readonly ReceiveRecentOrdersAndShipmentsJob receiveRecentOrdersAndShipmentsJob;
        private readonly ReceiveProductDetailsJob receiveProductDetailsJob;
        private readonly AppSettings settings;

        public Jobs(
            PaymentCaptureJob paymentCapture,
            SendRecentOrdersJob sendRecentOrdersJob,
            ReceiveRecentSalesOrdersJob receiveRecentSalesOrdersJob,
            ReceiveRecentPurchaseOrdersJob receiveRecentPurchaseOrdersJob,
            ReceiveRecentLineItemsJob receiveRecentLineItemsJob,
            ReceiveRecentOrdersAndShipmentsJob receiveRecentOrdersAndShipmentsJob,
            ReceiveProductDetailsJob receiveProductDetailsJob,
            AppSettings settings)
        {
            this.paymentCapture = paymentCapture;
            this.sendRecentOrdersJob = sendRecentOrdersJob;
            this.receiveRecentSalesOrdersJob = receiveRecentSalesOrdersJob;
            this.receiveRecentPurchaseOrdersJob = receiveRecentPurchaseOrdersJob;
            this.receiveRecentLineItemsJob = receiveRecentLineItemsJob;
            this.receiveRecentOrdersAndShipmentsJob = receiveRecentOrdersAndShipmentsJob;
            this.receiveProductDetailsJob = receiveProductDetailsJob;
            this.settings = settings;
        }

        // Every day at 1:00AM CST (7:00AM UTC)
        [FunctionName("CapturePayments")]
        public async Task CapturePayments([TimerTrigger("0 7 * * *")] TimerInfo myTimer, ILogger logger) => await paymentCapture.Run(logger);

        // Product Detail Cosmos Sync
        // Every day at 1:00AM CST (7:00AM UTC)
        [FunctionName("ReceiveRecentProductDetails")]
        public async Task ReceiveRecentProductDetails([TimerTrigger("0 7 * * *")] TimerInfo myTimer, ILogger logger) => await receiveProductDetailsJob.Run(logger);

        [FunctionName("SendRecentOrders")]

        // Runs every ten minutes
        public async Task ListRecentOrders([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer, ILogger logger) => await sendRecentOrdersJob.Run(logger);

        // Sales Order Detail Cosmos Sync
        // these settings need to be set directly on the Azure App Service (for local testing) instead of the Azure App Configuration until this bug is resolved https://github.com/Azure/azure-functions-host/issues/7210
        [FunctionName("ReceiveRecentSalesOrders")]
        public async Task ReceiveRecentSalesOrders(
        [ServiceBusTrigger(
            topicName: "%ServiceBusSettings:OrderReportsTopicName%",
            subscriptionName: "%ServiceBusSettings:SalesOrderDetailSubscriptionName%",
            Connection = "ServiceBusSettings:ConnectionString")]
        Message message,
        MessageReceiver messageReceiver,
        [ServiceBus(
            queueOrTopicName: "%ServiceBusSettings:OrderReportsTopicName%",
            Connection = "ServiceBusSettings:ConnectionString")]
        MessageSender messageSender,
        ILogger logger) => await receiveRecentSalesOrdersJob.Run(logger, message, messageReceiver, messageSender);

        // Purchase Order Cosmos Sync
        // these settings need to be set directly on the Azure App Service (for local testing) instead of the Azure App Configuration until this bug is resolved https://github.com/Azure/azure-functions-host/issues/7210
        [FunctionName("ReceiveRecentPurchaseOrders")]
        public async Task ReceiveRecentPurchaseOrders(
        [ServiceBusTrigger(
            topicName: "%ServiceBusSettings:OrderReportsTopicName%",
            subscriptionName: "%ServiceBusSettings:PurchaseOrderDetailSubscriptionName%",
            Connection = "ServiceBusSettings:ConnectionString")]
        Message message,
        MessageReceiver messageReceiver,
        [ServiceBus(
            queueOrTopicName: "%ServiceBusSettings:OrderReportsTopicName%",
            Connection = "ServiceBusSettings:ConnectionString")]
        MessageSender messageSender,
        ILogger logger) => await receiveRecentPurchaseOrdersJob.Run(logger, message, messageReceiver, messageSender);

        // Line Item Detail Cosmos Sync
        // these settings need to be set directly on the Azure App Service (for local testing) instead of the Azure App Configuration until this bug is resolved https://github.com/Azure/azure-functions-host/issues/7210
        [FunctionName("ReceiveRecentLineItems")]
        public async Task ReceiveRecentLineItems(
        [ServiceBusTrigger(
            topicName: "%ServiceBusSettings:OrderReportsTopicName%",
            subscriptionName: "%ServiceBusSettings:LineItemDetailSubscriptionName%",
            Connection = "ServiceBusSettings:ConnectionString")]
        Message message,
        MessageReceiver messageReceiver,
        [ServiceBus(
            queueOrTopicName: "%ServiceBusSettings:OrderReportsTopicName%",
            Connection = "ServiceBusSettings:ConnectionString")]
        MessageSender messageSender,
        ILogger logger) => await receiveRecentLineItemsJob.Run(logger, message, messageReceiver, messageSender);

        // Shipment Detail Cosmos Sync
        // these settings need to be set directly on the Azure App Service (for local testing) instead of the Azure App Configuration until this bug is resolved https://github.com/Azure/azure-functions-host/issues/7210
        [FunctionName("ReceiveRecentOrdersAndShipments")]
        public async Task ReceiveRecentOrdersAndShipments(
        [ServiceBusTrigger(
            topicName: "%ServiceBusSettings:OrderReportsTopicName%",
            subscriptionName: "%ServiceBusSettings:ShipmentDetailSubscriptionName%",
            Connection = "ServiceBusSettings:ConnectionString")]
        Message message,
        MessageReceiver messageReceiver,
        [ServiceBus(
            queueOrTopicName: "%ServiceBusSettings:OrderReportsTopicName%",
            Connection = "ServiceBusSettings:ConnectionString")]
        MessageSender messageSender,
        ILogger logger) => await receiveRecentOrdersAndShipmentsJob.Run(logger, message, messageReceiver, messageSender);
    }
}
