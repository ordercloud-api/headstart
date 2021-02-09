import { FilterObject } from '@app-seller/shared'

export const buyerLocation: FilterObject[] = [
  {
    name: 'Buyer',
    path: 'BuyerID',
    dataKey: 'ID',
    sourceType: 'oc',
    source: 'ocBuyerService',
    filterValues: [],
  },
  {
    name: 'Country',
    path: 'Country',
    dataKey: 'abbreviation',
    sourceType: 'model',
    source: 'GeographyConfig',
    filterValues: [],
  },
  {
    name: 'State',
    path: 'State',
    dataKey: 'abbreviation',
    sourceType: 'model',
    source: 'GeographyConfig',
    filterValues: [],
  },
]

export const salesOrderDetail: FilterObject[] = [
  {
    name: 'Submitted Order Status',
    path: 'SubmittedOrderStatus',
    sourceType: 'model',
    source: 'SubmittedOrderStatus',
    filterValues: [],
  },
  {
    name: 'Order Type',
    path: 'OrderType',
    sourceType: 'model',
    source: 'OrderType',
    filterValues: [],
  },
  {
    name: 'Country',
    path: 'Country',
    dataKey: 'abbreviation',
    sourceType: 'model',
    source: 'GeographyConfig',
    filterValues: [],
  },
]

export const purchaseOrderDetail: FilterObject[] = [
  {
    name: 'Submitted Order Status',
    path: 'SubmittedOrderStatus',
    sourceType: 'model',
    source: 'SubmittedOrderStatus',
    filterValues: [],
  },
  {
    name: 'Order Type',
    path: 'OrderType',
    sourceType: 'model',
    source: 'OrderType',
    filterValues: [],
  },
]

export const lineItemDetail: FilterObject[] = [
  {
    name: 'Submitted Order Status',
    path: 'SubmittedOrderStatus',
    sourceType: 'model',
    source: 'SubmittedOrderStatus',
    filterValues: [],
  },
  {
    name: 'Order Type',
    path: 'OrderType',
    sourceType: 'model',
    source: 'OrderType',
    filterValues: [],
  },
  {
    name: 'Country',
    path: 'Country',
    dataKey: 'abbreviation',
    sourceType: 'model',
    source: 'GeographyConfig',
    filterValues: [],
  },
]
