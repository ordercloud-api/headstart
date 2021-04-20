import { SpecOptionXp } from './SpecOptionXp';

export interface HSSpecOption {
    xp?: SpecOptionXp
    ID?: string
    Value?: string
    ListOrder?: number
    IsOpenText?: boolean
    PriceMarkupType?: 'NoMarkup' | 'AmountPerQuantity' | 'AmountTotal' | 'Percentage'
    PriceMarkup?: number
}