export interface PromotionXp {
  Type?: HSPromoType
  Value?: number
  AppliesTo?: HSPromoEligibility
  SKUs?: string[]
  Supplier?: string
  Automatic?: boolean
  MinReq?: HSPromoMinRequirement
  MaxShipCost?: number
  BOGO?: BOGOPromotion
  Buyers?: string[]
}

export interface HSPromoMinRequirement {
  Type?: MinRequirementType
  Int?: number
}

export enum HSPromoType {
  Percentage = 'Percentage',
  FixedAmount = 'FixedAmount',
  FreeShipping = 'FreeShipping',
  BOGO = 'BOGO',
}

export enum HSBogoType {
  Percentage = 'Percentage',
  FixedAmount = 'FixedAmount',
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

export interface BOGOPromotion {
  Type: HSBogoType
  Value: number
  BuySKU: BOGOSKU
  GetSKU: BOGOSKU
}

export interface BOGOSKU {
  SKU: string
  Qty: number
}
