import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'orderReturnReason',
})
export class OrderReturnReasonPipe implements PipeTransform {
  transform(value: string): string {
    if (value === 'IncorrectSizeOrStyle') {
      return 'ORDERS.RETURN_REASONS.INCORRECT_SIZE_OR_STYLE'
    } else if (value === 'IncorrectProductShipment') {
      return 'ORDERS.RETURN_REASONS.INCORRECT_PRODUCT_SHIPMENT'
    } else if (value === 'DoesNotMatchDescription') {
      return 'ORDERS.RETURN_REASONS.DOES_NOT_MATCH_DESCRIPTION'
    } else if (value === 'Defective') {
      return 'ORDERS.RETURN_REASONS.DEFECTIVE'
    } else if (value === 'PackagingDamaged') {
      return 'ORDERS.RETURN_REASONS.PACKAGING_DAMAGED'
    } else if (value === 'ReceivedExtraProduct') {
      return 'ORDERS.RETURN_REASONS.RECEIVED_EXTRA_PRODUCT'
    } else if (value === 'ArrivedLate') {
      return 'ORDERS.RETURN_REASONS.ARRIVED_LATE'
    } else if (value === 'PurchaseMistake') {
      return 'ORDERS.RETURN_REASONS.PURCHASE_MISTAKE'
    } else if (value === 'NotNeeded') {
      return 'ORDERS.RETURN_REASONS.NOT_NEEDED'
    } else if (value === 'NotApproved') {
      return 'ORDERS.RETURN_REASONS.NOT_APPROVED'
    } else if (value === 'UnappliedDiscount') {
      return 'ORDERS.RETURN_REASONS.UNAPPLIED_DISCOUNTS'
    } else if (value === 'ProductMissing') {
      return 'ORDERS.RETURN_REASONS.PRODUCT_MISSING'
    } else if (value === 'FoundDifferentProduct') {
      return 'ORDERS.RETURN_REASONS.FOUND_DIFFERENT_PRODUCT'
    } else if (value === 'FulfillmentTooLong') {
      return 'ORDERS.RETURN_REASONS.FULFILLMENT_TOO_LONG'
    } else {
      return value
    }
  }
}
