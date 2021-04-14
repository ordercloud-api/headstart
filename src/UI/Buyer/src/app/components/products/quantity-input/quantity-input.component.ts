import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core'
import { FormGroup, Validators, FormControl } from '@angular/forms'
import { PriceSchedule } from 'ordercloud-javascript-sdk'
import { Router } from '@angular/router'
import {
  HeadStartSDK,
  HSLineItem,
  HSMeProduct,
  HSVariant,
} from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { QtyChangeEvent } from 'src/app/models/product.types'

@Component({
  templateUrl: './quantity-input.component.html',
  styleUrls: ['./quantity-input.component.scss'],
})
export class OCMQuantityInput implements OnInit, OnChanges {
  @Input() priceSchedule: PriceSchedule
  @Input() product: HSMeProduct
  @Input() selectedVariant: HSVariant
  @Input() li: HSLineItem
  @Input() groupedLineItems: HSLineItem[]
  @Input() variantInventory?: number
  @Input() variantID: string
  @Input() isAddingToCart: boolean
  @Input() existingQty: number
  @Input() gridDisplay? = false
  @Input() noChange? = false
  @Input() isQtyChanging
  @Input() resetGridQtyFields
  @Output() qtyChange = new EventEmitter<QtyChangeEvent>()
  // TODO - replace with real product info
  form: FormGroup
  isQtyRestricted = false
  restrictedQuantities: number[] = []
  errorMsg = ''
  inventory: number
  min: number
  max: number
  cumulativeQuantity: number
  disabled = false
  qtyPreUpdate: number

  constructor(private router: Router, private context: ShopperContextService) {}

  ngOnInit(): void {
    const routeUrl = this.router.routerState.snapshot.url
    const splitUrl = routeUrl.split('/')
    const endUrl = splitUrl[splitUrl.length - 1]
    if (endUrl.includes('cart')) {
      this.form = new FormGroup({
        quantity: new FormControl(1, {
          validators: Validators.required,
          updateOn: 'blur',
        }),
      })
    } else {
      this.form = new FormGroup({
        quantity: new FormControl(1, {
          validators: Validators.required,
        }),
      })
    }
  }

  async ngOnChanges(changes: SimpleChanges): Promise<void> {
    if (
      changes?.variantID?.previousValue !== changes?.variantID?.currentValue &&
      !this.noChange
    ) {
      const _product = await HeadStartSDK.Mes.GetSuperProduct(this.product?.ID)
      const variant = _product?.Variants?.find((v) => v.ID === this.variantID)
      this.variantInventory = variant?.Inventory?.QuantityAvailable
      this.inventory = this.getInventory(_product.Product)
    }
    if (this.product && this.priceSchedule)
      this.init(this.product, this.priceSchedule)
    if (!this.isAddingToCart && !this.gridDisplay) {
      this.validateQty(this.getDefaultQty())
    }
    if (this.resetGridQtyFields) {
      this.form.reset()
    }
  }

  init(product: HSMeProduct, priceSchedule: PriceSchedule): void {
    this.disabled =
      this.isQtyChanging ||
      priceSchedule.MinQuantity === priceSchedule.MaxQuantity
    this.isQtyRestricted = priceSchedule.RestrictedQuantity
    this.inventory = this.getInventory(product)
    this.min = this.minQty(priceSchedule)
    this.max = this.maxQty(priceSchedule)
    this.restrictedQuantities = priceSchedule.PriceBreaks.map((b) => b.Quantity)
    if (this.inventory < this.min) {
      this.errorMsg = 'Out of stock.'
      this.disabled = true
    }
    if (
      this.form.controls.quantity.value !== this.getDefaultQty() &&
      !this.gridDisplay
    ) {
      this.form.setValue({ quantity: this.getDefaultQty() })
    }
    if (this.gridDisplay) {
      this.form.controls['quantity'].setValue(0)
    }
    this.quantityChangeListener()
    if (!this.existingQty) {
      this.emit(this.form.get('quantity').value)
    }
    if (this.groupedLineItems && this.li) {
      // Filter through the lis down to ProductIDs matching the qty inputs li.Product.ID
      this.cumulativeQuantity = this.groupedLineItems
        .filter((li) => li.Product?.ID === this.li?.Product?.ID)
        .map((f) => f.Quantity)
        .reduce((acc, curr) => acc + curr)
      this.qtyPreUpdate = this.groupedLineItems.find(
        (li) => li.ID === this.li.ID
      ).Quantity
    }
  }

