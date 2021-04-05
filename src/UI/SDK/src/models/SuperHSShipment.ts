import { HSShipment } from './HSShipment';
import { ShipmentItem } from './ShipmentItem';

export interface SuperHSShipment {
    Shipment?: HSShipment
    ShipmentItems?: ShipmentItem[]
}