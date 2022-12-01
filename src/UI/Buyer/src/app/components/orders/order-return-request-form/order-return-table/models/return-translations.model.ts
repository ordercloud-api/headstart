import { ReturnReason, TableHeader } from './return-translations.enum'

export class ReturnTableHeaders {
  productDetails: TableHeader
  price: TableHeader
  quantityOrdered: TableHeader
  quantityReturned: TableHeader
  quantityToReturn: TableHeader
  returnReason: TableHeader
  selectReason: TableHeader
}

export class ReturnTranslations {
  Headers: ReturnTableHeaders
  AvailableReasons: ReturnReason[]
}
