import { LineItem, LineItemSpec, LineItemVariant, LineItemProduct } from 'ordercloud-javascript-sdk';

export interface HSShipmentItemWithLineItem {
    LineItem?: LineItem
    OrderID?: string
    LineItemID?: string
    QuantityShipped?: number
    readonly UnitPrice?: number
    readonly CostCenter?: string
    readonly DateNeeded?: string
    readonly Product?: LineItemProduct
    readonly Variant?: LineItemVariant
    readonly Specs?: LineItemSpec[]
}