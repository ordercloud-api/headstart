import { OrderCloudIntegrationsCreditCardToken } from './OrderCloudIntegrationsCreditCardToken';

export interface OrderCloudIntegrationsCreditCardPayment {
    OrderID?: string
    PaymentID?: string
    CreditCardID?: string
    CreditCardDetails?: OrderCloudIntegrationsCreditCardToken
    Currency?: string
    CVV?: string
    MerchantID?: string
}