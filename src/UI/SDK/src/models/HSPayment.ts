import { PaymentXP } from './PaymentXP';
import { HSPaymentTransaction } from './HSPaymentTransaction';

export interface HSPayment {
    xp?: PaymentXP
    readonly Transactions?: HSPaymentTransaction[]
    ID?: string
    Type?: 'PurchaseOrder' | 'CreditCard' | 'SpendingAccount'
    readonly DateCreated?: string
    CreditCardID?: string
    SpendingAccountID?: string
    Description?: string
    Amount?: number
    Accepted?: boolean
}