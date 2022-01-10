import { Component, EventEmitter, Input, Output } from '@angular/core'
import { FormGroup } from '@angular/forms'
import { HSBogoType, PromotionXp } from '@app-seller/shared'
import { Product, Promotion } from 'ordercloud-javascript-sdk'
import { BehaviorSubject } from 'rxjs'
import { ProductXp } from '../../../../../../SDK/dist'

@Component({
  selector: 'promotion-bogo-edit',
  templateUrl: './bogo-edit.component.html',
  styleUrls: ['../promotion-edit/promotion-edit.component.scss'],
})
export class BogoEditComponent {
  @Input() set promotionEditable(promotion: Promotion<PromotionXp>) {
    this._promotionEditable = promotion
  }
  @Input() buyProductsCollapsed: boolean
  @Input() getProductsCollapsed: boolean
  @Input() formGroup: FormGroup
  @Input() _products: BehaviorSubject<Product[]>
  @Input() selectedBuySKU: Product<ProductXp>
  @Input() selectedGetSKU: Product<ProductXp>
  @Output() refreshPromoData = new EventEmitter<boolean>(false)
  @Output() scrollEnded = new EventEmitter<any>()
  @Output() searchProducts = new EventEmitter<any>()
  @Output() buySKUSelected = new EventEmitter<string>(null)
  @Output() getSKUSelected = new EventEmitter<string>(null)
  @Output() toggleProductsCollapse = new EventEmitter<string>(null)
  @Output() promoUpdated = new EventEmitter<{
    event: any
    path: string
    valueType?: string
  }>()
  _promotionEditable: Promotion<PromotionXp>

  selectBuySKU(id: string): void {
    this.buySKUSelected.emit(id)
  }

  selectGetSKU(id: string): void {
    this.getSKUSelected.emit(id)
  }

  BOGOPromoTypeCheck(type: HSBogoType): boolean {
    return type === this._promotionEditable?.xp?.BOGO?.Type
  }

  handleUpdatePromo(e: any, p: string, type?: string): void {
    this.promoUpdated.emit({ event: e, path: p, valueType: type })
  }

  handleScrollEnd(e: any): void {
    this.scrollEnded.emit(e)
  }

  toggleBOGOProductsCollapse(buyOrGet: string): void {
    this.toggleProductsCollapse.emit(buyOrGet)
  }

  searchedResources(e: any): void {
    this.searchProducts.emit(e)
  }
}
