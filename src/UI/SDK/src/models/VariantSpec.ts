
export interface VariantSpec {
    SpecID?: string
    readonly Name?: string
    OptionID?: string
    Value?: string
    PriceMarkupType?: 'NoMarkup' | 'AmountPerQuantity' | 'AmountTotal' | 'Percentage'
    PriceMarkup?: number
}