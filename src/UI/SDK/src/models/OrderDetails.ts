import { Payment, OrderApproval, OrderPromotion, OrderReturn } from 'ordercloud-javascript-sdk';
import { HSLineItem } from './HSLineItem';
import { HSOrder } from './HSOrder';

export interface OrderDetails {
    Order?: HSOrder
    LineItems?: HSLineItem[]
    Promotions?: OrderPromotion[]
    Payments?: Payment[]
    Approvals?: OrderApproval[]
    OrderReturns?: OrderReturn[]
}