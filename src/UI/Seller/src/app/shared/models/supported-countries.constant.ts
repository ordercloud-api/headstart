// replace with sdk in the future

import { SupportedCountries } from '@app-seller/models/currency-geography.types'

export class GeographyConfig {
  static getCountries(): SupportedCountries[] {
    return [
      { label: 'Australia', abbreviation: 'AU', currency: 'AUD' },
      { label: 'Austria', abbreviation: 'AT', currency: 'EUR' },
      { label: 'Belgium', abbreviation: 'BE', currency: 'EUR' },
      { label: 'Canada', abbreviation: 'CA', currency: 'CAD' },
      { label: 'Cyprus', abbreviation: 'CY', currency: 'EUR' },
      { label: 'Estonia', abbreviation: 'EE', currency: 'EUR' },
      { label: 'Finland', abbreviation: 'FI', currency: 'EUR' },
      { label: 'France', abbreviation: 'FR', currency: 'EUR' },
      { label: 'Germany', abbreviation: 'DE', currency: 'EUR' },
      { label: 'Greece', abbreviation: 'GR', currency: 'EUR' },
      { label: 'Ireland', abbreviation: 'IE', currency: 'EUR' },
      { label: 'Italy', abbreviation: 'IT', currency: 'EUR' },
      { label: 'Latvia', abbreviation: 'LV', currency: 'EUR' },
      { label: 'Lithuania', abbreviation: 'LT', currency: 'EUR' },
      { label: 'Luxembourg', abbreviation: 'LU', currency: 'EUR' },
      { label: 'Malta', abbreviation: 'MT', currency: 'EUR' },
      { label: 'The Netherlands', abbreviation: 'NL', currency: 'EUR' },
      { label: 'Portugal', abbreviation: 'PT', currency: 'EUR' },
      { label: 'Slovakia', abbreviation: 'SK', currency: 'EUR' },
      { label: 'Slovenia', abbreviation: 'SI', currency: 'EUR' },
      { label: 'Spain', abbreviation: 'ES', currency: 'EUR' },
      {
        label: 'United States of America',
        abbreviation: 'US',
        currency: 'USD',
      },
    ]
  }
}
