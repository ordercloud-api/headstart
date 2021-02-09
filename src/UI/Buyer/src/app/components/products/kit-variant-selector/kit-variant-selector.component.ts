import { Component, EventEmitter, Input, Output } from '@angular/core'
import { FormGroup } from '@angular/forms'
import {
  HSMeKitProduct,
  HSMeProductInKit,
  Variant,
} from '@ordercloud/headstart-sdk'
import { KitVariantSelection, LineItemToAdd, ProductSelectionEvent, QtyChangeEvent, SpecFormEvent } from 'src/app/models/product.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { ProductDetailService } from '../product-details/product-detail.service'
import { SpecFormService } from '../spec-form/spec-form.service'

@Component({
  // ocm-kit-variant-selector
  templateUrl: './kit-variant-selector.component.html',
  styleUrls: ['./kit-variant-selector.component.scss'],
})
export class OCMKitVariantSelector {
  @Input() set event(value: ProductSelectionEvent) {
    this._event = value
    this.onInit()
  }
  @Input() set allLineItems(value: LineItemToAdd[]) {
    this._allLineItems = value
  }
  @Input() kitProduct: HSMeKitProduct
  @Output() addLineItem = new EventEmitter<LineItemToAdd>()
  _allLineItems: LineItemToAdd[]
  _event: ProductSelectionEvent
  productKitDetails: HSMeProductInKit
  selection: KitVariantSelection
  disabledVariants: Variant[]
  specForm = {} as FormGroup
  price = 0
  quantity = 0
  quantityValid: boolean
  errorMessage: string
  resetFormToggle = true
  userCurrency: string

  constructor(
    private productDetailService: ProductDetailService,
    private specFormService: SpecFormService,
    private context: ShopperContextService
  ) {}

  onInit(): void {
    this.productKitDetails = this._event.productKitDetails
    this.selection = this._event.variantSelection
    this.disabledVariants = this.productKitDetails.Variants.filter(
      (v) => !v.Active
    )
    const currentUser = this.context.currentUser.get()
    this.userCurrency = currentUser.Currency
  }

  onSpecFormChange(event: SpecFormEvent): void {
    this.specForm = event.form
    this.price = this.getPrice()
  }

  qtyChange({ qty, valid }: QtyChangeEvent): void {
    this.quantityValid = valid
    if (valid) {
      this.errorMessage = ''
      this.quantity = qty
      this.price = this.getPrice()
    } else {
      const maxQty = this.productKitDetails.Product.PriceSchedule.MaxQuantity
      const minQty = this.productKitDetails.Product.PriceSchedule.MinQuantity
      if (maxQty && qty > maxQty) {
        this.errorMessage = `Quantity must not exceed ${maxQty}`
      } else if (minQty && qty < minQty) {
        this.errorMessage = `Quantity must not be less than ${minQty}`
      }
    }
  }

  getPrice(): number {
    return this.productDetailService.getProductPrice(
      this.productKitDetails.Product.PriceSchedule?.PriceBreaks ?? [],
      this.productKitDetails.Specs,
      this.quantity,
      this.specForm
    )
  }

  addVariantSelection(): void {
    const lineItemToAdd: LineItemToAdd = this.buildLineItem()
    if (
      this.productDetailService.isSameLineAsOthers(
        lineItemToAdd,
        this._allLineItems
      ) &&
      !window.confirm(
        'You have an existing selection with the same options. Adding this selection will overwrite your existing selection, would you like to proceed?'
      )
    ) {
      return
    }

    this.addLineItem.emit(lineItemToAdd)

    // dirty hack to reset quantity input and ocm-spec-form
    this.price = 0
    this.quantity = 0
    this.resetFormToggle = false
    window.setTimeout(() => {
      this.resetFormToggle = true
    })
  }

  buildLineItem(): LineItemToAdd {
    return {
      ProductID: this.productKitDetails.Product.ID,
      Product: {
        Name: this.productKitDetails.Product.Name,
      },
      Price: this.price,
      Quantity: this.quantity,
      Specs: this.specFormService.getLineItemSpecs(
        this.productKitDetails.Specs,
        this.specForm
      ),
      xp: {
        ImageUrl:
          this.specFormService.getLineItemImageUrl(
            this.productKitDetails.Images,
            this.productKitDetails.Specs,
            this.specForm
          ) || 'http://placehold.it/60x60',
        KitProductName: this.kitProduct.Name, // TODO remove this once i rebuild package with types
        KitProductID: this.kitProduct.ID,
        KitProductImageUrl:
          this.kitProduct.Images && this.kitProduct.Images.length
            ? this.kitProduct.Images[0].Url
            : 'http://placehold.it/60x60',
      },
    }
  }
}
