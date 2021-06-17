
export interface TemplateProductFlat {
    ID?: string
    Active?: boolean
    ProductType?: 'Standard' | 'Quote'
    Name?: string
    Description?: string
    QuantityMultiplier?: number
    ShipFromAddressID?: string
    ShipWeight?: number
    ShipHeight?: number
    ShipWidth?: number
    ShipLength?: number
    TaxCategory?: string
    TaxCode?: string
    TaxDescription?: string
    UnitOfMeasureQuantity?: number
    UnitOfMeasure?: string
    IsResale?: boolean
    ApplyTax?: boolean
    ApplyShipping?: boolean
    MinQuantity?: number
    MaxQuantity?: number
    UseCumulativeQuantity?: boolean
    RestrictedQuantity?: boolean
    Price?: number
    Currency?: 'CAD' | 'HKD' | 'ISK' | 'PHP' | 'DKK' | 'HUF' | 'CZK' | 'GBP' | 'RON' | 'SEK' | 'IDR' | 'INR' | 'BRL' | 'RUB' | 'HRK' | 'JPY' | 'THB' | 'CHF' | 'EUR' | 'MYR' | 'BGN' | 'TRY' | 'CNY' | 'NOK' | 'NZD' | 'ZAR' | 'USD' | 'MXN' | 'SGD' | 'AUD' | 'ILS' | 'KRW' | 'PLN'
    ImageTitle?: string
    Url?: string
    Type?: 'Image' | 'Text' | 'Audio' | 'Video' | 'Presentation' | 'SpreadSheet' | 'PDF' | 'Compressed' | 'Code' | 'JSON' | 'Markup' | 'Unknown'
    Tags?: string
    FileName?: string
    SizeTier?: 'G' | 'A' | 'B' | 'C' | 'D' | 'E' | 'F'
}