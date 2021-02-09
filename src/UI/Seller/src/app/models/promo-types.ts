export interface PromotionXp {
  Type?: HSPromoType
  Value?: number
  AppliesTo?: HSPromoEligibility
  SKUs?: string[]
  Supplier?: string
  Automatic?: boolean
  MinReq?: HSPromoMinRequirement
  MaxShipCost?: number
}

export interface HSPromoMinRequirement {
  Type?: MinRequirementType
  Int?: number
}

export enum HSPromoType {
  Percentage = 'Percentage',
  FixedAmount = 'FixedAmount',
  FreeShipping = 'FreeShipping',
}

export enum HSPromoEligibility {
  EntireOrder = 'EntireOrder',
  SpecificSupplier = 'SpecificSupplier',
  SpecificSKUs = 'SpecificSKUs',
}

export enum MinRequirementType {
  MinPurchase = 'MinPurchase',
  MinItemQty = 'MinItemQty',
}
