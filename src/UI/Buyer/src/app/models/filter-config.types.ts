import { Filter } from "@ordercloud/headstart-sdk";
import { Sortable } from "ordercloud-javascript-sdk";

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
  
  export enum BuyerAppFilterType {
    SelectOption = 'SelectOption',
    NonUI = 'NonUI',
  }

  export interface SupplierFilters {
    supplierID?: string
    page?: number
    sortBy?: Sortable<'Suppliers.List'>
    activeFilters?: any
    search?: string
  }

  export interface ProductFilters {
    page?: number
    sortBy?: string[]
    search?: string
    showOnlyFavorites?: boolean
    categoryID?: string
    activeFacets?: any
  }
  