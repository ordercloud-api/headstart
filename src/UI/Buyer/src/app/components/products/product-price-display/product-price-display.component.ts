import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
} from '@angular/core'
import { FormGroup } from '@angular/forms'
import { PriceSchedule, Spec } from 'ordercloud-javascript-sdk'
import { ProductDetailService } from '../product-details/product-detail.service'

@Component({
  selector: 'app-product-price-display',
  templateUrl: './product-price-display.component.html',
  styleUrls: ['./product-price-display.component.scss'],
})
export class ProductPriceDisplayComponent implements OnChanges {
  @Input() priceSchedule: PriceSchedule
  @Input() quantity = 1
  @Input() specs?: Spec[] = []
  @Input() specForm?: FormGroup
  isOnSale: boolean
  price: number
  salePrice: number
  constructor(private productDetailService: ProductDetailService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.quantity) {
      this.isOnSale = this.priceSchedule?.IsOnSale
      this.price = this.productDetailService.getProductUnitPrice(
        this.priceSchedule,
        this.specs,
        this.quantity,
        this.specForm,
        false
      )
      this.salePrice = this.productDetailService.getProductUnitPrice(
        this.priceSchedule,
        this.specs,
        this.quantity,
        this.specForm,
        true
      )
    }
  }
}
