import { OrderXp } from './OrderXp';
import { HSUser } from './HSUser';
import { HSAddressBuyer } from './HSAddressBuyer';

export interface HSOrder {
    xp?: OrderXp
    readonly FromUser?: HSUser
    readonly BillingAddress?: HSAddressBuyer
    ID?: string
    FromCompanyID?: string
    ToCompanyID?: string
    FromUserID?: string
    BillingAddressID?: string
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