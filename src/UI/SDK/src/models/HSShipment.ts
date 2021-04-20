import { ShipmentXp } from './ShipmentXp';
import { HSAddressSupplier } from './HSAddressSupplier';
import { HSAddressBuyer } from './HSAddressBuyer';

export interface HSShipment {
    xp?: ShipmentXp
    readonly FromAddress?: HSAddressSupplier
    readonly ToAddress?: HSAddressBuyer
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
}