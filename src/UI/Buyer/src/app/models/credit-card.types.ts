import { OrderCloudIntegrationsCreditCardToken } from "@ordercloud/headstart-sdk";
import { Address, BuyerCreditCard } from "ordercloud-javascript-sdk";

export interface CreditCardFormOutput {
    card: OrderCloudIntegrationsCreditCardToken
    cvv: string
}

export interface SelectedCreditCard {
  SavedCard?: HSBuyerCreditCard
  NewCard?: OrderCloudIntegrationsCreditCardToken
  CVV: string
}

export interface CreditCard {
  token: string
  name: string
  month: string
  year: string
  street: string
  state: string
  city: string
  zip: string
  country: string
  cvv: string
}

export type HSBuyerCreditCard = BuyerCreditCard<CreditCardXP>

export interface CreditCardXP {
  CCBillingAddress: Address
}

export type ComponentChange<T, P extends keyof T> = {
  previousValue: T[P]
  currentValue: T[P]
  firstChange: boolean
}

export type ComponentChanges<T> = {
  [P in keyof T]?: ComponentChange<T, P>
}
