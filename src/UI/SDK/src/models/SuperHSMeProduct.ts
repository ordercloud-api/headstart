import { HSMeProduct } from './HSMeProduct';
import { PriceSchedule } from './PriceSchedule';
import { Spec } from './Spec';
import { HSVariant } from './HSVariant';
import { Asset } from './Asset';

export interface SuperHSMeProduct {
    ID?: string
    Product?: HSMeProduct
    PriceSchedule?: PriceSchedule
    Specs?: Spec[]
    Variants?: HSVariant[]
    Images?: Asset[]
    Attachments?: Asset[]
}