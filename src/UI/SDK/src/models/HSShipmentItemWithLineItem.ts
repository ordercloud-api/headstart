import { LineItem, LineItemSpec, LineItemVariant } from 'ordercloud-javascript-sdk';
import { LineItemProduct } from './LineItemProduct';

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