import { Component, Input } from '@angular/core'
import { FormGroup } from '@angular/forms'
import {
  HSLineItem,
  HSMeProduct,
  SuperHSProduct,
} from '@ordercloud/headstart-sdk'
import { BuyerProduct, PriceBreak, PriceSchedule, Spec } from 'ordercloud-javascript-sdk'
import { ProductDetailService } from '../product-details/product-detail.service'
import { SpecFormService } from '../spec-form/spec-form.service'
import { minBy as _minBy } from 'lodash'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { QtyChangeEvent, GridSpecOption } from 'src/app/models/product.types'

@Component({
  templateUrl: `./grid-spec-form.component.html`,
})
export class OCMGridSpecForm {
  _specs: Spec[]
  _specForm: FormGroup
  _superProduct: SuperHSProduct
  product: HSMeProduct
  specOptions: string[]
  lineItems: any[] = []
  lineTotals: number[] = []
  unitPrices: number[] = []
  totalPrice = 0
  isAddingToCart = false
  priceSchedule: PriceSchedule
  priceBreaks: PriceBreak[]
  price: number
  percentSavings: number
  totalQtyToAdd: number
  qtyValid = false
  resetGridQtyFields = false
  errorMsg = ''
  constructor(
    private specFormService: SpecFormService,
    private context: ShopperContextService,
    private productDetailService: ProductDetailService
  ) {}

  @Input() set superProduct(value: SuperHSProduct) {
    this._superProduct = value
    this.product = this._superProduct.Product
    this.priceBreaks = this._superProduct.PriceSchedule.PriceBreaks
    this.priceSchedule = this._superProduct.PriceSchedule
  }
  @Input() set specs(value: Spec[]) {
    this._specs = value
    this.getSpecOptions(value)
  }
  @Input() set specForm(value: FormGroup) {
    this._specForm = value
  }

  getSpecOptions(specs: Spec[]): void {
    // creates an object with each spec option and its values
    const obj = {}
    for (const spec of specs) {
      for (const option of spec.Options) {
        const name = spec.Name.replace(/ /g, '')
        if (obj[name]) obj[name].push(option.Value)
        else obj[name] = [option.Value]
      }
    }
    this.specOptions = this.getAllSpecCombinations(obj)
  }

  getAllSpecCombinations(specOptions: object): string[] {
    // returns an array containing every combination of spec values
    const arr = []
    for (const specName in specOptions) {
      if (specOptions.hasOwnProperty(specName)) {
        arr.push(specOptions[specName])
      }
    }
    if (arr.length === 1) return arr[0]
    else {
      const result = []
      const combinations = this.getAllSpecCombinations(arr.slice(1))
      for (const combination of combinations) {
        for (const optionValue of arr[0]) {
          result.push(optionValue + ', ' + combination)
        }
      }
      result.forEach(() => {
        this.lineTotals.push(0)
        this.unitPrices.push(0)
      })
      return result
    }
  }

  changeQuantity(specs: string, event: QtyChangeEvent): void {
    const indexOfSpec = this.specOptions.indexOf(specs)
    let specArray = specs.split(',')
    specArray = specArray.map((x) => x.replace(/\s/g, ''))
    const item = {
      Quantity: event.qty,
      Product: this.product,
      ProductID: this.product.ID,
      Specs: this.specFormService.getGridLineItemSpecs(this._specs, specArray),
      xp: {
        ImageUrl: this.specFormService.getGridLineItemImageUrl(
          this._superProduct.Product?.xp?.Images,
          this._specs,
          specArray
        ),
      },
    }
    const i = this.lineItems.findIndex(
      (li: HSLineItem) =>
        JSON.stringify(li.Specs) === JSON.stringify(item.Specs)
    )
    if (i === -1) this.lineItems.push(item)
    else this.lineItems[i] = item
    const liQuantities: number[] = []
    this.lineItems.forEach((li: HSLineItem) => liQuantities.push(li.Quantity))
    this.totalQtyToAdd = liQuantities.reduce((acc, curr) => {
      return acc + curr
    })
    this.qtyValid = this.validateQuantity(this.lineItems)
    if (this.priceSchedule?.UseCumulativeQuantity) {
      // Update all line item totals, to check for price breaks.
      this.lineItems.forEach((li: HSLineItem, i: number) => {
        this.lineTotals[i] = this.getLineTotal(
          li.Quantity,
          li.Specs as GridSpecOption[],
          this.totalQtyToAdd,
          this.priceSchedule?.UseCumulativeQuantity
        )
        this.unitPrices[i] = this.getUnitPrice(
          li.Quantity,
          li.Specs as GridSpecOption[],
          this.totalQtyToAdd,
          this.priceSchedule?.UseCumulativeQuantity
        )
      })
    } else {
      this.lineTotals[indexOfSpec] = this.getLineTotal(
        event.qty,
        item.Specs,
        this.totalQtyToAdd,
        this.priceSchedule?.UseCumulativeQuantity
      )
      this.unitPrices[indexOfSpec] = this.getUnitPrice(event.qty, item.Specs)
    }
    this.totalPrice = this.getTotalPrice()
  }

