import { HSOrder } from './HSOrder';
import { LineItem } from './LineItem';
import { OrderPromotion } from './OrderPromotion';
import { Payment } from './Payment';
import { OrderApproval } from './OrderApproval';

export interface OrderDetails {
    Order?: HSOrder
    LineItems?: LineItem[]
    Promotions?: OrderPromotion[]
    Payments?: Payment[]
    Approvals?: OrderApproval[]
}