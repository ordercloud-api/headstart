import { MerchantDefinition } from "../models/config.types"

export class MerchantConfig {
  static merchants: MerchantDefinition[] = [
    { cardConnectMerchantID: '840000000052', currency: 'USD' },
    { cardConnectMerchantID: '840000000037', currency: 'CAD' },
    { cardConnectMerchantID: '840000000062', currency: 'EUR' },
  ]

  static getMerchant(currency: string): MerchantDefinition {
    return this.merchants.find((merchant) => merchant.currency === currency)
  }
}
