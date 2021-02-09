import { Filter } from '@ordercloud/headstart-sdk'

export interface SupplierFilterConfigDocument extends Document {
  Doc: SupplierFilterConfig
}
export interface SupplierFilterConfig {
  Display: string
  Path: string
  Items: Filter[]
  AllowSupplierEdit: boolean
  AllowSellerEdit: boolean
  BuyerAppFilterType: BuyerAppFilterType
}
export declare enum BuyerAppFilterType {
  SelectOption = 'SelectOption',
  NonUI = 'NonUI',
}

export interface FilterObject {
  name: string
  path: string
  nestedDataPath?: string
  dataKey?: string
  sourceType: string
  source: string
  filterValues: any[]
}
