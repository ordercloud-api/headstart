import { ProductXp } from './ProductXp';
import { BuyerProduct } from 'ordercloud-javascript-sdk';
import { PriceScheduleXp } from './PriceScheduleXp';

export type HSMeProduct = BuyerProduct<ProductXp, PriceScheduleXp>