export interface HeadstartExchangeRates {
  Currency: string
  Symbol: string
  Name: string
  Rate: number
  Icon: string
}

// TODO - remove when sdk has enum types
export type CurrenySymbol =
  | 'CAD'
  | 'HKD'
  | 'ISK'
  | 'PHP'
  | 'DKK'
  | 'HUF'
  | 'CZK'
  | 'GBP'
  | 'RON'
  | 'SEK'
  | 'IDR'
  | 'INR'
  | 'BRL'
  | 'RUB'
  | 'HRK'
  | 'JPY'
  | 'THB'
  | 'CHF'
  | 'EUR'
  | 'MYR'
  | 'BGN'
  | 'TRY'
  | 'CNY'
  | 'NOK'
  | 'NZD'
  | 'ZAR'
  | 'USD'
  | 'MXN'
  | 'SGD'
  | 'AUD'
  | 'ILS'
  | 'KRW'
  | 'PLN'

  export interface MerchantDefinition {
    cardConnectMerchantID: string
    currency: string
}