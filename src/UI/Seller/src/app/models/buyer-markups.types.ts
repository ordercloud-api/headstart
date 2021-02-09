import { HSBuyer } from '@ordercloud/headstart-sdk'

export interface HSBuyerPriceMarkup {
  Buyer: HSBuyer
  Markup: BuyerMarkup
}

interface BuyerMarkup {
  Percent: number
}
