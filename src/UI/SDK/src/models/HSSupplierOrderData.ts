import { HSOrderLineItemData } from './HSOrderLineItemData';
import { HSShipEstimate } from './HSShipEstimate';

export interface HSSupplierOrderData {
    SupplierOrder?: HSOrderLineItemData
    BuyerOrder?: HSOrderLineItemData
    ShipMethod?: HSShipEstimate
}