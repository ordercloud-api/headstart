import { DocumentAsset, ImageAsset } from './Asset';
import { TaxCategorization } from './TaxCategorization';
import { UnitOfMeasure } from './UnitOfMeasure';

export interface ProductXp {
    Note?: string
    Tax?: TaxCategorization
    UnitOfMeasure?: UnitOfMeasure
    ProductType?: 'Standard' | 'Quote'
    SizeTier?: 'G' | 'A' | 'B' | 'C' | 'D' | 'E' | 'F'
    IsResale?: boolean
    Currency?: 'CAD' | 'HKD' | 'ISK' | 'PHP' | 'DKK' | 'HUF' | 'CZK' | 'GBP' | 'RON' | 'SEK' | 'IDR' | 'INR' | 'BRL' | 'RUB' | 'HRK' | 'JPY' | 'THB' | 'CHF' | 'EUR' | 'MYR' | 'BGN' | 'TRY' | 'CNY' | 'NOK' | 'NZD' | 'ZAR' | 'USD' | 'MXN' | 'SGD' | 'AUD' | 'ILS' | 'KRW' | 'PLN'
    PromotionEligible?: boolean
    FreeShipping?: boolean
    FreeShippingMessage?: string
    Images?: ImageAsset[]
    Documents?: DocumentAsset[]
}