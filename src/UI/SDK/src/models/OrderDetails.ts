import { LineItem, Payment, OrderApproval, OrderPromotion } from 'ordercloud-javascript-sdk';
import { HSOrder } from './HSOrder';

export interface OrderDetails {
    Order?: HSOrder
    LineItems?: LineItem[]
    Promotions?: OrderPromotion[]
    Payments?: Payment[]
    Approvals?: OrderApproval[]
}