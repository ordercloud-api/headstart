import { Address } from "ordercloud-javascript-sdk";

export enum ShippingStatus {
    Shipped = 'Shipped',
    PartiallyShipped = 'PartiallyShipped',
    Canceled = 'Canceled',
    Processing = 'Processing',
    Backordered = 'Backordered',
  }

  export interface ShippingRate {
    Id: string
    AccountName: string
    Carrier: string
    Currency: string
    DeliveryDate: Date
    DeliveryDays: number
    CarrierQuoteId: string
    Service: string
    TotalCost: number
  }

  export interface ShipFromSourcesDic {
    [key: string]: Address[]
  }