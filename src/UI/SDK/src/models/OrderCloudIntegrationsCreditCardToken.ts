import { Address } from './Address';

export interface OrderCloudIntegrationsCreditCardToken {
    AccountNumber?: string
    ExpirationDate?: string
    CardholderName?: string
    CardType?: string
    CCBillingAddress?: Address
}