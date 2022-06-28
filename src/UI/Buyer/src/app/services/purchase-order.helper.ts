import { HSOrder, HSLineItem } from '@ordercloud/headstart-sdk'
import { OrderPromotion, ShipEstimate } from 'ordercloud-javascript-sdk'
import { OrderSummaryMeta } from '../models/order.types'

const getOverrideText = (checkoutPanel: string): string => {
  /* if there is override text for shipping and tax
   * we show that and calculate the order total differently */
  switch (checkoutPanel) {
    case 'cart':
      return 'ORDERS.SUMMARY.CALCULATED_AT_CHECKOUT'
    case 'shippingAddress':
    case 'shippingSelection':
      return 'ORDERS.SUMMARY.PENDING_SELECTIONS'
    default:
      return ''
  }
}

const getCreditCardTotal = (
  subTotal: number,
  shippingCost: number,
  taxCost: number,
  shouldHideShippingAndText: boolean,
  discountTotal: number
): number => {
  if (shouldHideShippingAndText) {
    return subTotal - discountTotal
  } else {
    return subTotal + shippingCost + taxCost - discountTotal
  }
}

/* eslint-disable */
export const getOrderSummaryMeta = (
  order: HSOrder,
  orderPromos: OrderPromotion[],
  lineItems: HSLineItem[],
  checkoutPanel: string,
): OrderSummaryMeta => {

  const ShippingAndTaxOverrideText = getOverrideText(checkoutPanel);
  const shouldHideShippingAndText = !!ShippingAndTaxOverrideText;

  const CreditCardDisplaySubtotal = lineItems.reduce((accumulator, li) => (li.Quantity * li.UnitPrice) + accumulator, 0);
  const DiscountTotal = orderPromos?.reduce((accumulator, promo) => (promo.Amount) + accumulator, 0);

  const ShippingCost = order.ShippingCost;

  const CreditCardTotal = getCreditCardTotal(CreditCardDisplaySubtotal, ShippingCost, order.TaxCost, shouldHideShippingAndText, DiscountTotal);
  const OrderTotal = CreditCardTotal;

  return {
    LineItemCount: lineItems.length,
    ShippingAndTaxOverrideText,
    ShouldHideShippingAndText: shouldHideShippingAndText,
    CreditCardDisplaySubtotal,
    ShippingCost: ShippingCost,
    TaxCost: order.TaxCost,
    CreditCardTotal,
    DiscountTotal,
    OrderTotal
  };
}

/* eslint-enable */