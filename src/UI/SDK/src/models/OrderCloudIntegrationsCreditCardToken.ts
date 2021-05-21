import { Address } from "node:cluster";

export interface OrderCloudIntegrationsCreditCardToken {
    AccountNumber?: string
    ExpirationDate?: string
    CardholderName?: string
    CardType?: string
    CCBillingAddress?: Address
}