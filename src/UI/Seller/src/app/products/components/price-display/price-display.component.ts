import { Component, Input } from '@angular/core'
import { PriceSchedule } from 'ordercloud-javascript-sdk'

@Component({
  selector: 'app-price-display',
  templateUrl: './price-display.component.html',
  styleUrls: ['./price-display.component.scss'],
})
export class PriceDisplayComponent {
  @Input() priceSchedule: PriceSchedule
  constructor() {}

  get isOnSale(): boolean {
    return this.priceSchedule?.IsOnSale
  }

  get price(): number {
    const priceBreaks = this.priceSchedule?.PriceBreaks
    if (!priceBreaks) {
      return
    }
    return priceBreaks[0].Price
  }

  get salePrice(): number {
    const priceBreaks = this.priceSchedule?.PriceBreaks
    if (!priceBreaks) {
      return
    }
    return priceBreaks[0].SalePrice
  }
}
