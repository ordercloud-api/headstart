import { User } from './User';
import { Address } from './Address';

export interface Order {
    ID?: string
    readonly FromUser?: User
    FromCompanyID?: string
    ToCompanyID?: string
    FromUserID?: string
    BillingAddressID?: string
    readonly BillingAddress?: Address
    ShippingAddressID?: string
    Comments?: string
    readonly LineItemCount?: number
    readonly Status?: 'Unsubmitted' | 'AwaitingApproval' | 'Declined' | 'Open' | 'Completed' | 'Canceled'
    readonly DateCreated?: string
    readonly DateSubmitted?: string
    readonly DateApproved?: string
    readonly DateDeclined?: string
    readonly DateCanceled?: string
    readonly DateCompleted?: string
    readonly Subtotal?: number
    ShippingCost?: number
    TaxCost?: number
    readonly PromotionDiscount?: number
    readonly Total?: number
    readonly IsSubmitted?: boolean
}