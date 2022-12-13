import { ReturnReason } from '../models/return-translations.enum'
import { TableHeader } from '../models/return-translations.enum'
import { ReturnTableHeaders } from '../models/return-translations.model'

export const returnHeaders: ReturnTableHeaders = {
  productDetails: TableHeader.productDetails,
  price: TableHeader.price,
  quantityOrdered: TableHeader.quantityOrdered,
  quantityReturned: TableHeader.quantityReturned,
  quantityToReturn: TableHeader.quantityToReturn,
  returnReason: TableHeader.returnReason,
  selectReason: TableHeader.selectReturnReason,
  refundAmount: TableHeader.refundAmount,
}

export const returnReasons: ReturnReason[] = [
  ReturnReason.IncorrectSizeOrStyle,
  ReturnReason.IncorrectProductShipped,
  ReturnReason.DoesNotMatchDescription,
  ReturnReason.ProductDefective,
  ReturnReason.PackagingDamaged,
  ReturnReason.ReceivedExtraProduct,
  ReturnReason.ArrivedLate,
  ReturnReason.PurchaseMistake,
  ReturnReason.NotNeeded,
  ReturnReason.NotApproved,
  ReturnReason.UnappliedDiscount,
  ReturnReason.ProductMissing,
]
