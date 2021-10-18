export interface SupportedCountries {
  label: string
  abbreviation: string
  currency: string
}

export interface SupportedRates {
  Currency: string
  Symbol: string
  Name: string
  Icon?: string
}

export enum SupportedCurrencies {
  AUD = 'AUD',
  USD = 'USD',
  CAD = 'CAD',
  EUR = 'EUR',
}

export interface CountryDefinition {
  label: string
  abbreviation: string
}

export interface StateDefinition {
  label: string
  abbreviation: string
  country: string
}
