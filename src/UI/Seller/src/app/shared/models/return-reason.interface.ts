export enum ReturnReason {
  IncorrectSizeOrStyle = 'Purchased incorrect size/style',
  IncorrectShipment = 'Incorrect product or size shipped',
  DoesNotMatchDescription = 'Product does not match description',
  ProductDefective = 'Product is defective/damaged',
  PackagingDamaged = 'Shipping box is damaged',
  ReceivedExtraProduct = 'Received extra product that I didn’t buy',
  ArrivedLate = 'Product arrived too late',
  PurchaseMistake = 'Purchased by mistake',
  NotNeeded = 'Product is no longer needed',
  NotApproved = 'Purchase was not approved',
  UnappliedDiscount = 'Discount was not applied',
  ProductMissing = 'Product is missing from shipment',
}
