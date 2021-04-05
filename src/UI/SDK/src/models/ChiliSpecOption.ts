import { ChiliSpecOptionXp } from './ChiliSpecOptionXp';

export interface ChiliSpecOption {
    xp?: ChiliSpecOptionXp
    ID?: string
    Value: string
    ListOrder?: number
    IsOpenText?: boolean
    PriceMarkupType?: 'NoMarkup' | 'AmountPerQuantity' | 'AmountTotal' | 'Percentage'
    PriceMarkup?: number
}