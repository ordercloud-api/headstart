import { Address } from "ordercloud-javascript-sdk";

export interface OrderCloudIntegrationsCreditCardToken {
    AccountNumber?: string
    ExpirationDate?: string
    CardholderName?: string
    CardType?: string
    CCBillingAddress?: Address
}