import { Component, Input } from '@angular/core'
import {
  Asset,
  HSMeKitProduct,
  HSMeProductInKit,
} from '@ordercloud/headstart-sdk'
import { ProductDetailService } from '../product-details/product-detail.service'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { KitVariantSelection, LineItemToAdd, ProductSelectionEvent } from 'src/app/models/product.types'

@Component({
  templateUrl: './kit-product-details.component.html',
  styleUrls: ['./kit-product-details.component.scss'],
})
export class OCMKitProductDetails {
  @Input() set product(product: HSMeKitProduct) {
    this._product = product
    this.images = product.Images
  }

  isAddingToCart = false
  _product: HSMeKitProduct
  variantSelection: KitVariantSelection
  productSelectionEvent: ProductSelectionEvent
  lineItemsToAdd: LineItemToAdd[] = []
  selectedProduct: HSMeProductInKit
  images: Asset[]

  constructor(
    private productDetailService: ProductDetailService,
    private context: ShopperContextService
  ) {}

  selectProduct(event: ProductSelectionEvent): void {
    this.productSelectionEvent = event
    this.images = event.productKitDetails.Images
  }

  async addToCart(): Promise<void> {
    this.isAddingToCart = true
    try {
      await this.context.order.cart.addMany(this.lineItemsToAdd)
    } finally {
      this.isAddingToCart = false
    }
  }

  addLineItem(newline: LineItemToAdd): void {
    const matchingIndex = this.lineItemsToAdd.findIndex((li) =>
      this.productDetailService.isSameLine(li, newline)
    )
    if (matchingIndex > -1) {
      // line item exists, replace it
      this.lineItemsToAdd = this.lineItemsToAdd.map((li, index) =>
        index === matchingIndex ? newline : li
      )
    } else {
      // line item doesnt exist, add it to array
      this.lineItemsToAdd = [...this.lineItemsToAdd, newline]
    }
  }

  removeLineItem(lineToRemove: LineItemToAdd): void {
    this.lineItemsToAdd = this.lineItemsToAdd.filter(
      (li) => !this.productDetailService.isSameLine(li, lineToRemove)
    )
  }

  canAddToCart(): boolean {
    // the cart is valid if all kit products have at least one associated line item
    // variable kit products may have more than one
    if (
      !this._product ||
      !this._product.ProductAssignments.ProductsInKit.length ||
      !this.lineItemsToAdd?.length
    ) {
      return false
    }
    const productsAddedToCart = this.lineItemsToAdd.map((li) => li.ProductID)
    return this._product.ProductAssignments.ProductsInKit.every(
      (details) =>
        details.Optional || productsAddedToCart.includes(details.Product.ID)
    )
  }
}
