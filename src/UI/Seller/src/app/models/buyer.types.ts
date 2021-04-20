import { Address, ImpersonationConfig } from '@ordercloud/angular-sdk';
import { HSCatalog } from '@ordercloud/headstart-sdk'
import { Buyer } from 'ordercloud-javascript-sdk';

export interface HSBuyerPriceMarkup {
  Buyer: Buyer //todo replace with HSBuyer once sdk is updated
  Markup: BuyerMarkup
  ImpersonationConfig?: ImpersonationConfig
}

interface BuyerMarkup {
  Percent: number
}

export interface HSBuyerData {
    Addresses: Address[]
    Catalogs: HSCatalog[]
}