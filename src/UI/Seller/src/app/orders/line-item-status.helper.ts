import { LineItemStatus } from '@app-seller/models/order.types'
import { HSLineItem } from '@ordercloud/headstart-sdk'

const validPreviousStates = {
  Submitted: [],
  Complete: [
    LineItemStatus.Submitted,
    LineItemStatus.Backordered,
    LineItemStatus.CancelRequested,
    LineItemStatus.CancelDenied,
  ],
  ReturnRequested: [LineItemStatus.Complete],
  Returned: [LineItemStatus.ReturnRequested, LineItemStatus.Complete],
  Backordered: [LineItemStatus.Submitted],
  CancelRequested: [LineItemStatus.Submitted, LineItemStatus.Backordered],
  Canceled: [
    LineItemStatus.CancelRequested,
    LineItemStatus.Backordered,
    LineItemStatus.Submitted,
  ],
}

export function NumberCanChangeTo(
  lineItemStatus: LineItemStatus,
  lineItem: HSLineItem
): number {
  return Object.entries(lineItem.xp.StatusByQuantity as any)
    .filter(([status, quantity]) =>
      validPreviousStates[lineItemStatus].includes(status)
    )
    .reduce((acc, [status, quantity]) => (quantity as number) + acc, 0)
}
export function CanChangeTo(
  lineItemStatus: LineItemStatus,
  lineItem: HSLineItem
): boolean {
  return !!NumberCanChangeTo(lineItemStatus, lineItem)
}

export function CanChangeLineItemsOnOrderTo(
  lineItemStatus: LineItemStatus,
  lineItems: HSLineItem[]
): boolean {
  return (
    !lineItems.some(
      (li) =>
        lineItemStatus === LineItemStatus.Backordered &&
        li.Product?.xp?.ProductType === 'Quote'
    ) && lineItems.some((li) => CanChangeTo(lineItemStatus, li))
  )
}

export function SellerOrderCanShip(
  lineItemStatus: LineItemStatus,
  lineItems: HSLineItem[],
  sellerUser: boolean
): boolean {
  return lineItems.some((li) => li.SupplierID === null && sellerUser)
}
