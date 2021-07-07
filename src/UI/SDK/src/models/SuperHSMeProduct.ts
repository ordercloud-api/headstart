import { PriceSchedule, Spec } from 'ordercloud-javascript-sdk';
import { ImageAsset } from './Asset';
import { HSMeProduct } from './HSMeProduct';
import { HSVariant } from './HSVariant';

export interface SuperHSMeProduct {
    ID?: string
    Product?: HSMeProduct
    PriceSchedule?: PriceSchedule
    Specs?: Spec[]
    Variants?: HSVariant[]
    Images?: ImageAsset[]
    Attachments?: ImageAsset[]
}