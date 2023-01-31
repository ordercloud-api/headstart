import { Injectable } from '@angular/core'
import { UntypedFormGroup } from '@angular/forms'
import { PriceBreak, PriceSchedule, Spec } from 'ordercloud-javascript-sdk'
import { minBy as _minBy } from 'lodash'
import { SpecFormService } from '../spec-form/spec-form.service'
import { GridSpecOption, LineItemToAdd } from 'src/app/models/product.types'

@Injectable({
  providedIn: 'root',
})
export class ProductDetailService {
  constructor(private specFormService: SpecFormService) {}

  getProductPrice(
    priceSchedule: PriceSchedule,
    quantity: number,
    specs: Spec[],
    specForm: UntypedFormGroup,
    isOnSale: boolean
  ): number {
    const unitPrice = this.getProductUnitPrice(
      priceSchedule,
      specs,
      quantity,
      specForm,
      isOnSale
    )
    return quantity * unitPrice
  }

  getProductUnitPrice(
    priceSchedule: PriceSchedule,
    specs: Spec[],
    quantity: number,
    specForm: UntypedFormGroup,
    isOnSale: boolean
  ): number {
    // In OC, the price per item can depend on the quantity ordered. This info is stored on the PriceSchedule as a list of PriceBreaks.
    // Find the PriceBreak with the highest Quantity less than the quantity ordered. The price on that price break
    // is the cost per item.
    const priceBreaks = priceSchedule?.PriceBreaks
    if (!priceSchedule?.PriceBreaks?.length) return 0
    const startingBreak = _minBy(priceBreaks, 'Quantity')
    const selectedBreak = priceBreaks.reduce((current, candidate) => {
      return candidate.Quantity > current.Quantity &&
        candidate.Quantity <= quantity
        ? candidate
        : current
    }, startingBreak)

    // Take into account spec markups if they are applied which can increase price
    const markupUnitPrice = specForm?.valid
      ? this.specFormService.getSpecMarkupUnit(
          specs,
          selectedBreak,
          quantity,
          specForm
        )
      : 0

    if (isOnSale) {
      return selectedBreak.SalePrice + markupUnitPrice
    } else {
      return selectedBreak.Price + markupUnitPrice
    }
  }

  getGridLineItemPrice(
    priceBreaks: PriceBreak[],
    specs: GridSpecOption[],
    quantity: number,
    totalQtyOfItem?: number
  ): number {
    if (!priceBreaks?.length) return
    const startingBreak = _minBy(priceBreaks, 'Quantity')
    const selectedBreak = priceBreaks.reduce((current, candidate) => {
      return candidate.Quantity > current.Quantity &&
        candidate.Quantity <= (totalQtyOfItem || quantity)
        ? candidate
        : current
    }, startingBreak)
    let totalMarkup = 0
    specs.forEach((spec) => (totalMarkup += spec.Markup))

    // Take into account markups if they are applied which can increase price
    return (selectedBreak.Price + totalMarkup) * quantity
  }

  getPercentSavings(actualPrice: number, basePrice: number): number {
    if (actualPrice === null || actualPrice === undefined) {
      return 0
    }
    if (basePrice === null || basePrice === undefined) {
      return 0
    }
    return parseInt(
      (((basePrice - actualPrice) / basePrice) * 100).toFixed(0),
      10
    )
  }

  isSameLine(line: LineItemToAdd, other: LineItemToAdd): boolean {
    return (
      line.ProductID === other.ProductID &&
      this.isObjectEqual(line.Specs, other.Specs)
    )
  }

  isSameLineAsOthers(line: LineItemToAdd, others: LineItemToAdd[]): boolean {
    return others.some((other) => {
      return (
        line.ProductID === other.ProductID &&
        this.isObjectEqual(line.Specs, other.Specs)
      )
    })
  }

  isObjectEqual(obj1: any, obj2: any): boolean {
    const sorted1 = this.sortObjectByKeys(obj1)
    const sorted2 = this.sortObjectByKeys(obj2)
    if (JSON.stringify(sorted1) === JSON.stringify(sorted2)) {
      return true
    }
  }

  private sortObjectByKeys(obj: any): any[] {
    return Object.entries(obj).sort(([a], [b]) => (a < b ? -1 : 1))
  }
}
