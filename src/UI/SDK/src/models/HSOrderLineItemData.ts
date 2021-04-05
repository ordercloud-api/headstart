import { HSOrder } from './HSOrder';
import { HSLineItem } from './HSLineItem';

export interface HSOrderLineItemData {
    Order?: HSOrder
    LineItems?: HSLineItem[]
}