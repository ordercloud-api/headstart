import { LineItemProduct } from './LineItemProduct';
import { LineItemVariant } from './LineItemVariant';
import { Address } from './Address';
import { LineItemSpec } from './LineItemSpec';

export interface LineItem {
    ID?: string
    ProductID?: string
    Quantity?: number
    readonly DateAdded?: string
    readonly QuantityShipped?: number
    UnitPrice?: number
    readonly PromotionDiscount?: number
    readonly LineTotal?: number
    readonly LineSubtotal?: number
    CostCenter?: string
    DateNeeded?: string
    ShippingAccount?: string
    ShippingAddressID?: string
    ShipFromAddressID?: string
    readonly Product?: LineItemProduct
    readonly Variant?: LineItemVariant
    readonly ShippingAddress?: Address
    readonly ShipFromAddress?: Address
    readonly SupplierID?: string
    Specs?: LineItemSpec[]
}