import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import randomString from '../helpers/random-string'
import testConfig from '../testConfig'
import {
    HeadStartSDK,
    HSLocationUserGroup,
    HSBuyerLocation,
    ReportTemplate,
} from '@ordercloud/headstart-sdk'
import { t } from 'testcafe'


export async function createReportTemplate(adminClientToken: string, reportType: 'BuyerLocation' | 'SalesOrderDetail' | 'PurchaseOrderDetail' | 'LineItemDetail', templateHeaders: string[]): Promise<ReportTemplate> {
    const reportName = `Automated Report_${randomString(5)}`
    const reportDescription = `Automated Report description text field ${randomString(20)}`


    const reportTemplate: ReportTemplate = {
        ReportType: reportType,
        TemplateID: '',
        Name: reportName,
        Description: reportDescription,
        Headers: templateHeaders,
        Filters: {
            BuyerID: [],
            State: [],
            Country: []
        },

        AvailableToSuppliers: true,
    }
    const buyerLocationReport: any = await HeadStartSDK.Reports.PostReportTemplate(reportType, reportTemplate, adminClientToken)
    return buyerLocationReport


}

export async function deleteCreatedTemplate(id: string, adminClientToken: string) {
    try {
        await HeadStartSDK.Reports.DeleteReportTemplate(id, adminClientToken)
    } catch (e) {
        console.log('Error deleting report')
        console.log(id)
    }

}

export const BUYER_LOCATION_HEADERS = [
    "ID",
    "Street1",
    "Street2",
    "City",
    "State",
    "Zip",
    "Country",
    "CompanyName",
    "xp.Email",
    "Phone",
    "xp.BillingNumber",
    "xp.Status",
]

export const SALES_ORDER_DETAIL_HEADERS = [
    "Data.ID",
    "Data.BillingAddress.ID",
    "buyerName",
    "Data.DateSubmitted",
    "Data.DateCompleted",
    "Data.Total",
    "Data.TaxCost",
    "Data.ShippingCost",
    "Data.PromotionDiscount",
    "Data.Subtotal",
    "Data.xp.OrderType",
    "Data.xp.Currency",
    "Data.xp.SubmittedOrderStatus",
    "Data.xp.ShippingStatus",
    "Data.xp.PaymentMethod",
    "Data.BillingAddress.Street1",
    "Data.BillingAddress.Street2",
    "Data.BillingAddress.City",
    "Data.BillingAddress.State",
    "Data.BillingAddress.Zip",
    "Data.BillingAddress.Country",
    "Data.BillingAddress.xp.BillingNumber",
    "Data.BillingAddress.xp.LocationID",
    "Data.BillingAddress.AddressName",
    "Data.FromUser.ID",
    "Data.FromUser.Username",
    "Data.FromUser.FirstName",
    "Data.FromUser.LastName",
    "Data.FromUser.Email",
    "Data.FromUser.Phone",
    "Data.xp.ShippingAddress.FirstName",
    "Data.xp.ShippingAddress.LastName",
    "Data.xp.ShippingAddress.Street1",
    "Data.xp.ShippingAddress.Street2",
    "Data.xp.ShippingAddress.City",
    "Data.xp.ShippingAddress.State",
    "Data.xp.ShippingAddress.Zip",
    "Data.xp.ShippingAddress.Country"
]

export const PURCHASE_ORDER_DETAIL_HEADERS = [
    "Data.ID",
    "Data.ToCompanyID",
    "SupplierName",
    "Data.DateSubmitted",
    "Data.DateCompleted",
    "Data.Total",
    "Data.ShippingAddressID",
    "buyerName",
    "Data.xp.Currency",
    "Data.Status",
    "Data.xp.SubmittedOrderStatus",
    "Data.xp.ShippingStatus",
    "Data.xp.PaymentMethod",
    "Data.FromUser.ID",
    "Data.FromUser.Username",
    "Data.FromUser.FirstName",
    "Data.FromUser.LastName",
    "Data.FromUser.Email",
    "Data.FromUser.Phone",
    "Data.BillingAddress.xp.LocationID",
    "Data.xp.ShippingAddress.FirstName",
    "Data.xp.ShippingAddress.LastName",
    "Data.xp.ShippingAddress.Street1",
    "Data.xp.ShippingAddress.Street2",
    "Data.xp.ShippingAddress.City",
    "Data.xp.ShippingAddress.State",
    "Data.xp.ShippingAddress.Zip",
    "Data.xp.ShippingAddress.Country",
    "Data.xp.OrderType",
    "ShipFromAddressID",
    "ShipMethod"
]

