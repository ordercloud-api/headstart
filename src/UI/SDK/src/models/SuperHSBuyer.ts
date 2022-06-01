import { ImpersonationConfig } from '@ordercloud/angular-sdk';
import { HSBuyer } from './HSBuyer';

export interface SuperHSBuyer {
    Buyer?: HSBuyer
    ImpersonationConfig?: ImpersonationConfig
}