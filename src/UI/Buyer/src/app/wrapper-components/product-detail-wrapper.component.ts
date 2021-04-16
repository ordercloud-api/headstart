import { Component, OnInit } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { SuperHSProduct, HSKitProduct } from '@ordercloud/headstart-sdk'
import { CurrentUserService } from '../services/current-user/current-user.service'

@Component({
  template: `
    <ocm-product-details [product]="product">
    </ocm-product-details>
  `,
})
export class ProductDetailWrapperComponent implements OnInit {
  product: SuperHSProduct
  constructor(
    private activatedRoute: ActivatedRoute,
    protected currentUser: CurrentUserService
  ) {}

  ngOnInit(): void {
    const product = this.activatedRoute.snapshot.data.product
    this.product = product
  }
}
