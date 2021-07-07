import { PriceSchedule, Spec } from 'ordercloud-javascript-sdk';
import { ImageAsset } from './Asset';
import { HSProduct } from './HSProduct';
import { HSVariant } from './HSVariant';

export interface SuperHSProduct {
    ID?: string
    Product?: HSProduct
    PriceSchedule?: PriceSchedule
    Specs?: Spec[]
    Variants?: HSVariant[]
    Images?: ImageAsset[]
    Attachments?: ImageAsset[]
}