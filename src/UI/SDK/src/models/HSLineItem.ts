import { LineItemXp } from './LineItemXp';
import { LineItem } from 'ordercloud-javascript-sdk';
import { ProductXp } from './ProductXp';
import { BuyerAddressXP } from '.';
import { SupplierAddressXP } from './SupplierAddressXP';

export type HSLineItem = LineItem<LineItemXp, ProductXp, any, BuyerAddressXP, SupplierAddressXP>