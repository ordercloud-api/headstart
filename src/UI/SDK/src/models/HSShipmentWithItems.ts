import { Address } from 'ordercloud-javascript-sdk';
import { HSShipmentItemWithLineItem } from './HSShipmentItemWithLineItem';
export interface HSShipmentWithItems {
    ShipmentItems?: HSShipmentItemWithLineItem[]
    ID?: string
    BuyerID?: string
    Shipper?: string
    DateShipped?: string
    DateDelivered?: string
    TrackingNumber?: string
    Cost?: number
    Account?: string
    FromAddressID?: string
    ToAddressID?: string
    readonly FromAddress?: Address
    readonly ToAddress?: Address
}