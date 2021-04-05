
export interface SpecOption {
    ID?: string
    Value: string
    ListOrder?: number
    IsOpenText?: boolean
    PriceMarkupType?: 'NoMarkup' | 'AmountPerQuantity' | 'AmountTotal' | 'Percentage'
    PriceMarkup?: number
}