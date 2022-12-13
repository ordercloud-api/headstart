import { HSLineItem, HSOrderReturn } from '@ordercloud/headstart-sdk'
import { flatten, sum } from 'lodash'

export function NumberCanReturn(
  lineItem: HSLineItem,
  orderReturns: HSOrderReturn[] = []
): number {
  if (!lineItem.Product.Returnable) {
    return 0
  }
  const maxReturnable = lineItem.QuantityShipped
  const quantityAlreadyRequested = NumberHasReturned(lineItem, orderReturns)
  return maxReturnable - quantityAlreadyRequested
}

export function NumberHasReturned(
  lineItem: HSLineItem,
  orderReturns: HSOrderReturn[]
): number {
  const alreadyReturnedItems = flatten(
    orderReturns
      .filter(
        (orderReturn) =>
          orderReturn.Status === 'Open' ||
          orderReturn.Status === 'AwaitingApproval' ||
          orderReturn.Status === 'Completed'
      )
      .map((orderReturn) => orderReturn.ItemsToReturn)
  ).filter((item) => item.LineItemID === lineItem.ID)

  return sum(alreadyReturnedItems.map((li) => li.Quantity))
}

export function CanReturn(
  lineItem: HSLineItem,
  orderReturns: HSOrderReturn[] = []
): boolean {
  return !!NumberCanReturn(lineItem, orderReturns)
}

export function CanReturnOrder(
  lineItems: HSLineItem[],
  orderReturns: HSOrderReturn[] = []
): boolean {
  return lineItems.some((li) => CanReturn(li, orderReturns))
}
