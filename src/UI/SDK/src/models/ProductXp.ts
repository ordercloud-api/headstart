import { TaxProperties } from './TaxProperties';
import { UnitOfMeasure } from './UnitOfMeasure';

export interface ProductXp {
    Status?: 'Draft' | 'Published'
    HasVariants?: boolean
    Note?: string
    Tax?: TaxProperties
    UnitOfMeasure?: UnitOfMeasure
    ProductType?: 'Standard' | 'Quote' | 'PurchaseOrder' | 'Kit'
    SizeTier?: 'G' | 'A' | 'B' | 'C' | 'D' | 'E' | 'F'
    IsResale?: boolean
    Accessorials?: 'Freezable' | 'Hazmat' | 'KeepFromFreezing'
    Currency?: 'CAD' | 'HKD' | 'ISK' | 'PHP' | 'DKK' | 'HUF' | 'CZK' | 'GBP' | 'RON' | 'SEK' | 'IDR' | 'INR' | 'BRL' | 'RUB' | 'HRK' | 'JPY' | 'THB' | 'CHF' | 'EUR' | 'MYR' | 'BGN' | 'TRY' | 'CNY' | 'NOK' | 'NZD' | 'ZAR' | 'USD' | 'MXN' | 'SGD' | 'AUD' | 'ILS' | 'KRW' | 'PLN'
    ArtworkRequired?: boolean
    PromotionEligible?: boolean
    FreeShipping?: boolean
    FreeShippingMessage?: string
}