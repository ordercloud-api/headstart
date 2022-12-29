import { OrderPromotion } from 'ordercloud-javascript-sdk'

export type CheckoutSectionName =
  | 'login'
  | 'shippingAddress'
  | 'shippingSelection'
  | 'payment'
  | 'confirm'

export interface CheckoutSection {
  id: CheckoutSectionName
  valid: boolean
}

export interface IGroupedOrderPromo {
  [id: string]: IOrderPromotionDisplay
}

export interface IOrderPromotionDisplay {
  OrderPromotions: OrderPromotion[]
  DiscountTotal: number
}

export type AddressType = 'billing' | 'shipping'

export enum AcceptedPaymentTypes {
  CreditCard = 'CreditCard',
  PurchaseOrder = 'PurchaseOrder',
}