  validateQuantity(lineItems: HSLineItem[]): boolean {
    this.resetGridQtyFields = false
    this.errorMsg = ''
    const lineItemsOfCurrentProductInCart: HSLineItem[] = this.context.order.cart
      .get()
      ?.Items?.filter((i) => i.ProductID === this.product?.ID)
    const isInventoryAvailable = this.checkIfInventoryIsAvailable(
      lineItems,
      lineItemsOfCurrentProductInCart
    )
    if (!isInventoryAvailable) {
      return false
    }
    let qtyInCart = 0
    // When users can mix and match variants when evaluating quantity
    if (this.priceSchedule.UseCumulativeQuantity) {
      // When users have not reached the minimum quantity
      lineItemsOfCurrentProductInCart.forEach(
        (li: HSLineItem) => (qtyInCart += li.Quantity)
      )
      if (
        this.totalQtyToAdd + qtyInCart <
        (lineItems[0].Product as BuyerProduct).PriceSchedule.MinQuantity
      ) {
        this.errorMsg = `Minimum quantity not reached.  ${qtyInCart} in cart, ${
          (lineItems[0].Product as BuyerProduct).PriceSchedule.MinQuantity -
          qtyInCart
        } of any product option are needed.`
        return false
      }
      // When users have exceeded the maximum quantity
      if (
        (lineItems[0].Product as BuyerProduct).PriceSchedule.MaxQuantity !==
          null &&
        this.totalQtyToAdd + qtyInCart >
          (lineItems[0].Product as BuyerProduct).PriceSchedule.MaxQuantity
      ) {
        this.errorMsg = `Maximum quantity reached (${
          (lineItems[0].Product as BuyerProduct).PriceSchedule.MaxQuantity
        }).  ${qtyInCart} currently in cart.`
        return false
      }
    } else {
      // When quantity must be evaluated variant-by-variant
      for (const li of lineItems) {
        qtyInCart = 0
        const liSpecs = li.Specs.map((spec) => spec.Value)
        const matchingLineItem = lineItemsOfCurrentProductInCart.find(
          (li: HSLineItem) => {
            const specs = li.Specs.map((spec) => spec.Value)
            return (
              specs.length === liSpecs.length &&
              specs.every((spec) => liSpecs.includes(spec))
            )
          }
        )
        if (matchingLineItem) {
          qtyInCart = matchingLineItem.Quantity
        }
        if (
          li.Quantity !== 0 &&
          li.Quantity !== null &&
          li.Quantity + qtyInCart <
            (li.Product as BuyerProduct).PriceSchedule.MinQuantity
        ) {
          this.errorMsg = `Minimum quantity not reached.  ${qtyInCart} in cart, ${
            (li.Product as BuyerProduct).PriceSchedule.MinQuantity - qtyInCart
          } of this product option are needed.`
          return false
        }
        if (
          (li.Product as BuyerProduct).PriceSchedule.MaxQuantity !== null &&
          li.Quantity !== 0 &&
          li.Quantity !== null &&
          li.Quantity + qtyInCart >
            (li.Product as BuyerProduct).PriceSchedule.MaxQuantity
        ) {
          this.errorMsg = `Maximum quantity reached (${
            (li.Product as BuyerProduct).PriceSchedule.MaxQuantity
          }).  ${qtyInCart} currently in cart.`
          return false
        }
      }
    }
    return true
  }

