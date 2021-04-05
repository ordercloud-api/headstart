import { HSOrder } from './HSOrder';
import { HSLineItem } from './HSLineItem';

export interface HSLineItemOrder {
    HSOrder?: HSOrder
    HSLineItem?: HSLineItem
}