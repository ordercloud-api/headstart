import { ImageAsset } from './Asset';
import { Contact } from './Contact';
import { SupplierCategory } from './SupplierCategory';

export interface SupplierXp {
    Description?: string
    SupportContact?: Contact
    SyncFreightPop?: boolean
    ApiClientID?: string
    Currency?: 'CAD' | 'HKD' | 'ISK' | 'PHP' | 'DKK' | 'HUF' | 'CZK' | 'GBP' | 'RON' | 'SEK' | 'IDR' | 'INR' | 'BRL' | 'RUB' | 'HRK' | 'JPY' | 'THB' | 'CHF' | 'EUR' | 'MYR' | 'BGN' | 'TRY' | 'CNY' | 'NOK' | 'NZD' | 'ZAR' | 'USD' | 'MXN' | 'SGD' | 'AUD' | 'ILS' | 'KRW' | 'PLN'
    ProductTypes?: 'Standard' | 'Quote'
    CountriesServicing?: string[]
    BuyersServicing?: string[]
    Categories?: SupplierCategory[]
    NotificationRcpts?: string[]
    FreeShippingThreshold?: number
    Image?: ImageAsset
}