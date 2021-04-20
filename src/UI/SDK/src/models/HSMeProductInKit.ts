import { Variant } from './Variant';
import { Spec } from './Spec';
import { HSMeProduct } from './HSMeProduct';
import { Asset } from './Asset';

export interface HSMeProductInKit {
    ID?: string
    MinQty?: number
    MaxQty?: number
    Static?: boolean
    Optional?: boolean
    SpecCombo?: string
    Variants?: Variant[]
    Specs?: Spec[]
    Product?: HSMeProduct
    Images?: Asset[]
    Attachments?: Asset[]
}