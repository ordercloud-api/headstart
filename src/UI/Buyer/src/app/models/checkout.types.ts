import { OrderPromotion } from "ordercloud-javascript-sdk";

export interface CheckoutSection {
  id: string
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