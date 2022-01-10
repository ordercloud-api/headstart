import { DocumentAsset, ImageAsset } from './Asset';
import { TaxCategorization } from './TaxCategorization';
import { UnitOfMeasure } from './UnitOfMeasure';

export interface ProductXp {
    Status?: 'Draft' | 'Published'
    HasVariants?: boolean
    Note?: string
    Tax?: TaxCategorization
    UnitOfMeasure?: UnitOfMeasure
    ProductType?: 'Standard' | 'Quote'
    SizeTier?: 'G' | 'A' | 'B' | 'C' | 'D' | 'E' | 'F'
    IsResale?: boolean
    Accessorials?: 'Freezable' | 'Hazmat' | 'KeepFromFreezing'
    Currency?: 'CAD' | 'HKD' | 'ISK' | 'PHP' | 'DKK' | 'HUF' | 'CZK' | 'GBP' | 'RON' | 'SEK' | 'IDR' | 'INR' | 'BRL' | 'RUB' | 'HRK' | 'JPY' | 'THB' | 'CHF' | 'EUR' | 'MYR' | 'BGN' | 'TRY' | 'CNY' | 'NOK' | 'NZD' | 'ZAR' | 'USD' | 'MXN' | 'SGD' | 'AUD' | 'ILS' | 'KRW' | 'PLN'
    ArtworkRequired?: boolean
    PromotionEligible?: boolean
    FreeShipping?: boolean
    FreeShippingMessage?: string
    Images?: ImageAsset[]
    Documents?: DocumentAsset[]
}