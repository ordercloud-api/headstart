import { PaymentTransaction } from './PaymentTransaction';

export interface Payment {
    ID?: string
    Type?: 'PurchaseOrder' | 'CreditCard' | 'SpendingAccount'
    readonly DateCreated?: string
    CreditCardID?: string
    SpendingAccountID?: string
    Description?: string
    Amount?: number
    Accepted?: boolean
    readonly Transactions?: PaymentTransaction[]
}