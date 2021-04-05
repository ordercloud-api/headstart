import { SpecOption } from './SpecOption';

export interface Spec {
    ID?: string
    ListOrder?: number
    Name: string
    DefaultValue?: string
    Required?: boolean
    AllowOpenText?: boolean
    DefaultOptionID?: string
    DefinesVariant?: boolean
    readonly OptionCount?: number
    readonly Options?: SpecOption[]
}