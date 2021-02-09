import { HSVariant, HSMeProductInKit, SpecOption } from '@ordercloud/headstart-sdk'
import { ValidatorFn } from '@angular/forms'
import { FormGroup } from '@angular/forms'
import { LineItemSpec } from 'ordercloud-javascript-sdk'

export interface QtyChangeEvent {
    qty: number
    valid: boolean
  }

export interface FieldConfig {
    disabled?: boolean
    label?: string
    name: string
    options?: any[]
    placeholder?: string
    min?: number
    max?: number
    step?: number
    rows?: number
    type: string
    validation?: ValidatorFn[]
    value?: any
    currency?: string
    disabledVariants?: HSVariant[]
}

export interface Field {
    config: FieldConfig
    group: FormGroup
    index: number
    compact?: boolean
}

export interface SpecFormEvent {
    form: FormGroup
}

export interface GridSpecOption {
    SpecID: string
    OptionID: string
    Value: string
    MarkupType: string
    Markup: number
}

export interface FullSpecOption extends SpecOption {
    SpecID: string
  }

  export interface LineItemToAdd {
    ProductID: string
    Quantity: number
    Specs: LineItemSpec[]
    Product: {
      // adding purely so i can use productNameWithSpecs pipe without modification
      Name: string
    }
    Price: number // adding for display purposes
    xp: {
      ImageUrl: string
      KitProductName: string
      KitProductImageUrl: string
      KitProductID: string
    }
  }

export interface ProductSelectionEvent {
  productKitDetails: HSMeProductInKit
  variantSelection?: KitVariantSelection
}

export interface KitVariantSelection {
  productID: string
  specForm: FormGroup
  quantity: number
}
