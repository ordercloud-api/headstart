import { ChiliSpecXp } from './ChiliSpecXp';
import { SpecOption } from './SpecOption';

export interface ChiliSpec {
    xp?: ChiliSpecXp
    readonly Options?: SpecOption[]
    ID?: string
    ListOrder?: number
    Name: string
    DefaultValue?: string
    Required?: boolean
    AllowOpenText?: boolean
    DefaultOptionID?: string
    DefinesVariant?: boolean
    readonly OptionCount?: number
}