  quantityChangeListener(): void {
    this.form.valueChanges.subscribe(() => {
      this.emit(this.form.value.quantity)
    })
  }

  emit(qty: number): void {
    this.qtyChange.emit({ qty, valid: this.validateQty(qty) })
  }

  getMaxQty(): number {
    if (this.variantInventory && !this.product?.Inventory?.OrderCanExceed) {
      return this.variantInventory
    } else if (
      this.product?.Inventory?.Enabled &&
      !this.product?.Inventory?.OrderCanExceed &&
      !this.product?.Inventory?.VariantLevelTracking
    ) {
      return this.product?.Inventory?.QuantityAvailable
    }
  }

  validateQty(qty: number): boolean {
    const routeUrl = this.router.routerState.snapshot.url
    const splitUrl = routeUrl.split('/')
    const endUrl = splitUrl[splitUrl.length - 1]
    if (!endUrl.includes('cart')) {
      const productInCart = this.context.order.cart
        .get()
        ?.Items?.filter((i) => i.ProductID === this.product?.ID)
        ?.find((p) => p.Variant?.ID === this.selectedVariant?.ID)
      if (
        productInCart &&
        !this.product?.PriceSchedule?.UseCumulativeQuantity
      ) {
        if (qty + productInCart.Quantity > this.max) {
          this.errorMsg = `The maximum is ${this.max} and your cart has ${productInCart.Quantity}.`
          return false
        }
        qty = qty + productInCart.Quantity
      }
      if (productInCart && this.product?.PriceSchedule?.UseCumulativeQuantity) {
        if (qty + this.cumulativeQuantity > this.max) {
          this.errorMsg = `The maximum is ${this.max} and your cart has ${productInCart.Quantity}.`
          return false
        }
        qty = qty + this.cumulativeQuantity
      }
    }
    if (isNaN(qty)) {
      this.errorMsg = 'Please Enter a Quantity'
      return false
    }
    if (
      (!this.product?.PriceSchedule?.UseCumulativeQuantity && qty < this.min) ||
      qty > this.max
    ) {
      this.errorMsg = `Please order a quantity between ${this.min}-${this.max}.`
      return false
    }
    if (
      this.product?.PriceSchedule?.UseCumulativeQuantity &&
      (this.cumulativeQuantity - this.qtyPreUpdate + qty < this.min ||
        this.cumulativeQuantity - this.qtyPreUpdate + qty > this.max)
    ) {
      this.errorMsg = `Please order a total quantity between ${this.min}-${this.max}.`
      return false
    }
    if (qty > this.inventory) {
      this.errorMsg = `Only ${this.inventory} available in inventory.`
      return false
    }
    this.errorMsg = ''
    return true
  }

  getDefaultQty(): number {
    if (this.existingQty) return this.existingQty
    if (this.priceSchedule.RestrictedQuantity)
      return this.priceSchedule.PriceBreaks[0].Quantity
    return this.priceSchedule.MinQuantity
  }

  minQty(priceSchedule: PriceSchedule): number {
    return priceSchedule.MinQuantity || (this.gridDisplay ? 0 : 1)
  }

  maxQty(priceSchedule: PriceSchedule): number {
    return priceSchedule.MaxQuantity || Infinity
  }

  getInventory(product: HSMeProduct): number {
    if (product?.Inventory?.OrderCanExceed) return Infinity
    if (
      product.Inventory &&
      product.Inventory.Enabled &&
      !product.Inventory.OrderCanExceed &&
      product.Inventory.QuantityAvailable != null &&
      !product.Inventory.VariantLevelTracking
    ) {
      return product.Inventory.QuantityAvailable
    } else if (product.Inventory && product.Inventory.VariantLevelTracking) {
      return this.variantInventory
    }
    return Infinity
  }
}
