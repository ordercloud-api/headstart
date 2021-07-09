import { HSLineItem } from '@ordercloud/headstart-sdk'
import { LineItem, Sortable } from 'ordercloud-javascript-sdk'

export interface OrderSummaryMeta {
  LineItemCount: number

  ShouldHideShippingAndText: boolean
  ShippingAndTaxOverrideText: string

  // with no purchase order these are displayed as the whole order
  CreditCardDisplaySubtotal: number
  ShippingCost: number
  TaxCost: number
  CreditCardTotal: number
  DiscountTotal: number

  OrderTotal: number
}

// TODO - remove once SDK has enum types
export enum OrderType {
  Standard = 'Standard',
  Quote = 'Quote',
}

export enum OrderAddressType {
  Billing = 'Billing',
  Shipping = 'Shipping',
}

export enum HeadstartOrderStatus {
  AllSubmitted = '!Unsubmitted',
  AwaitingApproval = 'AwaitingApproval',
  ChangesRequested = 'ChangesRequested',
  Open = 'Open',
  Completed = 'Completed',
  Canceled = 'Canceled',
}

export interface OrderFilters {
  page?: number
  sortBy?: Sortable<'Me.ListOrders'>
  search?: string
  showOnlyFavorites?: boolean
  status?: HeadstartOrderStatus
  /**
   * mm-dd-yyyy
   */
  fromDate?: string
  /**
   * mm-dd-yyyy
   */
  toDate?: string
  location?: string
}

export enum RMAStatus {
  Requested = 'Requested',
  Denied = 'Denied',
  Processing = 'Processing',
  Approved = 'Approved',
  Complete = 'Complete',
  Canceled = 'Canceled',
  PartialQtyApproved = 'PartialQtyApproved',
  PartialQtyComplete = 'PartialQtyComplete',
}

export enum OrderViewContext {
  MyOrders = 'MyOrders',
  Approve = 'Approve',
  Location = 'Location',
}

export interface OrderReorderResponse {
  ValidLi: Array<LineItem>
  InvalidLi: Array<LineItem>
}

export enum ClaimStatus {
  NoClaim = 'NoClaim',
  Pending = 'Pending',
  Complete = 'Complete',
}

export type OrderListColumn =
  | 'ID'
  | 'Status'
  | 'DateSubmitted'
  | 'SubmittedBy'
  | 'Total'
  | 'Favorite'
