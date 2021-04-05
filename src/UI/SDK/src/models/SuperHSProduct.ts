import { HSProduct } from './HSProduct';
import { PriceSchedule } from './PriceSchedule';
import { Spec } from './Spec';
import { HSVariant } from './HSVariant';
import { Asset } from './Asset';

export interface SuperHSProduct {
    ID?: string
    Product?: HSProduct
    PriceSchedule?: PriceSchedule
    Specs?: Spec[]
    Variants?: HSVariant[]
    Images?: Asset[]
    Attachments?: Asset[]
}