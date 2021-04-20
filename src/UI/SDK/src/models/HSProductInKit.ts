import { Variant } from './Variant';
import { Spec } from './Spec';
import { HSProduct } from './HSProduct';
import { Asset } from './Asset';

export interface HSProductInKit {
    ID?: string
    MinQty?: number
    MaxQty?: number
    Static?: boolean
    Optional?: boolean
    SpecCombo?: string
    Variants?: Variant[]
    Specs?: Spec[]
    Product?: HSProduct
    Images?: Asset[]
    Attachments?: Asset[]
}