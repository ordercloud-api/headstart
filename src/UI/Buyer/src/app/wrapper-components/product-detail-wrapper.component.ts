import { Component, OnInit } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { SuperHSProduct } from '@ordercloud/headstart-sdk'
import { CurrentUserService } from '../services/current-user/current-user.service'

@Component({
  template: `
    <ocm-product-details [product]="product"> </ocm-product-details>
  `,
})
export class ProductDetailWrapperComponent implements OnInit {
  product: SuperHSProduct
  constructor(
    private activatedRoute: ActivatedRoute,
    protected currentUser: CurrentUserService
  ) {}

  ngOnInit(): void {
    this.product = this.activatedRoute.snapshot.data.product
  }
}
