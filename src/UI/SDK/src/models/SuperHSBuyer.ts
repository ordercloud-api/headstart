import { ImpersonationConfig } from 'ordercloud-javascript-sdk';
import { HSBuyer } from './HSBuyer';

export interface SuperHSBuyer {
    Buyer?: HSBuyer
    ImpersonationConfig?: ImpersonationConfig
}