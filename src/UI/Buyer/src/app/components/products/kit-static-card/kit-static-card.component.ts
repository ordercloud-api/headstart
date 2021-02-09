import { Component, EventEmitter, Input, Output } from '@angular/core'
import { FormGroup } from '@angular/forms'
import {
  HSMeKitProduct,
  HSMeProductInKit,
  Spec,
  VariantSpec,
} from '@ordercloud/headstart-sdk'
import { LineItemToAdd, ProductSelectionEvent, QtyChangeEvent } from 'src/app/models/product.types'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { ProductDetailService } from '../product-details/product-detail.service'
import { SpecFormService } from '../spec-form/spec-form.service'

@Component({
  // ocm-kit-static-card
  templateUrl: './kit-static-card.component.html',
  styleUrls: ['./kit-static-card.component.scss'],
})
export class OCMKitStaticCard {
  @Input() set productKitDetails(value: HSMeProductInKit) {
    this._productKitDetails = value
    this.onInit()
  }
  @Input() kitProduct: HSMeKitProduct
  @Output() addLineItem = new EventEmitter<LineItemToAdd>()
  @Output() removeLineItem = new EventEmitter<LineItemToAdd>()
  @Output() selectProduct = new EventEmitter<ProductSelectionEvent>()

  _productKitDetails: HSMeProductInKit
  imageUrl: string
  quantityValid = false
  errorMessage: string
  quantity: number
  price: number
  specForm: FormGroup

  constructor(
    private context: ShopperContextService,
    private productDetailService: ProductDetailService,
    private specFormService: SpecFormService
  ) {}

  onInit(): void {
    const appSettings = this.context.appSettings
    this.imageUrl =
      this._productKitDetails.Images && this._productKitDetails.Images.length
        ? `${appSettings.cmsUrl}/assets/${appSettings.sellerID}/products/${this._productKitDetails.Product.ID}/thumbnail?size=S`
        : 'http://placehold.it/60x60'
    this.specForm = this.buildStaticSpecForm()
  }

  qtyChange({ valid, qty }: QtyChangeEvent): void {
    this.quantityValid = valid
    if (valid) {
      this.errorMessage = ''
      this.quantity = qty
      this.price = this.productDetailService.getProductPrice(
        this._productKitDetails.Product.PriceSchedule?.PriceBreaks ?? [],
        this.filterStaticKitSpecs(this._productKitDetails),
        qty,
        this.specForm
      )

      this.addLineItem.emit(this.buildLineItem())
    } else {
      this.removeLineItem.emit(this.buildLineItem())
      const maxQty = this._productKitDetails.Product.PriceSchedule.MaxQuantity
      const minQty = this._productKitDetails.Product.PriceSchedule.MinQuantity
      if (maxQty && qty > maxQty) {
        this.errorMessage = `Quantity must not exceed ${maxQty}`
      } else if (minQty && qty < minQty) {
        this.errorMessage = `Quantity must not be less than ${minQty}`
      }
    }
  }

  buildLineItem(): LineItemToAdd {
    return {
      ProductID: this._productKitDetails.Product.ID,
      Quantity: this.quantity,
      Specs: this.buildStaticKitSpecs(this._productKitDetails),
      Product: {
        Name: this._productKitDetails.Product.Name,
      },
      Price: this.price,
      xp: {
        ImageUrl: this.specFormService.getLineItemImageUrl(
          this._productKitDetails.Images,
          this._productKitDetails.Specs,
          this.specForm
        ),
        KitProductName: this.kitProduct.Name,
        KitProductID: this.kitProduct.ID,
        KitProductImageUrl:
          this.kitProduct.Images && this.kitProduct.Images.length
            ? this.kitProduct.Images[0].Url
            : null,
      },
    }
  }

  filterStaticKitSpecs(_productKitDetails: HSMeProductInKit): Spec[] {
    // filter our spec list to only include our pre-determined static specs
    const allowedSpecs = this.buildStaticKitSpecs(_productKitDetails)
    const filteredSpecs = _productKitDetails.Specs.filter((spec) =>
      allowedSpecs.find((s) => s.SpecID === spec.ID)
    ).map((spec) => {
      const allowedSpec = allowedSpecs.find((s) => s.SpecID === spec.ID)
      /** cast as any to set readonly property*/
      ;(spec as any).Options = spec.Options.filter(
        (o) => o.ID === allowedSpec.OptionID
      )
      return spec
    })
    return filteredSpecs
  }

  buildStaticKitSpecs(_productKitDetails: HSMeProductInKit): VariantSpec[] {
    // a static kit is one where the options for all specs are pre-determined
    // spec combo is the identifier for the combination of spec/variant
    const specCombo = _productKitDetails.SpecCombo
    const variant = _productKitDetails.Variants.find(
      (v: any) => v.xp.SpecCombo === specCombo
    )
    if (!variant) {
      return []
    }
    return variant.Specs
  }

  buildStaticSpecForm(): FormGroup {
    // build up a formgroup with the static spec values for this product
    const form = {
      valid: true,
      value: {},
    }
    const specs = this.filterStaticKitSpecs(this._productKitDetails)
    specs.forEach((spec) => {
      form.value[spec.Name] = spec.Options[0].Value
    })
    return form as FormGroup
  }

  onSelectProduct(): void {
    this.selectProduct.emit({ productKitDetails: this._productKitDetails })
  }
}