  checkIfInventoryIsAvailable(
    lineItems: HSLineItem[],
    lineItemsOfCurrentProductInCart: HSLineItem[]
  ): boolean {
    // If not tracking inventory, return true
    if (
      this._superProduct.Product.Inventory === null ||
      !this._superProduct.Product.Inventory?.Enabled ||
      this._superProduct.Product.Inventory?.OrderCanExceed
    ) {
      return true
    }

    // If tracking inventory, but not at a variant level, compare Quantity Available to amount currently in cart and about to add to cart
    if (
      this._superProduct.Product.Inventory?.Enabled &&
      !this._superProduct.Product.Inventory?.VariantLevelTracking
    ) {
      let inventoryInCart = 0
      lineItemsOfCurrentProductInCart.forEach(
        (li) => (inventoryInCart += li.Quantity)
      )
      if (
        this._superProduct.Product.Inventory?.QuantityAvailable !== null &&
        this._superProduct.Product.Inventory?.QuantityAvailable -
          inventoryInCart -
          this.totalQtyToAdd <
          0
      ) {
        this.errorMsg = `Sorry, but there is not enough inventory for the amount you are trying to add.  You have ${inventoryInCart} currently in your cart, but only ${
          this._superProduct.Product.Inventory?.QuantityAvailable -
          inventoryInCart
        } more of all total product options may be added.`
        return false
      }
    }

    // If tracking inventory at a variant level, ensure line items in cart and about to be added do not exceed quantities available
    if (
      this._superProduct.Product.Inventory?.Enabled &&
      this._superProduct.Product.Inventory?.VariantLevelTracking
    ) {
      for (const li of lineItems) {
        const liSpecs = li.Specs.map((spec) => spec.Value)
        const matchingVariant = this._superProduct.Variants.find((variant) => {
          const variantSpecs = variant.Specs.map((spec) => spec.Value)
          return (
            variantSpecs.length === liSpecs.length &&
            variantSpecs.every((spec) => liSpecs.includes(spec))
          )
        })
        if (matchingVariant.Inventory?.QuantityAvailable === null) {
          return false
        }
        const matchingLineItemInCart = lineItemsOfCurrentProductInCart.find(
          (liInCart) => {
            const liInCartSpecs = liInCart.Specs.map((spec) => spec.Value)
            return (
              liInCartSpecs.length === liSpecs.length &&
              liInCartSpecs.every((spec) => liSpecs.includes(spec))
            )
          }
        )
        if (matchingLineItemInCart) {
          if (
            matchingVariant.Inventory.QuantityAvailable -
              matchingLineItemInCart.Quantity -
              li.Quantity <
            0
          ) {
            this.errorMsg = `Sorry, but there is not enough inventory for the amount you are trying to add.  You have ${
              matchingLineItemInCart.Quantity
            } currently in your cart, but only ${
              matchingVariant.Inventory.QuantityAvailable -
              matchingLineItemInCart.Quantity
            } more of this product option may be added.`
            return false
          }
        } else {
          if (matchingVariant.Inventory.QuantityAvailable - li.Quantity < 0) {
            this.errorMsg = `Sorry, but there is not enough inventory for the amount you are trying to add.  Only ${matchingVariant.Inventory.QuantityAvailable} of this product option may be added.`
            return false
          }
        }
      }
    }
    return true
  }

  getUnitPrice(
    qty: number,
    specs: GridSpecOption[],
    totalQtyToAdd?: number,
    useCumulativeQty?: boolean
  ): number {
    if (!this.priceBreaks?.length) return
    const lineItemsOfCurrentProductInCartQty: number = this.context.order.cart
      .get()
      ?.Items?.filter((i) => i.ProductID === this.product?.ID)
      ?.map((li) => li.Quantity)
      ?.reduce((acc, curr) => acc + curr, 0)
    const quantityOrCumulativeQuantity = useCumulativeQty
      ? totalQtyToAdd + lineItemsOfCurrentProductInCartQty
      : qty
    const startingBreak = _minBy(this.priceBreaks, 'Quantity')
    const selectedBreak = this.priceBreaks.reduce((current, candidate) => {
      return candidate.Quantity > current.Quantity &&
        candidate.Quantity <= quantityOrCumulativeQuantity
        ? candidate
        : current
    }, startingBreak)
    let totalMarkup = 0
    specs.forEach((spec) => (totalMarkup += spec.Markup))
    return selectedBreak.Price + totalMarkup
  }

  getLineTotal(
    qty: number,
    specs: GridSpecOption[],
    totalQtyToAdd?: number,
    useCumulativeQty?: boolean
  ): number {
    const quantityOrCumulativeQuantity = useCumulativeQty ? totalQtyToAdd : qty
    if (qty > 0) {
      if (this.priceBreaks?.length) {
        const basePrice =
          quantityOrCumulativeQuantity * this.priceBreaks[0].Price
        this.percentSavings = this.productDetailService.getPercentSavings(
          this.price,
          basePrice
        )
      }
      return this.productDetailService.getGridLineItemPrice(
        this.priceBreaks,
        specs,
        qty,
        totalQtyToAdd
      )
    }
    return 0
  }

  async addToCart(): Promise<void> {
    const lineItems = this.lineItems.filter((li: HSLineItem) => li.Quantity > 0)
    try {
      this.isAddingToCart = true
      await this.context.order.cart.addMany(lineItems)
      this.resetGridQtyFields = true
    } catch (ex) {
      this.isAddingToCart = false
      throw ex
    }
    this.isAddingToCart = false
    this.lineItems = []
  }

  getTotalPrice(): number {
    return this.lineTotals.reduce((acc, curr) => {
      return acc + curr
    })
  }
}
