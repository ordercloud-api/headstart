import { Input, Component } from '@angular/core'
import { Supplier } from 'ordercloud-javascript-sdk'
import { DomSanitizer, SafeHtml } from '@angular/platform-browser'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './supplier-card.component.html',
  styleUrls: ['./supplier-card.component.scss'],
})
export class OCMSupplierCard {
  _supplier: Supplier
  @Input() set supplier(s: Supplier) {
    this._supplier = s
    this.logoUrl = s.xp?.Image?.ThumbnailUrl || '/assets/supplier.jpg'
  }
  logoUrl = ''
  constructor(
    private context: ShopperContextService,
    private domSanitizer: DomSanitizer
  ) {}

  shopSupplier(supplier: Supplier): void {
    this.context.router.toProductList({
      activeFacets: { supplier: supplier.Name.toLocaleLowerCase() },
    })
  }
  getSupplierDescription(): SafeHtml {
    if (this._supplier && this._supplier?.xp) {
      //In order to have css styles show from the Description (since it's html in string form),
      //we need to tell Angular to bypass sanitation of the html.
      return this.domSanitizer.bypassSecurityTrustHtml(
        this._supplier.xp.Description
      )
    }
  }
}
