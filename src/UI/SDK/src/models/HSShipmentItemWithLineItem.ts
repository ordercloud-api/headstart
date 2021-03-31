import { LineItem } from './LineItem';
import { LineItemProduct } from './LineItemProduct';
import { LineItemVariant } from './LineItemVariant';
import { LineItemSpec } from './LineItemSpec';

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