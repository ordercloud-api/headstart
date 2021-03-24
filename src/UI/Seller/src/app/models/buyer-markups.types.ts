import { ImpersonationConfig } from '@ordercloud/angular-sdk';
import { HSBuyer } from '@ordercloud/headstart-sdk'
import { Buyer } from 'ordercloud-javascript-sdk';

export interface HSBuyerPriceMarkup {
  Buyer: Buyer //todo replace with HSBuyer once sdk is updated
  Markup: BuyerMarkup
  ImpersonationConfig?: ImpersonationConfig
}

interface BuyerMarkup {
  Percent: number
}