export const LINE_ITEM_HEADERS = [
    "HSOrder.ID",
    "HSOrder.BillingAddressID",
    "HSLineItem.xp.buyerName",
    "HSLineItem.SupplierID",
    "HSLineItem.xp.SupplierName",
    "HSOrder.DateSubmitted",
    "HSOrder.DateCompleted",
    "HSOrder.xp.OrderType",
    "HSOrder.Total",
    "HSOrder.TaxCost",
    "HSOrder.ShippingCost",
    "HSOrder.PromotionDiscount",
    "HSOrder.Subtotal",
    "HSOrder.xp.Currency",
    "HSOrder.Status",
    "HSOrder.xp.SubmittedOrderStatus",
    "HSOrder.xp.PaymentMethod",
    "HSLineItem.ID",
    "HSLineItem.UnitPrice",
    "HSLineItem.LineTotal",
    "HSLineItem.LineSubtotal",
    "HSLineItem.xp.Tax",
    "HSLineItem.ProductID",
    "HSLineItem.Product.Name",
    "HSLineItem.Product.xp.ProductType",
    "HSLineItem.Variant.ID",
    "HSLineItem.Variant.xp.SpecCombo",
    "HSLineItem.Quantity",
    "HSLineItem.Product.xp.Tax.Code",
    "HSLineItem.Product.xp.IsResale",
    "HSLineItem.Product.xp.UnitOfMeasure.Qty",
    "HSLineItem.Product.xp.UnitOfMeasure.Unit",
    "HSLineItem.xp.ShipMethod",
    "HSLineItem.ShippingAddress.FirstName",
    "HSLineItem.ShippingAddress.LastName",
    "HSLineItem.ShippingAddress.Street1",
    "HSLineItem.ShippingAddress.Street2",
    "HSLineItem.ShippingAddress.City",
    "HSLineItem.ShippingAddress.State",
    "HSLineItem.ShippingAddress.Zip",
    "HSLineItem.ShippingAddress.Country",
    "HSOrder.BillingAddress.Street1",
    "HSOrder.BillingAddress.Street2",
    "HSOrder.BillingAddress.City",
    "HSOrder.BillingAddress.State",
    "HSOrder.BillingAddress.Zip",
    "HSOrder.BillingAddress.Country",
    "HSOrder.BillingAddress.xp.BillingNumber",
    "HSOrder.BillingAddress.xp.LocationID",
    "HSOrder.FromUser.ID",
    "HSOrder.FromUser.Username",
    "HSOrder.FromUser.FirstName",
    "HSOrder.FromUser.LastName",
    "HSLineItem.ShippingAddress.xp.Email",
    "HSOrder.FromUser.Phone",
    "HSLineItem.xp.StatusByQuantity.Submitted",
    "HSLineItem.xp.StatusByQuantity.Backordered",
    "HSLineItem.xp.StatusByQuantity.CancelRequested",
    "HSLineItem.xp.StatusByQuantity.CancelDenied",
    "HSLineItem.xp.StatusByQuantity.Complete",
    "HSLineItem.xp.StatusByQuantity.ReturnRequested",
    "HSLineItem.xp.StatusByQuantity.ReturnDenied",
    "HSLineItem.xp.StatusByQuantity.Returned",
    "HSLineItem.xp.StatusByQuantity.Canceled",
    "HSLineItem.xp.StatusByQuantity.Open",
    "HSLineItem.PromotionDiscount"
]

export const RMA_DETAIL_HEADERS = [
    "RMA.SourceOrderID",
    "RMA.SupplierID",
    "RMA.SupplierName",
    "RMA.DateCreated",
    "RMA.DateComplete",
    "RMA.RMANumber",
    "RMA.Type",
    "RMA.Status",
    "RMA.ShippingCredited",
    "RMA.TotalCredited",
    "RMALineItem.ID",
    "RMALineItem.Reason",
    "RMALineItem.QuantityProcessed",
    "RMALineItem.Status",
    "RMALineItem.PercentToRefund",
    "RMALineItem.LineTotalRefund",
    "RMALineItem.IsResolved",
    "RMALineItem.IsRefunded",
    "RMALineItem.Comment"
]

export const SHIPMENT_DETAIL_HEADERS = [
    "OrderID",
    "DateSubmitted",
    "SubmittedOrderStatus",
    "ShippingStatus",
    "LineItemID",
    "SupplierID",
    "SupplierName",
    "SupplierShippingCost",
    "BuyerShippingCost",
    "BuyerShippingTax",
    "BuyerShippingTotal",
    "ShippingCostDifference",
    "ProductID",
    "ProductName",
    "Quantity",
    "QuantityShipped",
    "LineTotal",
    "ShipToCompanyName",
    "ShipToStreet1",
    "ShipToStreet2",
    "ShipToCity",
    "ShipToState",
    "ShipToZip",
    "ShipToCountry",
    "ShipWeight",
    "ShipWidth",
    "ShipHeight",
    "ShipLength",
    "SizeTier",
    "FromUserFirstName",
    "FromUserLastName",
    "LocationID",
    "BillingNumber",
    "buyerID",
    "EstimateCarrier",
    "EstimateCarrierAccountID",
    "EstimateMethod",
    "EstimateTransitDays",
    "ShipmentID",
    "DateShipped",
    "TrackingNumber",
    "Service"
]