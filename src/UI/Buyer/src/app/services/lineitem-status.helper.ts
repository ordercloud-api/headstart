import { HSLineItem } from '@ordercloud/headstart-sdk'

export function NumberCanReturn(lineItem: HSLineItem): number {
  return lineItem?.xp?.StatusByQuantity
    ? lineItem?.xp?.StatusByQuantity['Complete']
    : 0
}

export function NumberCanCancel(lineItem: HSLineItem): number {
  return lineItem?.xp?.StatusByQuantity
    ? lineItem.xp.StatusByQuantity['Submitted'] +
        lineItem.xp.StatusByQuantity['Backordered']
    : 0
}

export function CanReturn(lineItem: HSLineItem): boolean {
  return !!NumberCanReturn(lineItem)
}

export function CanCancel(lineItem: HSLineItem): boolean {
  return !!NumberCanCancel(lineItem)
}

export function NumberCanCancelOrReturn(
  lineItem: HSLineItem,
  action: string
): number {
  if (action === 'return') {
    return NumberCanReturn(lineItem)
  } else {
    return NumberCanCancel(lineItem)
  }
}

export function CanReturnOrCancel(
  lineItem: HSLineItem,
  action: string
): boolean {
  return !!NumberCanCancelOrReturn(lineItem, action)
}

export function CanReturnOrder(lineItems: HSLineItem[]): boolean {
  return lineItems.some((li) => CanReturn(li))
}

export function CanCancelOrder(lineItems: HSLineItem[]): boolean {
  return lineItems.some((li) => CanCancel(li))
}
