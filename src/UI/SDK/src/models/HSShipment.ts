import { ShipmentXp } from './ShipmentXp';
import { Shipment } from 'ordercloud-javascript-sdk';
import { SupplierAddressXP } from './SupplierAddressXP';
import { BuyerAddressXP } from './BuyerAddressXP';

export type HSShipment = Shipment<ShipmentXp, SupplierAddressXP, BuyerAddressXP>